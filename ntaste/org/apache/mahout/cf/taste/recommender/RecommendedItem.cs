namespace org.apache.mahout.cf.taste.recommender
{
    public interface RecommendedItem
    {
        long getItemID();

        float getValue();
    }
}