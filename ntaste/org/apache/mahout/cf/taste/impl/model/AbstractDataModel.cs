namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Generic;

    public abstract class AbstractDataModel : DataModel, Refreshable
    {
        private float maxPreference = float.NaN;
        private float minPreference = float.NaN;

        protected AbstractDataModel()
        {
        }

        public abstract IEnumerator<long> getItemIDs();

        public abstract FastIDSet getItemIDsFromUser(long userID);

        public virtual float getMaxPreference()
        {
            return this.maxPreference;
        }

        public virtual float getMinPreference()
        {
            return this.minPreference;
        }

        public abstract int getNumItems();

        public abstract int getNumUsers();

        public abstract int getNumUsersWithPreferenceFor(long itemID);

        public abstract int getNumUsersWithPreferenceFor(long itemID1, long itemID2);

        public abstract PreferenceArray getPreferencesForItem(long itemID);

        public abstract PreferenceArray getPreferencesFromUser(long userID);

        public abstract DateTime? getPreferenceTime(long userID, long itemID);

        public abstract float? getPreferenceValue(long userID, long itemID);

        public abstract IEnumerator<long> getUserIDs();

        public abstract bool hasPreferenceValues();

        public abstract void refresh(IList<Refreshable> alreadyRefreshed);

        public abstract void removePreference(long userID, long itemID);

        protected virtual void setMaxPreference(float maxPreference)
        {
            this.maxPreference = maxPreference;
        }

        protected virtual void setMinPreference(float minPreference)
        {
            this.minPreference = minPreference;
        }

        public abstract void setPreference(long userID, long itemID, float value);
    }
}