namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public class GenericItemBasedRecommender : AbstractRecommender, ItemBasedRecommender, Recommender, Refreshable
    {
        private EstimatedPreferenceCapper capper;
        private static bool EXCLUDE_ITEM_IF_NOT_SIMILAR_TO_ALL_BY_DEFAULT = true;
        private static Logger log = LoggerFactory.getLogger(typeof(GenericItemBasedRecommender));
        private MostSimilarItemsCandidateItemsStrategy mostSimilarItemsCandidateItemsStrategy;
        private RefreshHelper refreshHelper;
        private ItemSimilarity similarity;

        public GenericItemBasedRecommender(DataModel dataModel, ItemSimilarity similarity)
            : this(dataModel, similarity, AbstractRecommender.getDefaultCandidateItemsStrategy(), getDefaultMostSimilarItemsCandidateItemsStrategy())
        {
        }

        public GenericItemBasedRecommender(DataModel dataModel, ItemSimilarity similarity, CandidateItemsStrategy candidateItemsStrategy, MostSimilarItemsCandidateItemsStrategy mostSimilarItemsCandidateItemsStrategy)
            : base(dataModel, candidateItemsStrategy)
        {
            Action refreshRunnable = null;
            this.similarity = similarity;
            this.mostSimilarItemsCandidateItemsStrategy = mostSimilarItemsCandidateItemsStrategy;
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.capper = this.buildCapper();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(dataModel);
            this.refreshHelper.addDependency(similarity);
            this.refreshHelper.addDependency(candidateItemsStrategy);
            this.refreshHelper.addDependency(mostSimilarItemsCandidateItemsStrategy);
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

        protected virtual float doEstimatePreference(long userID, PreferenceArray preferencesFromUser, long itemID)
        {
            double num = 0.0;
            double num2 = 0.0;
            int num3 = 0;
            double[] numArray = this.similarity.itemSimilarities(itemID, preferencesFromUser.getIDs());
            for (int i = 0; i < numArray.Length; i++)
            {
                double d = numArray[i];
                if (!double.IsNaN(d))
                {
                    num += d * preferencesFromUser.getValue(i);
                    num2 += d;
                    num3++;
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

        private List<RecommendedItem> doMostSimilarItems(long[] itemIDs, int howMany, TopItems.Estimator<long> estimator)
        {
            FastIDSet set = this.mostSimilarItemsCandidateItemsStrategy.getCandidateItems(itemIDs, this.getDataModel());
            return TopItems.getTopItems(howMany, set.GetEnumerator(), null, estimator);
        }

        public override float estimatePreference(long userID, long itemID)
        {
            PreferenceArray preferencesFromUser = this.getDataModel().getPreferencesFromUser(userID);
            float? nullable = getPreferenceForItem(preferencesFromUser, itemID);
            if (nullable.HasValue)
            {
                return nullable.Value;
            }
            return this.doEstimatePreference(userID, preferencesFromUser, itemID);
        }

        protected static MostSimilarItemsCandidateItemsStrategy getDefaultMostSimilarItemsCandidateItemsStrategy()
        {
            return new PreferredItemsNeighborhoodCandidateItemsStrategy();
        }

        private static float? getPreferenceForItem(PreferenceArray preferencesFromUser, long itemID)
        {
            int num = preferencesFromUser.length();
            for (int i = 0; i < num; i++)
            {
                if (preferencesFromUser.getItemID(i) == itemID)
                {
                    return new float?(preferencesFromUser.getValue(i));
                }
            }
            return null;
        }

        public ItemSimilarity getSimilarity()
        {
            return this.similarity;
        }

        public List<RecommendedItem> mostSimilarItems(long itemID, int howMany)
        {
            return this.mostSimilarItems(itemID, howMany, null);
        }

        public List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany)
        {
            TopItems.Estimator<long> estimator = new MultiMostSimilarEstimator(itemIDs, this.similarity, null, EXCLUDE_ITEM_IF_NOT_SIMILAR_TO_ALL_BY_DEFAULT);
            return this.doMostSimilarItems(itemIDs, howMany, estimator);
        }

        public List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany, Rescorer<Tuple<long, long>> rescorer)
        {
            TopItems.Estimator<long> estimator = new MultiMostSimilarEstimator(itemIDs, this.similarity, rescorer, EXCLUDE_ITEM_IF_NOT_SIMILAR_TO_ALL_BY_DEFAULT);
            return this.doMostSimilarItems(itemIDs, howMany, estimator);
        }

        public List<RecommendedItem> mostSimilarItems(long itemID, int howMany, Rescorer<Tuple<long, long>> rescorer)
        {
            TopItems.Estimator<long> estimator = new MostSimilarEstimator(itemID, this.similarity, rescorer);
            return this.doMostSimilarItems(new long[] { itemID }, howMany, estimator);
        }

        public List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany, bool excludeItemIfNotSimilarToAll)
        {
            TopItems.Estimator<long> estimator = new MultiMostSimilarEstimator(itemIDs, this.similarity, null, excludeItemIfNotSimilarToAll);
            return this.doMostSimilarItems(itemIDs, howMany, estimator);
        }

        public List<RecommendedItem> mostSimilarItems(long[] itemIDs, int howMany, Rescorer<Tuple<long, long>> rescorer, bool excludeItemIfNotSimilarToAll)
        {
            TopItems.Estimator<long> estimator = new MultiMostSimilarEstimator(itemIDs, this.similarity, rescorer, excludeItemIfNotSimilarToAll);
            return this.doMostSimilarItems(itemIDs, howMany, estimator);
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            log.debug("Recommending items for user ID '{}'", new object[] { userID });
            PreferenceArray preferencesFromUser = this.getDataModel().getPreferencesFromUser(userID);
            if (preferencesFromUser.length() == 0)
            {
                return new List<RecommendedItem>();
            }
            FastIDSet set = this.getAllOtherItems(userID, preferencesFromUser);
            TopItems.Estimator<long> estimator = new Estimator(this, userID, preferencesFromUser);
            List<RecommendedItem> list = TopItems.getTopItems(howMany, set.GetEnumerator(), rescorer, estimator);
            log.debug("Recommendations are: {}", new object[] { list });
            return list;
        }

        public List<RecommendedItem> recommendedBecause(long userID, long itemID, int howMany)
        {
            DataModel model = this.getDataModel();
            TopItems.Estimator<long> estimator = new RecommendedBecauseEstimator(this, userID, itemID);
            PreferenceArray array = model.getPreferencesFromUser(userID);
            int size = array.length();
            FastIDSet set = new FastIDSet(size);
            for (int i = 0; i < size; i++)
            {
                set.add(array.getItemID(i));
            }
            set.remove(itemID);
            return TopItems.getTopItems(howMany, set.GetEnumerator(), null, estimator);
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        public override string ToString()
        {
            return ("GenericItemBasedRecommender[similarity:" + this.similarity + ']');
        }

        private class Estimator : TopItems.Estimator<long>
        {
            private PreferenceArray preferencesFromUser;
            private GenericItemBasedRecommender recommender;
            private long userID;

            internal Estimator(GenericItemBasedRecommender recommender, long userID, PreferenceArray preferencesFromUser)
            {
                this.recommender = recommender;
                this.userID = userID;
                this.preferencesFromUser = preferencesFromUser;
            }

            public double estimate(long itemID)
            {
                return (double)this.recommender.doEstimatePreference(this.userID, this.preferencesFromUser, itemID);
            }
        }

        public class MostSimilarEstimator : TopItems.Estimator<long>
        {
            private Rescorer<Tuple<long, long>> rescorer;
            private ItemSimilarity similarity;
            private long toItemID;

            public MostSimilarEstimator(long toItemID, ItemSimilarity similarity, Rescorer<Tuple<long, long>> rescorer)
            {
                this.toItemID = toItemID;
                this.similarity = similarity;
                this.rescorer = rescorer;
            }

            public double estimate(long itemID)
            {
                Tuple<long, long> thing = new Tuple<long, long>(this.toItemID, itemID);
                if ((this.rescorer != null) && this.rescorer.isFiltered(thing))
                {
                    return double.NaN;
                }
                double originalScore = this.similarity.itemSimilarity(this.toItemID, itemID);
                return ((this.rescorer == null) ? originalScore : this.rescorer.rescore(thing, originalScore));
            }
        }

        private sealed class MultiMostSimilarEstimator : TopItems.Estimator<long>
        {
            private bool excludeItemIfNotSimilarToAll;
            private Rescorer<Tuple<long, long>> rescorer;
            private ItemSimilarity similarity;
            private long[] toItemIDs;

            internal MultiMostSimilarEstimator(long[] toItemIDs, ItemSimilarity similarity, Rescorer<Tuple<long, long>> rescorer, bool excludeItemIfNotSimilarToAll)
            {
                this.toItemIDs = toItemIDs;
                this.similarity = similarity;
                this.rescorer = rescorer;
                this.excludeItemIfNotSimilarToAll = excludeItemIfNotSimilarToAll;
            }

            public double estimate(long itemID)
            {
                RunningAverage average = new FullRunningAverage();
                double[] numArray = this.similarity.itemSimilarities(itemID, this.toItemIDs);
                for (int i = 0; i < this.toItemIDs.Length; i++)
                {
                    long num2 = this.toItemIDs[i];
                    Tuple<long, long> thing = new Tuple<long, long>(num2, itemID);
                    if ((this.rescorer == null) || !this.rescorer.isFiltered(thing))
                    {
                        double originalScore = numArray[i];
                        if (this.rescorer != null)
                        {
                            originalScore = this.rescorer.rescore(thing, originalScore);
                        }
                        if (!(!this.excludeItemIfNotSimilarToAll && double.IsNaN(originalScore)))
                        {
                            average.addDatum(originalScore);
                        }
                    }
                }
                double num4 = average.getAverage();
                return ((num4 == 0.0) ? double.NaN : num4);
            }
        }

        private sealed class RecommendedBecauseEstimator : TopItems.Estimator<long>
        {
            private GenericItemBasedRecommender r;
            private long recommendedItemID;
            private long userID;

            internal RecommendedBecauseEstimator(GenericItemBasedRecommender r, long userID, long recommendedItemID)
            {
                this.r = r;
                this.userID = userID;
                this.recommendedItemID = recommendedItemID;
            }

            public double estimate(long itemID)
            {
                float? nullable = this.r.getDataModel().getPreferenceValue(this.userID, itemID);
                if (!nullable.HasValue)
                {
                    return double.NaN;
                }
                double num = this.r.similarity.itemSimilarity(this.recommendedItemID, itemID);
                return ((1.0 + num) * ((double)nullable.Value));
            }
        }
    }
}