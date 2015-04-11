namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System;
    using System.Collections.Generic;

    public sealed class CachingItemSimilarity : ItemSimilarity, Refreshable
    {
        private RefreshHelper refreshHelper;
        private ItemSimilarity similarity;
        private Cache<Tuple<long, long>, double> similarityCache;

        public CachingItemSimilarity(ItemSimilarity similarity, DataModel dataModel)
            : this(similarity, dataModel.getNumItems())
        {
        }

        public CachingItemSimilarity(ItemSimilarity similarity, int maxCacheSize)
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

        public long[] allSimilarItemIDs(long itemID)
        {
            return this.similarity.allSimilarItemIDs(itemID);
        }

        public void clearCacheForItem(long itemID)
        {
            this.similarityCache.removeKeysMatching(new Func<Tuple<long, long>, bool>(new longPairMatchPredicate(itemID).matches));
        }

        public double[] itemSimilarities(long itemID1, long[] itemID2s)
        {
            int length = itemID2s.Length;
            double[] numArray = new double[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = this.itemSimilarity(itemID1, itemID2s[i]);
            }
            return numArray;
        }

        public double itemSimilarity(long itemID1, long itemID2)
        {
            Tuple<long, long> key = (itemID1 < itemID2) ? new Tuple<long, long>(itemID1, itemID2) : new Tuple<long, long>(itemID2, itemID1);
            return this.similarityCache.get(key);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        private sealed class SimilarityRetriever : Retriever<Tuple<long, long>, double>
        {
            private ItemSimilarity similarity;

            internal SimilarityRetriever(ItemSimilarity similarity)
            {
                this.similarity = similarity;
            }

            public double get(Tuple<long, long> key)
            {
                return this.similarity.itemSimilarity(key.Item1, key.Item2);
            }
        }
    }
}