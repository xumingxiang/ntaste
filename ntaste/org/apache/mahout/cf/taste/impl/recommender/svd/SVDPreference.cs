namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste.impl.model;

    internal sealed class SVDPreference : GenericPreference
    {
        private double cache;

        public SVDPreference(long userID, long itemID, float value, double cache)
            : base(userID, itemID, value)
        {
            this.setCache(cache);
        }

        public double getCache()
        {
            return this.cache;
        }

        public void setCache(double value)
        {
            this.cache = value;
        }
    }
}