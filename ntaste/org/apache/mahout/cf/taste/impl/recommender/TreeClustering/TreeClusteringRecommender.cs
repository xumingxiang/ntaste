using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.model;
using org.apache.mahout.cf.taste.recommender;
using org.apache.mahout.common;
using System;
using System.Collections.Generic;

namespace org.apache.mahout.cf.taste.impl.recommender.treeclustering
{
    /// <summary>
    /// TODO:not test
    /// </summary>
    public class TreeClusteringRecommender : AbstractRecommender, ClusteringRecommender
    {
        private static Logger log = LoggerFactory.getLogger(typeof(TreeClusteringRecommender));
        private static FastIDSet[] NO_CLUSTERS = new FastIDSet[0];

        private RandomWrapper random;
        private ClusterSimilarity clusterSimilarity;
        private int numClusters;
        private double clusteringThreshold;
        private bool clusteringByThreshold;
        private double samplingRate;
        private FastByIDMap<List<RecommendedItem>> topRecsByUserID;
        private FastIDSet[] allClusters;
        private FastByIDMap<FastIDSet> clustersByUserID;
        private RefreshHelper refreshHelper;

        /**
         * @param dataModel
         *          {@link DataModel} which provdes users
         * @param clusterSimilarity
         *          {@link ClusterSimilarity} used to compute cluster similarity
         * @param numClusters
         *          desired number of clusters to create
         * @throws IllegalArgumentException
         *           if arguments are {@code null}, or {@code numClusters} is less than 2
         */

        public TreeClusteringRecommender(DataModel dataModel, ClusterSimilarity clusterSimilarity, int numClusters)
            : this(dataModel, clusterSimilarity, numClusters, 1.0)
        {
        }

        /**
         * @param dataModel
         *          {@link DataModel} which provdes users
         * @param clusterSimilarity
         *          {@link ClusterSimilarity} used to compute cluster similarity
         * @param numClusters
         *          desired number of clusters to create
         * @param samplingRate
         *          percentage of all cluster-cluster pairs to consider when finding next-most-similar clusters.
         *          Decreasing this value from 1.0 can increase performance at the cost of accuracy
         * @throws IllegalArgumentException
         *           if arguments are {@code null}, or {@code numClusters} is less than 2, or samplingRate
         *           is {@link Double#NaN} or nonpositive or greater than 1.0
         */

        public TreeClusteringRecommender(DataModel dataModel,
                                         ClusterSimilarity clusterSimilarity,
                                         int numClusters,
                                         double samplingRate)
            : base(dataModel)
        {
            //Preconditions.checkArgument(numClusters >= 2, "numClusters must be at least 2");
            //Preconditions.checkArgument(samplingRate > 0.0 && samplingRate <= 1.0,
            //  "samplingRate is invalid: %f", samplingRate);
            random = RandomUtils.getRandom();
            this.clusterSimilarity = clusterSimilarity;//Preconditions.checkNotNull(clusterSimilarity);
            this.numClusters = numClusters;
            this.clusteringThreshold = Double.NaN;
            this.clusteringByThreshold = false;
            this.samplingRate = samplingRate;
            this.refreshHelper = new RefreshHelper(buildClusters);
            refreshHelper.addDependency(dataModel);
            refreshHelper.addDependency(clusterSimilarity);
            buildClusters();
        }

        /**
         * @param dataModel
         *          {@link DataModel} which provdes users
         * @param clusterSimilarity
         *          {@link ClusterSimilarity} used to compute cluster similarity
         * @param clusteringThreshold
         *          clustering similarity threshold; clusters will be aggregated into larger clusters until the next
         *          two nearest clusters' similarity drops below this threshold
         * @throws IllegalArgumentException
         *           if arguments are {@code null}, or {@code clusteringThreshold} is {@link Double#NaN}
         */

