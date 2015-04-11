namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using System.Collections.Generic;

    public class Factorization
    {
        private double[][] itemFeatures;
        private FastByIDMap<int?> itemIDMapping;
        private double[][] userFeatures;
        private FastByIDMap<int?> userIDMapping;

        public Factorization(FastByIDMap<int?> userIDMapping, FastByIDMap<int?> itemIDMapping, double[][] userFeatures, double[][] itemFeatures)
        {
            this.userIDMapping = userIDMapping;
            this.itemIDMapping = itemIDMapping;
            this.userFeatures = userFeatures;
            this.itemFeatures = itemFeatures;
        }

        public double[][] allItemFeatures()
        {
            return this.itemFeatures;
        }

        public double[][] allUserFeatures()
        {
            return this.userFeatures;
        }

        public override bool Equals(object o)
        {
            if (o is Factorization)
            {
                Factorization factorization = (Factorization)o;
                return (((this.userIDMapping.Equals(factorization.userIDMapping) && this.itemIDMapping.Equals(factorization.itemIDMapping)) && Utils.ArrayDeepEquals(this.userFeatures, factorization.userFeatures)) && Utils.ArrayDeepEquals(this.itemFeatures, factorization.itemFeatures));
            }
            return false;
        }

        public override int GetHashCode()
        {
            int num = (0x1f * this.userIDMapping.GetHashCode()) + this.itemIDMapping.GetHashCode();
            num = (0x1f * num) + Utils.GetArrayDeepHashCode(this.userFeatures);
            return ((0x1f * num) + Utils.GetArrayDeepHashCode(this.itemFeatures));
        }

        public virtual double[] getItemFeatures(long itemID)
        {
            int? nullable = this.itemIDMapping.get(itemID);
            if (!nullable.HasValue)
            {
                throw new NoSuchItemException(itemID);
            }
            return this.itemFeatures[nullable.Value];
        }

        public IEnumerable<KeyValuePair<long, int?>> getItemIDMappings()
        {
            return this.itemIDMapping.entrySet();
        }

        public virtual double[] getUserFeatures(long userID)
        {
            int? nullable = this.userIDMapping.get(userID);
            if (!nullable.HasValue)
            {
                throw new NoSuchUserException(userID);
            }
            return this.userFeatures[nullable.Value];
        }

        public IEnumerable<KeyValuePair<long, int?>> getUserIDMappings()
        {
            return this.userIDMapping.entrySet();
        }

        public int itemIndex(long itemID)
        {
            int? nullable = this.itemIDMapping.get(itemID);
            if (!nullable.HasValue)
            {
                throw new NoSuchItemException(itemID);
            }
            return nullable.Value;
        }

        public int numFeatures()
        {
            return ((this.userFeatures.Length > 0) ? this.userFeatures[0].Length : 0);
        }

        public int numItems()
        {
            return this.itemIDMapping.size();
        }

        public int numUsers()
        {
            return this.userIDMapping.size();
        }

        public int userIndex(long userID)
        {
            int? nullable = this.userIDMapping.get(userID);
            if (!nullable.HasValue)
            {
                throw new NoSuchUserException(userID);
            }
            return nullable.Value;
        }
    }
}