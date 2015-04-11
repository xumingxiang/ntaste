namespace org.apache.mahout.cf.taste.recommender
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.model;
    using System.Collections.Generic;

    public interface Recommender : Refreshable
    {
        float estimatePreference(long userID, long itemID);

        DataModel getDataModel();

        List<RecommendedItem> recommend(long userID, int howMany);

        List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer);

        void removePreference(long userID, long itemID);

        void setPreference(long userID, long itemID, float value);
    }
}