        public TreeClusteringRecommender(DataModel dataModel,
                                         ClusterSimilarity clusterSimilarity,
                                         double clusteringThreshold) :
            this(dataModel, clusterSimilarity, clusteringThreshold, 1.0)
        {
        }

        /**
         * @param dataModel
         *          {@link DataModel} which provides users
         * @param clusterSimilarity
         *          {@link ClusterSimilarity} used to compute cluster similarity
         * @param clusteringThreshold
         *          clustering similarity threshold; clusters will be aggregated into larger clusters until the next
         *          two nearest clusters' similarity drops below this threshold
         * @param samplingRate
         *          percentage of all cluster-cluster pairs to consider when finding next-most-similar clusters.
         *          Decreasing this value from 1.0 can increase performance at the cost of accuracy
         * @throws IllegalArgumentException
         *           if arguments are {@code null}, or {@code clusteringThreshold} is {@link Double#NaN},
         *           or samplingRate is {@link Double#NaN} or nonpositive or greater than 1.0
         */

        public TreeClusteringRecommender(DataModel dataModel,
                                         ClusterSimilarity clusterSimilarity,
                                         double clusteringThreshold,
                                         double samplingRate)
            : base(dataModel)
        {
            //Preconditions.checkArgument(!Double.IsNaN(clusteringThreshold), "clusteringThreshold must not be NaN");
            //Preconditions.checkArgument(samplingRate > 0.0 && samplingRate <= 1.0, "samplingRate is invalid: %f", samplingRate);
            random = RandomUtils.getRandom();
            this.clusterSimilarity = clusterSimilarity;//Preconditions.checkNotNull(clusterSimilarity);
            this.numClusters = int.MinValue; //Integer.MIN_VALUE;
            this.clusteringThreshold = clusteringThreshold;
            this.clusteringByThreshold = true;
            this.samplingRate = samplingRate;
            this.refreshHelper = new RefreshHelper(buildClusters);
            refreshHelper.addDependency(dataModel);
            refreshHelper.addDependency(clusterSimilarity);
            buildClusters();
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            //Preconditions.checkArgument(howMany >= 1, "howMany must be at least 1");
            buildClusters();

            log.debug("Recommending items for user ID '{}'", userID);

            List<RecommendedItem> recommended = topRecsByUserID.get(userID);
            if (recommended == null)
            {
                return new List<RecommendedItem>();
            }

            DataModel dataModel = getDataModel();
            List<RecommendedItem> rescored = new List<RecommendedItem>();
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
            // Collections.sort(rescored, new ByRescoreComparator(rescorer));
            rescored.Sort(new ByRescoreComparator(rescorer));
            return rescored;
        }

