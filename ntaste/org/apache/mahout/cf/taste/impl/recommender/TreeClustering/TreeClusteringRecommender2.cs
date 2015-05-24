using org.apache.mahout.cf.taste.common;
using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.model;
using org.apache.mahout.cf.taste.recommender;
using org.apache.mahout.common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace org.apache.mahout.cf.taste.impl.recommender.treeclustering
{
    /// <summary>
    /// TODO:not test by 徐明祥
    /// </summary>
    public class TreeClusteringRecommender2 : AbstractRecommender, ClusteringRecommender
    {
        private static Logger log = LoggerFactory.getLogger(typeof(TreeClusteringRecommender2));
        private static int NUM_CLUSTER_RECS = 100;

        private ClusterSimilarity clusterSimilarity;
        private int numClusters;
        private double clusteringThreshold;
        private bool clusteringByThreshold;
        private FastByIDMap<List<RecommendedItem>> topRecsByUserID;
        private FastIDSet[] allClusters;
        private FastByIDMap<FastIDSet> clustersByUserID;
        private RefreshHelper refreshHelper;

        /**
         * @param dataModel
         *          {@link DataModel} which provides users
         * @param clusterSimilarity
         *          {@link ClusterSimilarity} used to compute cluster similarity
         * @param numClusters
         *          desired number of clusters to create
         * @throws IllegalArgumentException
         *           if arguments are {@code null}, or {@code numClusters} is less than 2
         */

        public TreeClusteringRecommender2(DataModel dataModel, ClusterSimilarity clusterSimilarity, int numClusters)
            : base(dataModel)
        {
            if (numClusters < 2) { throw new Exception("numClusters must be at least 2"); }
            //Preconditions.checkArgument(numClusters >= 2, "numClusters must be at least 2");
            this.clusterSimilarity = clusterSimilarity;//Preconditions.checkNotNull(clusterSimilarity);
            this.numClusters = numClusters;
            this.clusteringThreshold = Double.NaN;
            this.clusteringByThreshold = false;
            this.refreshHelper = new RefreshHelper(buildClusters);
            refreshHelper.addDependency(dataModel);
            refreshHelper.addDependency(clusterSimilarity);
            buildClusters();
        }

        /**
         * @param dataModel
         *          {@link DataModel} which provides users
         * @param clusterSimilarity
         *          {@link ClusterSimilarity} used to compute cluster
         *          similarity
         * @param clusteringThreshold
         *          clustering similarity threshold; clusters will be aggregated into larger clusters until the next
         *          two nearest clusters' similarity drops below this threshold
         * @throws IllegalArgumentException
         *           if arguments are {@code null}, or {@code clusteringThreshold} is {@link Double#NaN}
         */

        public TreeClusteringRecommender2(DataModel dataModel,
                                          ClusterSimilarity clusterSimilarity,
                                          double clusteringThreshold)
            : base(dataModel)
        {
            //  Preconditions.checkArgument(!Double.isNaN(clusteringThreshold), "clusteringThreshold must not be NaN");
            if (Double.IsNaN(clusteringThreshold)) { throw new Exception("clusteringThreshold must not be NaN"); }
            this.clusterSimilarity = clusterSimilarity;//Preconditions.checkNotNull(clusterSimilarity);
            this.numClusters = int.MinValue; // Integer.MIN_VALUE;
            this.clusteringThreshold = clusteringThreshold;
            this.clusteringByThreshold = true;
            this.refreshHelper = new RefreshHelper(buildClusters);
            refreshHelper.addDependency(dataModel);
            refreshHelper.addDependency(clusterSimilarity);
            buildClusters();
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            // Preconditions.checkArgument(howMany >= 1, "howMany must be at least 1");
            if (howMany < 1) { throw new Exception("howMany must be at least 1"); };
            buildClusters();

            log.debug("Recommending items for user ID '{}'", userID);

            List<RecommendedItem> recommended = topRecsByUserID.get(userID);
            if (recommended == null)
            {
                return new List<RecommendedItem>();
            }

            DataModel dataModel = getDataModel();
            List<RecommendedItem> rescored = new List<RecommendedItem>(); //Lists.newArrayListWithCapacity(recommended.size());
            // Only add items the user doesn't already have a preference for.
            // And that the rescorer doesn't "reject".
            foreach (RecommendedItem recommendedItem in recommended)
            {
                long itemID = recommendedItem.getItemID();
                if (rescorer != null && rescorer.isFiltered(itemID))
                {
                    continue;
                }
                if (dataModel.getPreferenceValue(userID, itemID) == null
                    && (rescorer == null || !Double.IsNaN(rescorer.rescore(itemID, recommendedItem.getValue()))))
                {
                    rescored.Add(recommendedItem);
                }
            }

            rescored.Sort(new ByRescoreComparator(rescorer));

            return rescored;
        }

        public override float estimatePreference(long userID, long itemID)
        {
            float? actualPref = getDataModel().getPreferenceValue(userID, itemID);
            if (actualPref != null)
            {
                return actualPref.Value;
            }
            buildClusters();
            List<RecommendedItem> topRecsForUser = topRecsByUserID.get(userID);
            if (topRecsForUser != null)
            {
                foreach (RecommendedItem item in topRecsForUser)
                {
                    if (itemID == item.getItemID())
                    {
                        return item.getValue();
                    }
                }
            }
            // Hmm, we have no idea. The item is not in the user's cluster
            return float.NaN;
        }

        public FastIDSet getCluster(long userID)
        {
            buildClusters();
            FastIDSet cluster = clustersByUserID.get(userID);
            return cluster == null ? new FastIDSet() : cluster;
        }

        public FastIDSet[] getClusters()
        {
            buildClusters();
            return allClusters;
        }

        private class ClusterClusterPair : Comparer<ClusterClusterPair>
        {
            private FastIDSet cluster1;
            private FastIDSet cluster2;
            private double similarity;

            public ClusterClusterPair()
            {
            }

            public ClusterClusterPair(FastIDSet cluster1, FastIDSet cluster2, double similarity)
            {
                this.cluster1 = cluster1;
                this.cluster2 = cluster2;
                this.similarity = similarity;
            }

            public FastIDSet getCluster1()
            {
                return cluster1;
            }

            public FastIDSet getCluster2()
            {
                return cluster2;
            }

            public double getSimilarity()
            {
                return similarity;
            }

            public override int GetHashCode()
            {
                return cluster1.GetHashCode() ^ cluster2.GetHashCode() ^ RandomUtils.hashDouble(similarity);
            }

            public override bool Equals(Object o)
            {
                if (!(o is ClusterClusterPair))
                {
                    return false;
                }
                ClusterClusterPair other = (ClusterClusterPair)o;
                return cluster1.Equals(other.getCluster1())
                    && cluster2.Equals(other.getCluster2())
                    && similarity == other.getSimilarity();
            }

            //public int Compare(ClusterClusterPair other)
            //{
            //    double otherSimilarity = other.getSimilarity();
            //    if (similarity > otherSimilarity)
            //    {
            //        return -1;
            //    }
            //    else if (similarity < otherSimilarity)
            //    {
            //        return 1;
            //    }
            //    else
            //    {
            //        return 0;
            //    }
            //}

            public override int Compare(ClusterClusterPair x, ClusterClusterPair y)
            {
                double otherSimilarity = y.getSimilarity();
                if (similarity > otherSimilarity)
                {
                    return 1;// reverseOrder -1
                }
                else if (similarity < otherSimilarity)
                {
                    return -1;//reverseOrder
                }
                else
                {
                    return 0;
                }
            }
        }

        private void buildClusters()
        {
            DataModel model = getDataModel();
            int numUsers = model.getNumUsers();

            if (numUsers == 0)
            {
                topRecsByUserID = new FastByIDMap<List<RecommendedItem>>();
                clustersByUserID = new FastByIDMap<FastIDSet>();
            }
            else
            {
                List<FastIDSet> clusters = new List<FastIDSet>();
                // Begin with a cluster for each user:
                var it = model.getUserIDs();
                while (it.MoveNext())
                {
                    FastIDSet newCluster = new FastIDSet();
                    newCluster.add(it.Current);
                    clusters.Add(newCluster);
                }

                bool done = false;
                while (!done)
                {
                    done = mergeClosestClusters(numUsers, clusters, done);
                }

                topRecsByUserID = computeTopRecsPerUserID(clusters);
                clustersByUserID = computeClustersPerUserID(clusters);
                allClusters = clusters.ToArray();
            }
        }

        private bool mergeClosestClusters(int numUsers, List<FastIDSet> clusters, bool done)
        {
            // We find a certain number of closest clusters...
            List<ClusterClusterPair> queue = findClosestClusters(numUsers, clusters);
            //  List<ClusterClusterPair> queue = new List<ClusterClusterPair>();
            //foreach (var item in _queue)
            //{
            //    queue.Enqueue(item);
            //}

            // The first one is definitely the closest pair in existence so we can cluster
            // the two together, put it back into the set of clusters, and start again. Instead
            // we assume everything else in our list of closest cluster pairs is still pretty good,
            // and we cluster them too.

            for (int n = 0; n < queue.Count; n++)
            {
                //}
                //while (queue.Count > 0)
                //{
                if (!clusteringByThreshold && clusters.Count <= numClusters)
                {
                    done = true;
                    break;
                }

                ClusterClusterPair top = queue[n];
                queue.RemoveAt(n);
                if (clusteringByThreshold && top.getSimilarity() < clusteringThreshold)
                {
                    done = true;
                    break;
                }

                FastIDSet cluster1 = top.getCluster1();
                FastIDSet cluster2 = top.getCluster2();

                // Pull out current two clusters from clusters
                var clusterIterator = clusters;
                bool removed1 = false;
                bool removed2 = false;
                for (int m = 0; m < clusterIterator.Count; m++)
                {
                    if (!(removed1 && removed2))
                    {
                        FastIDSet current = clusterIterator[m];

                        // Yes, use == here
                        if (!removed1 && cluster1 == current)
                        {
                            clusterIterator.RemoveAt(m);
                            m--;
                            removed1 = true;
                        }
                        else if (!removed2 && cluster2 == current)
                        {
                            clusterIterator.RemoveAt(m);
                            m--;
                            removed2 = true;
                        }
                    }

                    // The only catch is if a cluster showed it twice in the list of best cluster pairs;
                    // have to remove the others. Pull out anything referencing these clusters from queue
                    for (int k = 0; k < queue.Count; k++)
                    {
                        //}

                        //    for (Iterator<ClusterClusterPair> queueIterator = queue.iterator(); queueIterator.hasNext(); )
                        //    {
                        ClusterClusterPair pair = queue[k];
                        FastIDSet pair1 = pair.getCluster1();
                        FastIDSet pair2 = pair.getCluster2();
                        if (pair1 == cluster1 || pair1 == cluster2 || pair2 == cluster1 || pair2 == cluster2)
                        {
                            queue.RemoveAt(k);
                            //queueIterator.remove();
                        }
                    }

                    // Make new merged cluster
                    FastIDSet merged = new FastIDSet(cluster1.size() + cluster2.size());
                    merged.addAll(cluster1);
                    merged.addAll(cluster2);

                    // Compare against other clusters; update queue if needed
                    // That new pair we're just adding might be pretty close to something else, so
                    // catch that case here and put it back into our queue
                    for (var i = 0; i < clusters.Count; i++)
                    {
                        FastIDSet cluster = clusters[i];
                        double similarity = clusterSimilarity.getSimilarity(merged, cluster);
                        if (similarity > queue[queue.Count - 1].getSimilarity())
                        {
                            var queueIterator = queue.GetEnumerator();

                            while (queueIterator.MoveNext())
                            {
                                if (similarity > queueIterator.Current.getSimilarity())
                                {
                                    n--;
                                    // queueIterator.previous();
                                    break;
                                }
                            }
                            queue.Add(new ClusterClusterPair(merged, cluster, similarity));
                        }
                    }

                    // Finally add new cluster to list
                    clusters.Add(merged);
                }
            }
            return done;
        }

        private List<ClusterClusterPair> findClosestClusters(int numUsers, List<FastIDSet> clusters)
        {
            PriorityQueue<ClusterClusterPair> queue = new PriorityQueue<ClusterClusterPair>(numUsers + 1, new ClusterClusterPair());
            int size = clusters.Count;
            for (int i = 0; i < size; i++)
            {
                FastIDSet cluster1 = clusters[i];
                for (int j = i + 1; j < size; j++)
                {
                    FastIDSet cluster2 = clusters[j];
                    double similarity = clusterSimilarity.getSimilarity(cluster1, cluster2);
                    if (!Double.IsNaN(similarity))
                    {
                        if (queue.Count < numUsers)
                        {
                            queue.Push(new ClusterClusterPair(cluster1, cluster2, similarity));
                        }
                        else if (similarity > queue.Pop().getSimilarity())
                        {
                            queue.Push(new ClusterClusterPair(cluster1, cluster2, similarity));
                            queue.Pop();
                        }
                    }
                }
            }
            List<ClusterClusterPair> result = queue.ToList();
            result.Sort();

            return result;
        }

        private FastByIDMap<List<RecommendedItem>> computeTopRecsPerUserID(List<FastIDSet> clusters)
        {
            FastByIDMap<List<RecommendedItem>> recsPerUser = new FastByIDMap<List<RecommendedItem>>();
            foreach (FastIDSet cluster in clusters)
            {
                List<RecommendedItem> recs = computeTopRecsForCluster(cluster);
                var it = cluster.GetEnumerator();
                while (it.MoveNext())
                {
                    recsPerUser.put(it.Current, recs);
                }
            }
            return recsPerUser;
        }

        private List<RecommendedItem> computeTopRecsForCluster(FastIDSet cluster)
        {
            DataModel dataModel = getDataModel();
            FastIDSet possibleItemIDs = new FastIDSet();
            var it = cluster.GetEnumerator();
            while (it.MoveNext())
            {
                possibleItemIDs.addAll(dataModel.getItemIDsFromUser(it.Current));
            }

            TopItems.Estimator<long> estimator = new Estimator(cluster, this);

            List<RecommendedItem> topItems = TopItems.getTopItems(NUM_CLUSTER_RECS,
              possibleItemIDs.GetEnumerator(), null, estimator);

            log.debug("Recommendations are: {}", topItems);
            return topItems;
        }

        private static FastByIDMap<FastIDSet> computeClustersPerUserID(IList<FastIDSet> clusters)
        {
            FastByIDMap<FastIDSet> clustersPerUser = new FastByIDMap<FastIDSet>(clusters.Count);
            foreach (FastIDSet cluster in clusters)
            {
                var it = cluster.GetEnumerator();
                while (it.MoveNext())
                {
                    clustersPerUser.put(it.Current, cluster);
                }
            }
            return clustersPerUser;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            refreshHelper.refresh(alreadyRefreshed);
        }

        public override string ToString()
        {
            return "TreeClusteringRecommender2[clusterSimilarity:" + clusterSimilarity + ']';
        }

        private class Estimator : TopItems.Estimator<long>
        {
            private FastIDSet cluster;
            private TreeClusteringRecommender2 re;

            public Estimator(FastIDSet cluster, TreeClusteringRecommender2 recommender)
            {
                this.cluster = cluster;
                this.re = recommender;
            }

            public double estimate(long itemID)
            {
                DataModel dataModel = this.re.getDataModel();
                RunningAverage average = new FullRunningAverage();
                var it = cluster.GetEnumerator();
                while (it.MoveNext())
                {
                    float? pref = dataModel.getPreferenceValue(it.Current, itemID);
                    if (pref != null)
                    {
                        average.addDatum(pref.Value);
                    }
                }
                return average.getAverage();
            }
        }
    }
}