namespace org.apache.mahout.cf.taste.model
{
    using System.Collections;
    using System.Collections.Generic;

    public interface PreferenceArray : IEnumerable<Preference>, IEnumerable
    {
        PreferenceArray clone();

        Preference get(int i);

        long[] getIDs();

        long getItemID(int i);

        long getUserID(int i);

        float getValue(int i);

        bool hasPrefWithItemID(long itemID);

        bool hasPrefWithUserID(long userID);

        int length();

        void set(int i, Preference pref);

        void setItemID(int i, long itemID);

        void setUserID(int i, long userID);

        void setValue(int i, float value);

        void sortByItem();

        void sortByUser();

        void sortByValue();

        void sortByValueReversed();
    }
}