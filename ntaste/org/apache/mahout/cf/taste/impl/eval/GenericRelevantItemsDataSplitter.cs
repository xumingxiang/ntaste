namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste.eval;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.model;
    using org.apache.mahout.cf.taste.model;
    using System.Collections.Generic;

    public sealed class GenericRelevantItemsDataSplitter : RelevantItemsDataSplitter
    {
        public FastIDSet getRelevantItemsIDs(long userID, int at, double relevanceThreshold, DataModel dataModel)
        {
            PreferenceArray array = dataModel.getPreferencesFromUser(userID);
            FastIDSet set = new FastIDSet(at);
            array.sortByValueReversed();
            for (int i = 0; (i < array.length()) && (set.size() < at); i++)
            {
                if (array.getValue(i) >= relevanceThreshold)
                {
                    set.add(array.getItemID(i));
                }
            }
            return set;
        }

        public void processOtherUser(long userID, FastIDSet relevantItemIDs, FastByIDMap<PreferenceArray> trainingUsers, long otherUserID, DataModel dataModel)
        {
            PreferenceArray array = dataModel.getPreferencesFromUser(otherUserID);
            if (userID == otherUserID)
            {
                List<Preference> prefs = new List<Preference>(array.length());
                foreach (Preference preference in array)
                {
                    if (!relevantItemIDs.contains(preference.getItemID()))
                    {
                        prefs.Add(preference);
                    }
                }
                if (prefs.Count > 0)
                {
                    trainingUsers.put(otherUserID, new GenericUserPreferenceArray(prefs));
                }
            }
            else
            {
                trainingUsers.put(otherUserID, array);
            }
        }
    }
}