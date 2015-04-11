namespace org.apache.mahout.cf.taste.impl.neighborhood
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.neighborhood;
    using System.Collections.Generic;

    public sealed class CachingUserNeighborhood : UserNeighborhood, Refreshable
    {
        private UserNeighborhood neighborhood;
        private Cache<long, long[]> neighborhoodCache;

        public CachingUserNeighborhood(UserNeighborhood neighborhood, DataModel dataModel)
        {
            this.neighborhood = neighborhood;
            int maxEntries = dataModel.getNumUsers();
            this.neighborhoodCache = new Cache<long, long[]>(new NeighborhoodRetriever(neighborhood), maxEntries);
        }

        public long[] getUserNeighborhood(long userID)
        {
            return this.neighborhoodCache.get(userID);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.neighborhoodCache.clear();
            RefreshHelper.maybeRefresh(RefreshHelper.buildRefreshed(alreadyRefreshed), this.neighborhood);
        }

        private sealed class NeighborhoodRetriever : Retriever<long, long[]>
        {
            private UserNeighborhood neighborhood;

            internal NeighborhoodRetriever(UserNeighborhood neighborhood)
            {
                this.neighborhood = neighborhood;
            }

            public long[] get(long key)
            {
                return this.neighborhood.getUserNeighborhood(key);
            }
        }
    }
}