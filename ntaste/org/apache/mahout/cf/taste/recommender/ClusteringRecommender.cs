using org.apache.mahout.cf.taste.impl.common;

namespace org.apache.mahout.cf.taste.recommender
{
    public interface ClusteringRecommender : Recommender
    {
        /**
    * <p>
    * Returns the cluster of users to which the given user, denoted by user ID, belongs.
    * </p>
    *
    * @param userID
    *          user ID for which to find a cluster
    * @return {@link FastIDSet} of IDs of users in the requested user's cluster
    * @throws TasteException
    *           if an error occurs while accessing the {@link org.apache.mahout.cf.taste.model.DataModel}
    */

        FastIDSet getCluster(long userID);

        /**
         * <p>
         * Returns all clusters of users.
         * </p>
         *
         * @return array of {@link FastIDSet}s of user IDs
         * @throws TasteException
         *           if an error occurs while accessing the {@link org.apache.mahout.cf.taste.model.DataModel}
         */

        FastIDSet[] getClusters();
    }
}