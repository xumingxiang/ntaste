namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Generic;

    public abstract class AbstractFactorizer : Factorizer, Refreshable
    {
        private DataModel dataModel;
        private FastByIDMap<int?> itemIDMapping;
        private RefreshHelper refreshHelper;
        private FastByIDMap<int?> userIDMapping;

        protected AbstractFactorizer(DataModel dataModel)
        {
            Action refreshRunnable = null;
            this.dataModel = dataModel;
            this.buildMappings();
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.buildMappings();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(dataModel);
        }

        private void buildMappings()
        {
            this.userIDMapping = createIDMapping(this.dataModel.getNumUsers(), this.dataModel.getUserIDs());
            this.itemIDMapping = createIDMapping(this.dataModel.getNumItems(), this.dataModel.getItemIDs());
        }

        protected Factorization createFactorization(double[][] userFeatures, double[][] itemFeatures)
        {
            return new Factorization(this.userIDMapping, this.itemIDMapping, userFeatures, itemFeatures);
        }

        private static FastByIDMap<int?> createIDMapping(int size, IEnumerator<long> idIterator)
        {
            FastByIDMap<int?> map = new FastByIDMap<int?>(size);
            int num = 0;
            while (idIterator.MoveNext())
            {
                map.put(idIterator.Current, new int?(num++));
            }
            return map;
        }

        public abstract Factorization factorize();

        protected int itemIndex(long itemID)
        {
            int? nullable = this.itemIDMapping.get(itemID);
            if (!nullable.HasValue)
            {
                nullable = new int?(this.itemIDMapping.size());
                this.itemIDMapping.put(itemID, nullable);
            }
            return nullable.Value;
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        protected int userIndex(long userID)
        {
            int? nullable = this.userIDMapping.get(userID);
            if (!nullable.HasValue)
            {
                nullable = new int?(this.userIDMapping.size());
                this.userIDMapping.put(userID, nullable);
            }
            return nullable.Value;
        }
    }
}