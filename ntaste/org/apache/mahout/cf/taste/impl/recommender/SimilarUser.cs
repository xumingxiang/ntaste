namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.common;
    using System;

    public sealed class SimilarUser : IComparable<SimilarUser>
    {
        private double similarity;
        private long userID;

        public SimilarUser(long userID, double similarity)
        {
            this.userID = userID;
            this.similarity = similarity;
        }

        public int CompareTo(SimilarUser other)
        {
            double num = other.getSimilarity();
            if (this.similarity > num)
            {
                return -1;
            }
            if (this.similarity < num)
            {
                return 1;
            }
            long num2 = other.getUserID();
            if (this.userID < num2)
            {
                return -1;
            }
            if (this.userID > num2)
            {
                return 1;
            }
            return 0;
        }

        public override bool Equals(object o)
        {
            if (!(o is SimilarUser))
            {
                return false;
            }
            SimilarUser user = (SimilarUser)o;
            return ((this.userID == user.getUserID()) && (this.similarity == user.getSimilarity()));
        }

        public override int GetHashCode()
        {
            return (((int)this.userID) ^ RandomUtils.hashDouble(this.similarity));
        }

        public double getSimilarity()
        {
            return this.similarity;
        }

        public long getUserID()
        {
            return this.userID;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "SimilarUser[user:", this.userID, ", similarity:", this.similarity, ']' });
        }
    }
}