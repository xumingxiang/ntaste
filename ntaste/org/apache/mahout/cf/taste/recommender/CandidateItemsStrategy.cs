namespace org.apache.mahout.cf.taste.recommender
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;

    public interface CandidateItemsStrategy : Refreshable
    {
        FastIDSet getCandidateItems(long userID, PreferenceArray preferencesFromUser, DataModel dataModel);
    }
}