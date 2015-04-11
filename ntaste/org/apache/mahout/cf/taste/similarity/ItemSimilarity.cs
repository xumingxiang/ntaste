namespace org.apache.mahout.cf.taste.similarity
{
    using org.apache.mahout.cf.taste.common;

    public interface ItemSimilarity : Refreshable
    {
        long[] allSimilarItemIDs(long itemID);

        double[] itemSimilarities(long itemID1, long[] itemID2s);

        double itemSimilarity(long itemID1, long itemID2);
    }
}