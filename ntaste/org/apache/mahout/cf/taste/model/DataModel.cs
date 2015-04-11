namespace org.apache.mahout.cf.taste.model
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using System;
    using System.Collections.Generic;

    public interface DataModel : Refreshable
    {
        IEnumerator<long> getItemIDs();

        FastIDSet getItemIDsFromUser(long userID);

        float getMaxPreference();

        float getMinPreference();

        int getNumItems();

        int getNumUsers();

        int getNumUsersWithPreferenceFor(long itemID);

        int getNumUsersWithPreferenceFor(long itemID1, long itemID2);

        PreferenceArray getPreferencesForItem(long itemID);

        PreferenceArray getPreferencesFromUser(long userID);

        DateTime? getPreferenceTime(long userID, long itemID);

        float? getPreferenceValue(long userID, long itemID);

        IEnumerator<long> getUserIDs();

        bool hasPreferenceValues();

        void removePreference(long userID, long itemID);

        void setPreference(long userID, long itemID, float value);
    }
}