namespace org.apache.mahout.cf.taste.similarity.precompute
{
    using org.apache.mahout.cf.taste.recommender;
    using System.Collections.Generic;

    public class SimilarItems
    {
        private long itemID;
        private long[] similarItemIDs;
        private double[] similarities;

        public SimilarItems(long itemID, List<RecommendedItem> similarItems)
        {
            this.itemID = itemID;
            int count = similarItems.Count;
            this.similarItemIDs = new long[count];
            this.similarities = new double[count];
            for (int i = 0; i < count; i++)
            {
                this.similarItemIDs[i] = similarItems[i].getItemID();
                this.similarities[i] = similarItems[i].getValue();
            }
        }

        public long getItemID()
        {
            return this.itemID;
        }

        public IEnumerable<SimilarItem> getSimilarItems()
        {
            for (int i = 0; i < this.similarItemIDs.Length; i++)
            {
                yield return new SimilarItem(this.similarItemIDs[i], this.similarities[i]);
            }
        }

        public int numSimilarItems()
        {
            return this.similarItemIDs.Length;
        }
    }
}