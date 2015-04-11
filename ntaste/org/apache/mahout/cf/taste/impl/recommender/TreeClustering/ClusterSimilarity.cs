using org.apache.mahout.cf.taste.common;
using org.apache.mahout.cf.taste.impl.common;

namespace org.apache.mahout.cf.taste.impl.recommender.treeclustering
{
    public interface ClusterSimilarity : Refreshable
    {
        /**
  * @param cluster1
  *          first cluster of user IDs
  * @param cluster2
  *          second cluster of user IDs
  * @return "distance" between clusters; a bigger value means less similarity
  * @throws TasteException
  *           if an error occurs while computing similarity, such as errors accessing an underlying
  *           {@link org.apache.mahout.cf.taste.model.DataModel}
  * @throws IllegalArgumentException
  *           if either argument is null or empty
  */

        double getSimilarity(FastIDSet cluster1, FastIDSet cluster2);
    }
}