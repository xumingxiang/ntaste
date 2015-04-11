namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System;
    using System.Collections.Generic;

    public sealed class ItemUserAverageRecommender : AbstractRecommender
    {
        private FastByIDMap<RunningAverage> itemAverages;
        private static Logger log = LoggerFactory.getLogger(typeof(ItemUserAverageRecommender));
        private RunningAverage overallAveragePrefValue;
        private RefreshHelper refreshHelper;
        private FastByIDMap<RunningAverage> userAverages;

        public ItemUserAverageRecommender(DataModel dataModel)
            : base(dataModel)
        {
            Action refreshRunnable = null;
            this.itemAverages = new FastByIDMap<RunningAverage>();
            this.userAverages = new FastByIDMap<RunningAverage>();
            this.overallAveragePrefValue = new FullRunningAverage();
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.buildAverageDiffs();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(dataModel);
            this.buildAverageDiffs();
        }

        private static void addDatumAndCreateIfNeeded(long itemID, float value, FastByIDMap<RunningAverage> averages)
        {
            RunningAverage average = averages.get(itemID);
            if (average == null)
            {
                average = new FullRunningAverage();
                averages.put(itemID, average);
            }
            average.addDatum((double)value);
        }

        private void buildAverageDiffs()
        {
            lock (this)
            {
                DataModel model = this.getDataModel();
                IEnumerator<long> enumerator = model.getUserIDs();
                while (enumerator.MoveNext())
                {
                    long current = enumerator.Current;
                    PreferenceArray array = model.getPreferencesFromUser(current);
                    int num2 = array.length();
                    for (int i = 0; i < num2; i++)
                    {
                        long itemID = array.getItemID(i);
                        float num5 = array.getValue(i);
                        addDatumAndCreateIfNeeded(itemID, num5, this.itemAverages);
                        addDatumAndCreateIfNeeded(current, num5, this.userAverages);
                        this.overallAveragePrefValue.addDatum((double)num5);
                    }
                }
            }
        }

        private float doEstimatePreference(long userID, long itemID)
        {
            lock (this)
            {
                RunningAverage average = this.itemAverages.get(itemID);
                if (average == null)
                {
                    return float.NaN;
                }
                RunningAverage average2 = this.userAverages.get(userID);
                if (average2 == null)
                {
                    return float.NaN;
                }
                double num = average2.getAverage() - this.overallAveragePrefValue.getAverage();
                return (float)(average.getAverage() + num);
            }
        }

        public override float estimatePreference(long userID, long itemID)
        {
            float? nullable = this.getDataModel().getPreferenceValue(userID, itemID);
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
            return this.doEstimatePreference(userID, itemID);
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            log.debug("Recommending items for user ID '{}'", new object[] { userID });
            PreferenceArray preferencesFromUser = this.getDataModel().getPreferencesFromUser(userID);
            FastIDSet set = this.getAllOtherItems(userID, preferencesFromUser);
            TopItems.Estimator<long> estimator = new Estimator(this, userID);
            List<RecommendedItem> list = TopItems.getTopItems(howMany, set.GetEnumerator(), rescorer, estimator);
            log.debug("Recommendations are: {}", new object[] { list });
            return list;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        public override void removePreference(long userID, long itemID)
        {
            float? nullable = this.getDataModel().getPreferenceValue(userID, itemID);
            base.removePreference(userID, itemID);
            if (nullable.HasValue)
            {
                lock (this)
                {
                    RunningAverage average = this.itemAverages.get(itemID);
                    if (average == null)
                    {
                        throw new InvalidOperationException("No preferences exist for item ID: " + itemID);
                    }
                    average.removeDatum((double)nullable.Value);
                    RunningAverage average2 = this.userAverages.get(userID);
                    if (average2 == null)
                    {
                        throw new InvalidOperationException("No preferences exist for user ID: " + userID);
                    }
                    average2.removeDatum((double)nullable.Value);
                    this.overallAveragePrefValue.removeDatum((double)nullable.Value);
                }
            }
        }

        public override void setPreference(long userID, long itemID, float value)
        {
            double num;
            DataModel model = this.getDataModel();
            try
            {
                float? nullable = model.getPreferenceValue(userID, itemID);
                num = !nullable.HasValue ? ((double)value) : ((double)(value - nullable.Value));
            }
            catch (NoSuchUserException)
            {
                num = value;
            }
            base.setPreference(userID, itemID, value);
            lock (this)
            {
                RunningAverage average = this.itemAverages.get(itemID);
                if (average == null)
                {
                    RunningAverage average2 = new FullRunningAverage();
                    average2.addDatum(num);
                    this.itemAverages.put(itemID, average2);
                }
                else
                {
                    average.changeDatum(num);
                }
                RunningAverage average3 = this.userAverages.get(userID);
                if (average3 == null)
                {
                    RunningAverage average4 = new FullRunningAverage();
                    average4.addDatum(num);
                    this.userAverages.put(userID, average4);
                }
                else
                {
                    average3.changeDatum(num);
                }
                this.overallAveragePrefValue.changeDatum(num);
            }
        }

        public override string ToString()
        {
            return "ItemUserAverageRecommender";
        }

        private sealed class Estimator : TopItems.Estimator<long>
        {
            private ItemUserAverageRecommender itemUserAverageRecommender;
            private long userID;

            internal Estimator(ItemUserAverageRecommender itemUserAverageRecommender, long userID)
            {
                this.userID = userID;
                this.itemUserAverageRecommender = itemUserAverageRecommender;
            }

            public double estimate(long itemID)
            {
                return (double)this.itemUserAverageRecommender.doEstimatePreference(this.userID, itemID);
            }
        }
    }
}