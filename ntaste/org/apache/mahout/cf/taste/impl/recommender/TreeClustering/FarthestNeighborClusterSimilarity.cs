using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.similarity;
using System;
using System.Collections.Generic;

namespace org.apache.mahout.cf.taste.impl.recommender.treeclustering
{
    /**
    * <p>
    * Defines cluster similarity as the <em>smallest</em> similarity between any two users in the clusters --
    * that is, it says that clusters are close when <em>all pairs</em> of their members have relatively high
    * similarity.
    * </p>
    */

    public class FarthestNeighborClusterSimilarity : ClusterSimilarity
    {
        private UserSimilarity similarity;
        private double samplingRate;

        /**
         * <p>
         * Constructs a  based on the given {@link UserSimilarity}. All
         * user-user similarities are examined.
         * </p>
         */

        public FarthestNeighborClusterSimilarity(UserSimilarity similarity)
            : this(similarity, 1.0)
        {
        }

        /**
         * <p>
         * Constructs a  based on the given {@link UserSimilarity}. By
         * setting {@code samplingRate} to a value less than 1.0, this implementation will only examine that
         * fraction of all user-user similarities between two clusters, increasing performance at the expense of
         * accuracy.
         * </p>
         */

        public FarthestNeighborClusterSimilarity(UserSimilarity similarity, double samplingRate)
        {
            if (similarity == null) { throw new Exception("similarity is null"); }
            //Preconditions.checkArgument(similarity != null, "similarity is null");
            //Preconditions.checkArgument(!Double.IsNaN(samplingRate) && samplingRate > 0.0 && samplingRate <= 1.0,
            //                            "samplingRate is invalid: %.4f", samplingRate);
            if (!(!Double.IsNaN(samplingRate) && samplingRate > 0.0 && samplingRate <= 1.0))
            {
                throw new Exception("samplingRate is invalid");
            }
            this.similarity = similarity;
            this.samplingRate = samplingRate;
        }

        public double getSimilarity(FastIDSet cluster1, FastIDSet cluster2)
        {
            if (cluster1.isEmpty() || cluster2.isEmpty())
            {
                return Double.NaN;
            }
            double leastSimilarity = Double.PositiveInfinity;
            var someUsers = SamplingLongPrimitiveIterator.maybeWrapIterator(cluster1.GetEnumerator(), samplingRate);
            while (someUsers.MoveNext())
            {
                long userID1 = someUsers.Current;
                var it2 = cluster2.GetEnumerator();
                while (it2.MoveNext())
                {
                    double theSimilarity = similarity.userSimilarity(userID1, it2.Current);
                    if (theSimilarity < leastSimilarity)
                    {
                        leastSimilarity = theSimilarity;
                    }
                }
            }
            // We skipped everything? well, at least try comparing the first Users to get some value
            if (leastSimilarity == Double.PositiveInfinity)
            {
                return similarity.userSimilarity(cluster1.GetEnumerator().Current, cluster2.GetEnumerator().Current);
            }
            return leastSimilarity;
        }

        public void refresh(IList<taste.common.Refreshable> alreadyRefreshed)
        {
            alreadyRefreshed = RefreshHelper.buildRefreshed(alreadyRefreshed);
            RefreshHelper.maybeRefresh(alreadyRefreshed, similarity);
        }

        public override String ToString()
        {
            return "FarthestNeighborClusterSimilarity[similarity:" + similarity + ']';
        }
    }
}