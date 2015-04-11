namespace org.apache.mahout.cf.taste.similarity.precompute
{
    public class SimilarItem
    {
        private long itemID;
        private double similarity;

        public SimilarItem(long itemID, double similarity)
        {
            this.set(itemID, similarity);
        }

        public static int COMPARE_BY_SIMILARITY(SimilarItem x, SimilarItem y)
        {
            return x.similarity.CompareTo(y.similarity);
        }

        public long getItemID()
        {
            return this.itemID;
        }

        public double getSimilarity()
        {
            return this.similarity;
        }

        public void set(long itemID, double similarity)
        {
            this.itemID = itemID;
            this.similarity = similarity;
        }
    }
}