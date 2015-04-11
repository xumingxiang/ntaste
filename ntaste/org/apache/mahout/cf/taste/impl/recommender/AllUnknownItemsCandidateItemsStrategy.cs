namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System.Collections.Generic;

    public sealed class AllUnknownItemsCandidateItemsStrategy : AbstractCandidateItemsStrategy
    {
        protected override FastIDSet doGetCandidateItems(long[] preferredItemIDs, DataModel dataModel)
        {
            FastIDSet set = new FastIDSet(dataModel.getNumItems());
            IEnumerator<long> enumerator = dataModel.getItemIDs();
            while (enumerator.MoveNext())
            {
                set.add(enumerator.Current);
            }
            set.removeAll(preferredItemIDs);
            return set;
        }
    }
}