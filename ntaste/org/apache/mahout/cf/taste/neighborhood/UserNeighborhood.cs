namespace org.apache.mahout.cf.taste.neighborhood
{
    using org.apache.mahout.cf.taste.common;

    public interface UserNeighborhood : Refreshable
    {
        long[] getUserNeighborhood(long userID);
    }
}