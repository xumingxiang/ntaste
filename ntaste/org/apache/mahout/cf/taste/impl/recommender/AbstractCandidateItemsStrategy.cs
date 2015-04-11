namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System.Collections.Generic;

    public abstract class AbstractCandidateItemsStrategy : CandidateItemsStrategy, MostSimilarItemsCandidateItemsStrategy, Refreshable
    {
        protected AbstractCandidateItemsStrategy()
        {
        }

        protected abstract FastIDSet doGetCandidateItems(long[] preferredItemIDs, DataModel dataModel);

        public FastIDSet getCandidateItems(long[] itemIDs, DataModel dataModel)
        {
            return this.doGetCandidateItems(itemIDs, dataModel);
        }

        public FastIDSet getCandidateItems(long userID, PreferenceArray preferencesFromUser, DataModel dataModel)
        {
            return this.doGetCandidateItems(preferencesFromUser.getIDs(), dataModel);
        }

        public virtual void refresh(IList<Refreshable> alreadyRefreshed)
        {
        }
    }
}