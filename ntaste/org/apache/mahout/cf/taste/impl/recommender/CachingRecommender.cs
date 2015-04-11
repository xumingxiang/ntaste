namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System;
    using System.Collections.Generic;

    public sealed class CachingRecommender : Recommender, Refreshable
    {
        private IDRescorer currentRescorer;
        private Cache<Tuple<long, long>, float> estimatedPrefCache;
        private static Logger log = LoggerFactory.getLogger(typeof(CachingRecommender));
        private int[] maxHowMany;
        private Cache<long, Recommendations> recommendationCache;
        private Retriever<long, Recommendations> recommendationsRetriever;
        private Recommender recommender;
        private RefreshHelper refreshHelper;

        public CachingRecommender(Recommender recommender)
        {
            Action refreshRunnable = null;
            this.recommender = recommender;
            this.maxHowMany = new int[] { 1 };
            int maxEntries = recommender.getDataModel().getNumUsers();
            this.recommendationsRetriever = new RecommendationRetriever(this);
            this.recommendationCache = new Cache<long, Recommendations>(this.recommendationsRetriever, maxEntries);
            this.estimatedPrefCache = new Cache<Tuple<long, long>, float>(new EstimatedPrefRetriever(this), maxEntries);
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.clear();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(recommender);
        }

        public void clear()
        {
            log.debug("Clearing all recommendations...", new object[0]);
            this.recommendationCache.clear();
            this.estimatedPrefCache.clear();
        }

        public void clear(long userID)
        {
            log.debug("Clearing recommendations for user ID '{}'", new object[] { userID });
            this.recommendationCache.remove(userID);
            this.estimatedPrefCache.removeKeysMatching(userItemPair => userItemPair.Item1 == userID);
        }

        public float estimatePreference(long userID, long itemID)
        {
            return this.estimatedPrefCache.get(new Tuple<long, long>(userID, itemID));
        }

        public DataModel getDataModel()
        {
            return this.recommender.getDataModel();
        }

        public List<RecommendedItem> recommend(long userID, int howMany)
        {
            return this.recommend(userID, howMany, null);
        }

        public List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            lock (this.maxHowMany)
            {
                if (howMany > this.maxHowMany[0])
                {
                    this.maxHowMany[0] = howMany;
                }
            }
            if (userID == -9223372036854775808L)
            {
                return this.recommendationsRetriever.get(-9223372036854775808L).getItems();
            }
            this.setCurrentRescorer(rescorer);
            Recommendations recommendations = this.recommendationCache.get(userID);
            if ((recommendations.getItems().Count < howMany) && !recommendations.isNoMoreRecommendableItems())
            {
                this.clear(userID);
                recommendations = this.recommendationCache.get(userID);
                if (recommendations.getItems().Count < howMany)
                {
                    recommendations.setNoMoreRecommendableItems(true);
                }
            }
            List<RecommendedItem> list = recommendations.getItems();
            return ((list.Count > howMany) ? list.GetRange(0, howMany) : list);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        public void removePreference(long userID, long itemID)
        {
            this.recommender.removePreference(userID, itemID);
            this.clear(userID);
        }

        private void setCurrentRescorer(IDRescorer rescorer)
        {
            if (rescorer == null)
            {
                if (this.currentRescorer != null)
                {
                    this.currentRescorer = null;
                    this.clear();
                }
            }
            else if (!rescorer.Equals(this.currentRescorer))
            {
                this.currentRescorer = rescorer;
                this.clear();
            }
        }

        public void setPreference(long userID, long itemID, float value)
        {
            this.recommender.setPreference(userID, itemID, value);
            this.clear(userID);
        }

        public override string ToString()
        {
            return ("CachingRecommender[recommender:" + this.recommender + ']');
        }

        private sealed class EstimatedPrefRetriever : Retriever<Tuple<long, long>, float>
        {
            private CachingRecommender p;

            internal EstimatedPrefRetriever(CachingRecommender parent)
            {
                this.p = parent;
            }

            public float get(Tuple<long, long> key)
            {
                long userID = key.Item1;
                long itemID = key.Item2;
                CachingRecommender.log.debug("Retrieving estimated preference for user ID '{}' and item ID '{}'", new object[] { userID, itemID });
                return this.p.recommender.estimatePreference(userID, itemID);
            }
        }

        private sealed class RecommendationRetriever : Retriever<long, CachingRecommender.Recommendations>
        {
            private CachingRecommender p;

            internal RecommendationRetriever(CachingRecommender parent)
            {
                this.p = parent;
            }

            public CachingRecommender.Recommendations get(long key)
            {
                CachingRecommender.log.debug("Retrieving new recommendations for user ID '{}'", new object[] { key });
                int howMany = this.p.maxHowMany[0];
                IDRescorer currentRescorer = this.p.currentRescorer;
                List<RecommendedItem> collection = (currentRescorer == null) ? this.p.recommender.recommend(key, howMany) : this.p.recommender.recommend(key, howMany, currentRescorer);
                return new CachingRecommender.Recommendations(new List<RecommendedItem>(collection));
            }
        }

        private sealed class Recommendations
        {
            private List<RecommendedItem> items;
            private bool noMoreRecommendableItems;

            internal Recommendations(List<RecommendedItem> items)
            {
                this.items = items;
            }

            public List<RecommendedItem> getItems()
            {
                return this.items;
            }

            public bool isNoMoreRecommendableItems()
            {
                return this.noMoreRecommendableItems;
            }

            public void setNoMoreRecommendableItems(bool noMoreRecommendableItems)
            {
                this.noMoreRecommendableItems = noMoreRecommendableItems;
            }
        }
    }
}