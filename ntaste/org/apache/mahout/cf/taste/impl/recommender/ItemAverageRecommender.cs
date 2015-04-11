namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System;
    using System.Collections.Generic;

    public sealed class ItemAverageRecommender : AbstractRecommender
    {
        private FastByIDMap<RunningAverage> itemAverages;
        private static Logger log = LoggerFactory.getLogger(typeof(ItemAverageRecommender));
        private RefreshHelper refreshHelper;

        public ItemAverageRecommender(DataModel dataModel)
            : base(dataModel)
        {
            Action refreshRunnable = null;
            this.itemAverages = new FastByIDMap<RunningAverage>();
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.buildAverageDiffs();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(dataModel);
            this.buildAverageDiffs();
        }

        private void buildAverageDiffs()
        {
            lock (this)
            {
                DataModel model = this.getDataModel();
                IEnumerator<long> enumerator = model.getUserIDs();
                while (enumerator.MoveNext())
                {
                    PreferenceArray array = model.getPreferencesFromUser(enumerator.Current);
                    int num = array.length();
                    for (int i = 0; i < num; i++)
                    {
                        long key = array.getItemID(i);
                        RunningAverage average = this.itemAverages.get(key);
                        if (average == null)
                        {
                            average = new FullRunningAverage();
                            this.itemAverages.put(key, average);
                        }
                        average.addDatum((double)array.getValue(i));
                    }
                }
            }
        }

        private float doEstimatePreference(long itemID)
        {
            lock (this)
            {
                RunningAverage average = this.itemAverages.get(itemID);
                return ((average == null) ? float.NaN : ((float)average.getAverage()));
            }
        }

        public override float estimatePreference(long userID, long itemID)
        {
            float? nullable = this.getDataModel().getPreferenceValue(userID, itemID);
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
            return this.doEstimatePreference(itemID);
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            log.debug("Recommending items for user ID '{}'", new object[] { userID });
            PreferenceArray preferencesFromUser = this.getDataModel().getPreferencesFromUser(userID);
            FastIDSet set = this.getAllOtherItems(userID, preferencesFromUser);
            TopItems.Estimator<long> estimator = new Estimator(this);
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
            }
        }

        public override string ToString()
        {
            return "ItemAverageRecommender";
        }

        private sealed class Estimator : TopItems.Estimator<long>
        {
            private ItemAverageRecommender r;

            internal Estimator(ItemAverageRecommender r)
            {
                this.r = r;
            }

            public double estimate(long itemID)
            {
                return (double)this.r.doEstimatePreference(itemID);
            }
        }
    }
}