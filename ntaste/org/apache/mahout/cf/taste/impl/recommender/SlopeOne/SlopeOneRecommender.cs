using org.apache.mahout.cf.taste.common;
using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.model;
using org.apache.mahout.cf.taste.recommender;
using org.apache.mahout.cf.taste.recommender.slopeone;
using System;
using System.Collections.Generic;

namespace org.apache.mahout.cf.taste.impl.recommender.slopeone
{
    public sealed class SlopeOneRecommender : AbstractRecommender
    {
        private static Logger log = LoggerFactory.getLogger(typeof(SlopeOneRecommender));

        private bool weighted;
        private bool stdDevWeighted;
        private DiffStorage diffStorage;

        public SlopeOneRecommender(DataModel dataModel) :
            this(dataModel,
                Weighting.WEIGHTED,
                Weighting.WEIGHTED,
                new MemoryDiffStorage(dataModel, Weighting.WEIGHTED, long.MaxValue)) { }

        public SlopeOneRecommender(
            DataModel dataModel,
            Weighting weighting,
            Weighting stdDevWeighting, DiffStorage diffStorage)
            : base(dataModel)
        {
            //if (stdDevWeighting != Weighting.WEIGHTED)

            //Preconditions.checkArgument(stdDevWeighting != Weighting.WEIGHTED || weighting != Weighting.UNWEIGHTED,
            //  "weighted required when stdDevWeighted is set");
            //Preconditions.checkArgument(diffStorage != null, "diffStorage is null");
            this.weighted = weighting == Weighting.WEIGHTED;
            this.stdDevWeighted = stdDevWeighting == Weighting.WEIGHTED;
            this.diffStorage = diffStorage;
        }

        public override float estimatePreference(long userID, long itemID)
        {
            DataModel model = getDataModel();
            float? actualPref = model.getPreferenceValue(userID, itemID);
            if (actualPref != null)
            {
                return actualPref.Value;
            }
            return doEstimatePreference(userID, itemID);
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, taste.recommender.IDRescorer rescorer)
        {
            //Preconditions.checkArgument(howMany >= 1, "howMany must be at least 1");
            log.debug("Recommending items for user ID '{}'", userID);

            FastIDSet possibleItemIDs = diffStorage.getRecommendableItemIDs(userID);

            TopItems.Estimator<long> estimator = new Estimator(this, userID);

            List<RecommendedItem> topItems = TopItems.getTopItems(howMany, possibleItemIDs.GetEnumerator(), rescorer, estimator);

            log.debug("Recommendations are: {}", topItems);
            return topItems;
        }

        public override void refresh(IList<taste.common.Refreshable> alreadyRefreshed)
        {
            alreadyRefreshed = RefreshHelper.buildRefreshed(alreadyRefreshed);
            RefreshHelper.maybeRefresh(alreadyRefreshed, diffStorage);
        }

        public override void setPreference(long userID, long itemID, float value)
        {
            DataModel dataModel = getDataModel();
            float? oldPref;
            try
            {
                oldPref = dataModel.getPreferenceValue(userID, itemID);
            }
            catch (NoSuchUserException nsee)
            {
                oldPref = null;
            }
            base.setPreference(userID, itemID, value);
            if (oldPref == null)
            {
                // Add new preference
                diffStorage.addItemPref(userID, itemID, value);
            }
            else
            {
                // Update preference
                diffStorage.updateItemPref(itemID, value - oldPref.Value);
            }
        }

        public override void removePreference(long userID, long itemID)
        {
            DataModel dataModel = getDataModel();
            float? oldPref = dataModel.getPreferenceValue(userID, itemID);
            base.removePreference(userID, itemID);
            if (oldPref != null)
            {
                diffStorage.removeItemPref(userID, itemID, oldPref.Value);
            }
        }

        public override string ToString()
        {
            return "SlopeOneRecommender[weighted:" + weighted + ", stdDevWeighted:" + stdDevWeighted
                   + ", diffStorage:" + diffStorage + ']';
        }

        private float doEstimatePreference(long userID, long itemID)
        {
            double count = 0.0;
            double totalPreference = 0.0;
            PreferenceArray prefs = getDataModel().getPreferencesFromUser(userID);
            RunningAverage[] averages = diffStorage.getDiffs(userID, itemID, prefs);
            int size = prefs.length();
            for (int i = 0; i < size; i++)
            {
                RunningAverage averageDiff = averages[i];
                if (averageDiff != null)
                {
                    double averageDiffValue = averageDiff.getAverage();
                    if (weighted)
                    {
                        double weight = averageDiff.getCount();
                        if (stdDevWeighted)
                        {
                            var raaStdDev = averageDiff as RunningAverageAndStdDev;
                            if (raaStdDev != null)
                            {
                                double stdev = raaStdDev.getStandardDeviation();
                                if (!Double.IsNaN(stdev))
                                {
                                    weight /= 1.0 + stdev;
                                }
                            }

                            // If stdev is NaN, then it is because count is 1. Because we're weighting by count,
                            // the weight is already relatively low. We effectively assume stdev is 0.0 here and
                            // that is reasonable enough. Otherwise, dividing by NaN would yield a weight of NaN
                            // and disqualify this pref entirely
                            // (Thanks Daemmon)
                        }
                        totalPreference += weight * (prefs.getValue(i) + averageDiffValue);
                        count += weight;
                    }
                    else
                    {
                        totalPreference += prefs.getValue(i) + averageDiffValue;
                        count += 1.0;
                    }
                }
            }
            if (count <= 0.0)
            {
                RunningAverage itemAverage = diffStorage.getAverageItemPref(itemID);
                return itemAverage == null ? float.NaN : (float)itemAverage.getAverage();
            }
            else
            {
                return (float)(totalPreference / count);
            }
        }

        private sealed class Estimator : TopItems.Estimator<long>
        {
            private SlopeOneRecommender r;
            private long userID;

            public Estimator(SlopeOneRecommender _r, long userID)
            {
                this.userID = userID;
                this.r = _r;
            }

            public double estimate(long itemID)
            {
                return this.r.doEstimatePreference(userID, itemID);
            }
        }
    }
}