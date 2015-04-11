namespace org.apache.mahout.cf.taste.similarity.precompute
{
    using org.apache.mahout.cf.taste.recommender;

    public abstract class BatchItemSimilarities
    {
        private ItemBasedRecommender recommender;
        private int similarItemsPerItem;

        protected BatchItemSimilarities(ItemBasedRecommender recommender, int similarItemsPerItem)
        {
            this.recommender = recommender;
            this.similarItemsPerItem = similarItemsPerItem;
        }

        public abstract int computeItemSimilarities(int degreeOfParallelism, int maxDurationInHours, SimilarItemsWriter writer);

        protected ItemBasedRecommender getRecommender()
        {
            return this.recommender;
        }

        protected int getSimilarItemsPerItem()
        {
            return this.similarItemsPerItem;
        }
    }
}