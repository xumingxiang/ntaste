namespace org.apache.mahout.cf.taste.recommender
{
    using org.apache.mahout.cf.taste.common;
    using System;

    public interface UserBasedRecommender : Recommender, Refreshable
    {
        long[] mostSimilarUserIDs(long userID, int howMany);

        long[] mostSimilarUserIDs(long userID, int howMany, Rescorer<Tuple<long, long>> rescorer);
    }
}