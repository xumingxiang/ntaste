namespace org.apache.mahout.cf.taste.impl.neighborhood
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.recommender;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System.Collections.Generic;

    public sealed class NearestNUserNeighborhood : AbstractUserNeighborhood
    {
        private double minSimilarity;
        private int n;

        public NearestNUserNeighborhood(int n, UserSimilarity userSimilarity, DataModel dataModel)
            : this(n, double.NegativeInfinity, userSimilarity, dataModel, 1.0)
        {
        }

        public NearestNUserNeighborhood(int n, double minSimilarity, UserSimilarity userSimilarity, DataModel dataModel)
            : this(n, minSimilarity, userSimilarity, dataModel, 1.0)
        {
        }

        public NearestNUserNeighborhood(int n, double minSimilarity, UserSimilarity userSimilarity, DataModel dataModel, double samplingRate)
            : base(userSimilarity, dataModel, samplingRate)
        {
            int num = dataModel.getNumUsers();
            this.n = (n > num) ? num : n;
            this.minSimilarity = minSimilarity;
        }

        public override long[] getUserNeighborhood(long userID)
        {
            DataModel model = this.getDataModel();
            TopItems.Estimator<long> estimator = new Estimator(this.getUserSimilarity(), userID, this.minSimilarity);
            IEnumerator<long> allUserIDs = SamplingLongPrimitiveIterator.maybeWrapIterator(model.getUserIDs(), this.getSamplingRate());
            return TopItems.getTopUsers(this.n, allUserIDs, null, estimator);
        }

        public override string ToString()
        {
            return "NearestNUserNeighborhood";
        }

        private sealed class Estimator : TopItems.Estimator<long>
        {
            private double minSim;
            private long theUserID;
            private UserSimilarity userSimilarityImpl;

            internal Estimator(UserSimilarity userSimilarityImpl, long theUserID, double minSim)
            {
                this.userSimilarityImpl = userSimilarityImpl;
                this.theUserID = theUserID;
                this.minSim = minSim;
            }

            public double estimate(long userID)
            {
                if (userID == this.theUserID)
                {
                    return double.NaN;
                }
                double num = this.userSimilarityImpl.userSimilarity(this.theUserID, userID);
                return ((num >= this.minSim) ? num : double.NaN);
            }
        }
    }
}