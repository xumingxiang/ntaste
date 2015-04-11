namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public sealed class CachingUserSimilarity : UserSimilarity, Refreshable
    {
        private RefreshHelper refreshHelper;
        private UserSimilarity similarity;
        private Cache<Tuple<long, long>, double> similarityCache;

        public CachingUserSimilarity(UserSimilarity similarity, DataModel dataModel)
            : this(similarity, dataModel.getNumUsers())
        {
        }

        public CachingUserSimilarity(UserSimilarity similarity, int maxCacheSize)
        {
            Action refreshRunnable = null;
            this.similarity = similarity;
            this.similarityCache = new Cache<Tuple<long, long>, double>(new SimilarityRetriever(similarity), maxCacheSize);
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.similarityCache.clear();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(similarity);
        }

        public void clearCacheForUser(long userID)
        {
            this.similarityCache.removeKeysMatching(new Func<Tuple<long, long>, bool>(new longPairMatchPredicate(userID).matches));
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        public void setPreferenceInferrer(PreferenceInferrer inferrer)
        {
            this.similarityCache.clear();
            this.similarity.setPreferenceInferrer(inferrer);
        }

        public double userSimilarity(long userID1, long userID2)
        {
            Tuple<long, long> key = (userID1 < userID2) ? new Tuple<long, long>(userID1, userID2) : new Tuple<long, long>(userID2, userID1);
            return this.similarityCache.get(key);
        }

        private sealed class SimilarityRetriever : Retriever<Tuple<long, long>, double>
        {
            private UserSimilarity similarity;

            internal SimilarityRetriever(UserSimilarity similarity)
            {
                this.similarity = similarity;
            }

            public double get(Tuple<long, long> key)
            {
                return this.similarity.userSimilarity(key.Item1, key.Item2);
            }
        }
    }
}