namespace org.apache.mahout.cf.taste.eval
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;

    public interface RelevantItemsDataSplitter
    {
        FastIDSet getRelevantItemsIDs(long userID, int at, double relevanceThreshold, DataModel dataModel);

        void processOtherUser(long userID, FastIDSet relevantItemIDs, FastByIDMap<PreferenceArray> trainingUsers, long otherUserID, DataModel dataModel);
    }
}