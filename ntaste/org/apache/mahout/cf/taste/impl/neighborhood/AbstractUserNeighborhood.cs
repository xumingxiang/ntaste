namespace org.apache.mahout.cf.taste.impl.neighborhood
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.neighborhood;
    using org.apache.mahout.cf.taste.similarity;
    using System.Collections.Generic;

    public abstract class AbstractUserNeighborhood : UserNeighborhood, Refreshable
    {
        private DataModel dataModel;
        private RefreshHelper refreshHelper;
        private double samplingRate;
        private UserSimilarity userSimilarity;

        public AbstractUserNeighborhood(UserSimilarity userSimilarity, DataModel dataModel, double samplingRate)
        {
            this.userSimilarity = userSimilarity;
            this.dataModel = dataModel;
            this.samplingRate = samplingRate;
            this.refreshHelper = new RefreshHelper(null);
            this.refreshHelper.addDependency(this.dataModel);
            this.refreshHelper.addDependency(this.userSimilarity);
        }

        public virtual DataModel getDataModel()
        {
            return this.dataModel;
        }

        public virtual double getSamplingRate()
        {
            return this.samplingRate;
        }

        public abstract long[] getUserNeighborhood(long userID);

        public virtual UserSimilarity getUserSimilarity()
        {
            return this.userSimilarity;
        }

        public virtual void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }
    }
}