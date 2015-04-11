namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;

    public class AllSimilarItemsCandidateItemsStrategy : AbstractCandidateItemsStrategy
    {
        private ItemSimilarity similarity;

        public AllSimilarItemsCandidateItemsStrategy(ItemSimilarity similarity)
        {
            this.similarity = similarity;
        }

        protected override FastIDSet doGetCandidateItems(long[] preferredItemIDs, DataModel dataModel)
        {
            FastIDSet set = new FastIDSet();
            foreach (long num in preferredItemIDs)
            {
                set.addAll(this.similarity.allSimilarItemIDs(num));
            }
            set.removeAll(preferredItemIDs);
            return set;
        }
    }
}