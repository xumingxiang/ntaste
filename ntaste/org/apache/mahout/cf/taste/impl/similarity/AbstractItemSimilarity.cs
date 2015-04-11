namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System.Collections.Generic;

    public abstract class AbstractItemSimilarity : ItemSimilarity, Refreshable
    {
        private DataModel dataModel;
        private RefreshHelper refreshHelper;

        protected AbstractItemSimilarity(DataModel dataModel)
        {
            this.dataModel = dataModel;
            this.refreshHelper = new RefreshHelper(null);
            this.refreshHelper.addDependency(this.dataModel);
        }

        public virtual long[] allSimilarItemIDs(long itemID)
        {
            FastIDSet set = new FastIDSet();
            IEnumerator<long> enumerator = this.dataModel.getItemIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                if (!double.IsNaN(this.itemSimilarity(itemID, current)))
                {
                    set.add(current);
                }
            }
            return set.toArray();
        }

        protected DataModel getDataModel()
        {
            return this.dataModel;
        }

        public abstract double[] itemSimilarities(long itemID1, long[] itemID2s);

        public abstract double itemSimilarity(long itemID1, long itemID2);

        public virtual void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }
    }
}