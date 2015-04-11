namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public sealed class PlusAnonymousConcurrentUserDataModel : PlusAnonymousUserDataModel
    {
        private static Logger log = LoggerFactory.getLogger(typeof(PlusAnonymousUserDataModel));
        private IDictionary<long, FastIDSet> prefItemIDs;
        private IDictionary<long, PreferenceArray> tempPrefs;
        private ConcurrentQueue<long> usersPool;

        public PlusAnonymousConcurrentUserDataModel(DataModel _delegate, int maxConcurrentUsers)
            : base(_delegate)
        {
            this.tempPrefs = new ConcurrentDictionary<long, PreferenceArray>();
            this.prefItemIDs = new ConcurrentDictionary<long, FastIDSet>();
            this.initializeUsersPools(maxConcurrentUsers);
        }

        public void clearTempPrefs(long anonymousUserID)
        {
            this.tempPrefs.Remove(anonymousUserID);
            this.prefItemIDs.Remove(anonymousUserID);
        }

        public override FastIDSet getItemIDsFromUser(long userID)
        {
            if (this.isAnonymousUser(userID))
            {
                return this.prefItemIDs[userID];
            }
            return base.getDelegate().getItemIDsFromUser(userID);
        }

        public override int getNumUsers()
        {
            return base.getDelegate().getNumUsers();
        }

        public override int getNumUsersWithPreferenceFor(long itemID)
        {
            if (this.tempPrefs.Count == 0)
            {
                return base.getDelegate().getNumUsersWithPreferenceFor(itemID);
            }
            int num = 0;
            foreach (KeyValuePair<long, PreferenceArray> pair in this.tempPrefs)
            {
                for (int i = 0; i < pair.Value.length(); i++)
                {
                    if (pair.Value.getItemID(i) == itemID)
                    {
                        num++;
                        break;
                    }
                }
            }
            return (base.getDelegate().getNumUsersWithPreferenceFor(itemID) + num);
        }

        public override int getNumUsersWithPreferenceFor(long itemID1, long itemID2)
        {
            if (this.tempPrefs.Count == 0)
            {
                return base.getDelegate().getNumUsersWithPreferenceFor(itemID1, itemID2);
            }
            int num = 0;
            foreach (KeyValuePair<long, PreferenceArray> pair in this.tempPrefs)
            {
                bool flag = false;
                bool flag2 = false;
                for (int i = 0; (i < pair.Value.length()) && (!flag || !flag2); i++)
                {
                    long num3 = pair.Value.getItemID(i);
                    if (num3 == itemID1)
                    {
                        flag = true;
                    }
                    if (num3 == itemID2)
                    {
                        flag2 = true;
                    }
                }
                if (flag && flag2)
                {
                    num++;
                }
            }
            return (base.getDelegate().getNumUsersWithPreferenceFor(itemID1, itemID2) + num);
        }

        public override PreferenceArray getPreferencesForItem(long itemID)
        {
            int num;
            if (this.tempPrefs.Count == 0)
            {
                return base.getDelegate().getPreferencesForItem(itemID);
            }
            PreferenceArray array = null;
            try
            {
                array = base.getDelegate().getPreferencesForItem(itemID);
            }
            catch (NoSuchItemException)
            {
            }
            List<Preference> list = new List<Preference>();
            foreach (KeyValuePair<long, PreferenceArray> pair in this.tempPrefs)
            {
                PreferenceArray array2 = pair.Value;
                num = 0;
                while (num < array2.length())
                {
                    if (array2.getItemID(num) == itemID)
                    {
                        list.Add(array2.get(num));
                    }
                    num++;
                }
            }
            int num2 = (array == null) ? 0 : array.length();
            int count = list.Count;
            int num4 = 0;
            PreferenceArray array3 = new GenericItemPreferenceArray(num2 + count);
            for (num = 0; num < num2; num++)
            {
                array3.set(num4++, array.get(num));
            }
            foreach (Preference preference in list)
            {
                array3.set(num4++, preference);
            }
            if (array3.length() == 0)
            {
                throw new NoSuchItemException(itemID);
            }
            return array3;
        }

        public override PreferenceArray getPreferencesFromUser(long userID)
        {
            if (this.isAnonymousUser(userID))
            {
                return this.tempPrefs[userID];
            }
            return base.getDelegate().getPreferencesFromUser(userID);
        }

        public override DateTime? getPreferenceTime(long userID, long itemID)
        {
            if (this.isAnonymousUser(userID))
            {
                return null;
            }
            return base.getDelegate().getPreferenceTime(userID, itemID);
        }

        public override float? getPreferenceValue(long userID, long itemID)
        {
            if (this.isAnonymousUser(userID))
            {
                PreferenceArray array = this.tempPrefs[userID];
                for (int i = 0; i < array.length(); i++)
                {
                    if (array.getItemID(i) == itemID)
                    {
                        return new float?(array.getValue(i));
                    }
                }
                return null;
            }
            return base.getDelegate().getPreferenceValue(userID, itemID);
        }

        public override IEnumerator<long> getUserIDs()
        {
            return base.getDelegate().getUserIDs();
        }

        private void initializeUsersPools(int usersPoolSize)
        {
            this.usersPool = new ConcurrentQueue<long>();
            for (int i = 0; i < usersPoolSize; i++)
            {
                this.usersPool.Enqueue(-9223372036854775808L + i);
            }
        }

        private bool isAnonymousUser(long userID)
        {
            return this.tempPrefs.ContainsKey(userID);
        }

        public bool releaseUser(long userID)
        {
            if (this.tempPrefs.ContainsKey(userID))
            {
                this.clearTempPrefs(userID);
                this.usersPool.Enqueue(userID);
                return true;
            }
            return false;
        }

        public override void removePreference(long userID, long itemID)
        {
            if (this.isAnonymousUser(userID))
            {
                throw new NotSupportedException();
            }
            base.getDelegate().removePreference(userID, itemID);
        }

        public override void setPreference(long userID, long itemID, float value)
        {
            if (this.isAnonymousUser(userID))
            {
                throw new NotSupportedException();
            }
            base.getDelegate().setPreference(userID, itemID, value);
        }

        public void setTempPrefs(PreferenceArray prefs, long anonymousUserID)
        {
            this.tempPrefs[anonymousUserID] = prefs;
            FastIDSet set = new FastIDSet();
            for (int i = 0; i < prefs.length(); i++)
            {
                set.add(prefs.getItemID(i));
            }
            this.prefItemIDs[anonymousUserID] = set;
        }

        public long? takeAvailableUser()
        {
            long num;
            if (this.usersPool.TryDequeue(out num))
            {
                this.tempPrefs[num] = new GenericUserPreferenceArray(0);
                return new long?(num);
            }
            return null;
        }
    }
}