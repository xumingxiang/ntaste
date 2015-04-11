namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.neighborhood;
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public class GenericUserBasedRecommender : AbstractRecommender, UserBasedRecommender, Recommender, Refreshable
    {
        private EstimatedPreferenceCapper capper;
        private static Logger log = LoggerFactory.getLogger(typeof(GenericUserBasedRecommender));
        private UserNeighborhood neighborhood;
        private RefreshHelper refreshHelper;
        private UserSimilarity similarity;

        public GenericUserBasedRecommender(DataModel dataModel, UserNeighborhood neighborhood, UserSimilarity similarity)
            : base(dataModel)
        {
            Action refreshRunnable = null;
            this.neighborhood = neighborhood;
            this.similarity = similarity;
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.capper = this.buildCapper();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(dataModel);
            this.refreshHelper.addDependency(similarity);
            this.refreshHelper.addDependency(neighborhood);
            this.capper = this.buildCapper();
        }

        private EstimatedPreferenceCapper buildCapper()
        {
            DataModel model = this.getDataModel();
            if (float.IsNaN(model.getMinPreference()) && float.IsNaN(model.getMaxPreference()))
            {
                return null;
            }
            return new EstimatedPreferenceCapper(model);
        }

        protected virtual float doEstimatePreference(long theUserID, long[] theNeighborhood, long itemID)
        {
            if (theNeighborhood.Length == 0)
            {
                return float.NaN;
            }
            DataModel model = this.getDataModel();
            double num = 0.0;
            double num2 = 0.0;
            int num3 = 0;
            foreach (long num4 in theNeighborhood)
            {
                if (num4 != theUserID)
                {
                    float? nullable = model.getPreferenceValue(num4, itemID);
                    if (nullable.HasValue)
                    {
                        double d = this.similarity.userSimilarity(theUserID, num4);
                        if (!double.IsNaN(d))
                        {
                            num += d * ((double)nullable.Value);
                            num2 += d;
                            num3++;
                        }
                    }
                }
            }
            if (num3 <= 1)
            {
                return float.NaN;
            }
            float estimate = (float)(num / num2);
            if (this.capper != null)
            {
                estimate = this.capper.capEstimate(estimate);
            }
            return estimate;
        }

        private long[] doMostSimilarUsers(int howMany, TopItems.Estimator<long> estimator)
        {
            DataModel model = this.getDataModel();
            return TopItems.getTopUsers(howMany, model.getUserIDs(), null, estimator);
        }

        public override float estimatePreference(long userID, long itemID)
        {
            float? nullable = this.getDataModel().getPreferenceValue(userID, itemID);
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
            long[] theNeighborhood = this.neighborhood.getUserNeighborhood(userID);
            return this.doEstimatePreference(userID, theNeighborhood, itemID);
        }

        protected FastIDSet getAllOtherItems(long[] theNeighborhood, long theUserID)
        {
            DataModel model = this.getDataModel();
            FastIDSet set = new FastIDSet();
            foreach (long num in theNeighborhood)
            {
                set.addAll(model.getItemIDsFromUser(num));
            }
            set.removeAll(model.getItemIDsFromUser(theUserID));
            return set;
        }

        public UserSimilarity getSimilarity()
        {
            return this.similarity;
        }

        public virtual long[] mostSimilarUserIDs(long userID, int howMany)
        {
            return this.mostSimilarUserIDs(userID, howMany, null);
        }

        public virtual long[] mostSimilarUserIDs(long userID, int howMany, Rescorer<Tuple<long, long>> rescorer)
        {
            TopItems.Estimator<long> estimator = new MostSimilarEstimator(userID, this.similarity, rescorer);
            return this.doMostSimilarUsers(howMany, estimator);
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            log.debug("Recommending items for user ID '{}'", new object[] { userID });
            long[] theNeighborhood = this.neighborhood.getUserNeighborhood(userID);
            if (theNeighborhood.Length == 0)
            {
                return new List<RecommendedItem>();
            }
            FastIDSet set = this.getAllOtherItems(theNeighborhood, userID);
            TopItems.Estimator<long> estimator = new Estimator(this, userID, theNeighborhood);
            List<RecommendedItem> list = TopItems.getTopItems(howMany, set.GetEnumerator(), rescorer, estimator);
            log.debug("Recommendations are: {}", new object[] { list });
            return list;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        public override string ToString()
        {
            return ("GenericUserBasedRecommender[neighborhood:" + this.neighborhood + ']');
        }

        private sealed class Estimator : TopItems.Estimator<long>
        {
            private GenericUserBasedRecommender r;
            private long[] theNeighborhood;
            private long theUserID;

            internal Estimator(GenericUserBasedRecommender r, long theUserID, long[] theNeighborhood)
            {
                this.r = r;
                this.theUserID = theUserID;
                this.theNeighborhood = theNeighborhood;
            }

            public double estimate(long itemID)
            {
                return (double)this.r.doEstimatePreference(this.theUserID, this.theNeighborhood, itemID);
            }
        }

        private sealed class MostSimilarEstimator : TopItems.Estimator<long>
        {
            private Rescorer<Tuple<long, long>> rescorer;
            private UserSimilarity similarity;
            private long toUserID;

            internal MostSimilarEstimator(long toUserID, UserSimilarity similarity, Rescorer<Tuple<long, long>> rescorer)
            {
                this.toUserID = toUserID;
                this.similarity = similarity;
                this.rescorer = rescorer;
            }

            public double estimate(long userID)
            {
                if (userID == this.toUserID)
                {
                    return double.NaN;
                }
                if (this.rescorer == null)
                {
                    return this.similarity.userSimilarity(this.toUserID, userID);
                }
                Tuple<long, long> thing = new Tuple<long, long>(this.toUserID, userID);
                if (this.rescorer.isFiltered(thing))
                {
                    return double.NaN;
                }
                double originalScore = this.similarity.userSimilarity(this.toUserID, userID);
                return this.rescorer.rescore(thing, originalScore);
            }
        }
    }
}