        public override float estimatePreference(long userID, long itemID)
        {
            DataModel model = getDataModel();
            float? actualPref = model.getPreferenceValue(userID, itemID);
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

        private void buildClusters()
        {
            DataModel model = getDataModel();
            int numUsers = model.getNumUsers();
            if (numUsers > 0)
            {
                List<FastIDSet> newClusters = new List<FastIDSet>();
                // Begin with a cluster for each user:
                var it = model.getUserIDs();
                while (it.MoveNext())
                {
                    FastIDSet newCluster = new FastIDSet();
                    newCluster.add(it.Current);
                    newClusters.Add(newCluster);
                }
                if (numUsers > 1)
                {
                    findClusters(newClusters);
                }
                topRecsByUserID = computeTopRecsPerUserID(newClusters);
                clustersByUserID = computeClustersPerUserID(newClusters);
                allClusters = newClusters.ToArray();
            }
            else
            {
                topRecsByUserID = new FastByIDMap<List<RecommendedItem>>();
                clustersByUserID = new FastByIDMap<FastIDSet>();
                allClusters = NO_CLUSTERS;
            }
        }

        private void findClusters(List<FastIDSet> newClusters)
        {
            if (clusteringByThreshold)
            {
                KeyValuePair<FastIDSet, FastIDSet> nearestPair = findNearestClusters(newClusters);
                FastIDSet _cluster1 = nearestPair.Key;
                FastIDSet _cluster2 = nearestPair.Value;

                if (_cluster1 != null && _cluster2 != null)
                {
                    FastIDSet cluster1 = _cluster1;
                    FastIDSet cluster2 = _cluster2;
                    while (clusterSimilarity.getSimilarity(cluster1, cluster2) >= clusteringThreshold)
                    {
                        newClusters.Remove(cluster1);
                        newClusters.Remove(cluster2);
                        FastIDSet merged = new FastIDSet(cluster1.size() + cluster2.size());
                        merged.addAll(cluster1);
                        merged.addAll(cluster2);
                        newClusters.Add(merged);
                        nearestPair = findNearestClusters(newClusters);
                        var __cluster1 = nearestPair.Key;
                        var __cluster2 = nearestPair.Value;
                        if (__cluster1 == null || __cluster2 == null)
                        {
                            break;
                        }
                        cluster1 = __cluster1;
                        cluster2 = __cluster2;
                    }
                }
            }
            else
            {
                while (newClusters.Count > numClusters)
                {
                    KeyValuePair<FastIDSet, FastIDSet> nearestPair = findNearestClusters(newClusters);
                    FastIDSet _cluster1 = nearestPair.Key;
                    FastIDSet _cluster2 = nearestPair.Value;
                    if (_cluster1 == null || _cluster2 == null)
                    {
                        break;
                    }
                    FastIDSet cluster1 = _cluster1;
                    FastIDSet cluster2 = _cluster2;
                    newClusters.Remove(cluster1);
                    newClusters.Remove(cluster2);
                    FastIDSet merged = new FastIDSet(cluster1.size() + cluster2.size());
                    merged.addAll(cluster1);
                    merged.addAll(cluster2);
                    newClusters.Add(merged);
                }
            }
        }

        private KeyValuePair<FastIDSet, FastIDSet> findNearestClusters(List<FastIDSet> clusters)
        {
            int size = clusters.Count;
            KeyValuePair<FastIDSet, FastIDSet> nearestPair = new KeyValuePair<FastIDSet, FastIDSet>();
            double bestSimilarity = Double.NegativeInfinity;
            for (int i = 0; i < size; i++)
            {
                FastIDSet cluster1 = clusters[i];
                for (int j = i + 1; j < size; j++)
                {
                    if (samplingRate >= 1.0 || random.nextDouble() < samplingRate)
                    {
                        FastIDSet cluster2 = clusters[j];
                        double similarity = clusterSimilarity.getSimilarity(cluster1, cluster2);
                        if (!Double.IsNaN(similarity) && similarity > bestSimilarity)
                        {
                            bestSimilarity = similarity;
                            nearestPair = new KeyValuePair<FastIDSet, FastIDSet>(cluster1, cluster2);
                        }
                    }
                }
            }
            return nearestPair;
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

            TopItems.Estimator<long> estimator = new Estimator(this, cluster);

            List<RecommendedItem> topItems =
                TopItems.getTopItems(possibleItemIDs.size(), possibleItemIDs.GetEnumerator(), null, estimator);

            log.debug("Recommendations are: {}", topItems);
            return topItems;
        }

        private static FastByIDMap<FastIDSet> computeClustersPerUserID(List<FastIDSet> clusters)
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

        public override void refresh(IList<taste.common.Refreshable> alreadyRefreshed)
        {
            refreshHelper.refresh(alreadyRefreshed);
        }

        public override String ToString()
        {
            return "TreeClusteringRecommender[clusterSimilarity:" + clusterSimilarity + ']';
        }

        private class Estimator : TopItems.Estimator<long>
        {
            private FastIDSet cluster;
            private TreeClusteringRecommender r;

            public Estimator(TreeClusteringRecommender _r, FastIDSet cluster)
            {
                this.cluster = cluster;
                this.r = _r;
            }

            public double estimate(long itemID)
            {
                DataModel dataModel = r.getDataModel();
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