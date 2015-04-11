namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;

    public sealed class PreferredItemsNeighborhoodCandidateItemsStrategy : AbstractCandidateItemsStrategy
    {
        protected override FastIDSet doGetCandidateItems(long[] preferredItemIDs, DataModel dataModel)
        {
            FastIDSet set = new FastIDSet();
            foreach (long num in preferredItemIDs)
            {
                PreferenceArray array = dataModel.getPreferencesForItem(num);
                int num2 = array.length();
                for (int i = 0; i < num2; i++)
                {
                    set.addAll(dataModel.getItemIDsFromUser(array.getUserID(i)));
                }
            }
            set.removeAll(preferredItemIDs);
            return set;
        }
    }
}