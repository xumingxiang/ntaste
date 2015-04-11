namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class GenericBooleanPrefDataModel : AbstractDataModel
    {
        private long[] itemIDs;
        private FastByIDMap<FastIDSet> preferenceForItems;
        private FastByIDMap<FastIDSet> preferenceFromUsers;
        private FastByIDMap<FastByIDMap<DateTime?>> timestamps;
        private long[] userIDs;

        public GenericBooleanPrefDataModel(FastByIDMap<FastIDSet> userData)
            : this(userData, null)
        {
        }

        [Obsolete]
        public GenericBooleanPrefDataModel(DataModel dataModel)
            : this(toDataMap(dataModel))
        {
        }

        public GenericBooleanPrefDataModel(FastByIDMap<FastIDSet> userData, FastByIDMap<FastByIDMap<DateTime?>> timestamps)
        {
            this.preferenceFromUsers = userData;
            this.preferenceForItems = new FastByIDMap<FastIDSet>();
            FastIDSet set = new FastIDSet();
            foreach (KeyValuePair<long, FastIDSet> pair in this.preferenceFromUsers.entrySet())
            {
                long key = pair.Key;
                FastIDSet c = pair.Value;
                set.addAll(c);
                IEnumerator<long> enumerator = c.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    long current = enumerator.Current;
                    FastIDSet set3 = this.preferenceForItems.get(current);
                    if (set3 == null)
                    {
                        set3 = new FastIDSet(2);
                        this.preferenceForItems.put(current, set3);
                    }
                    set3.add(key);
                }
            }
            this.itemIDs = set.toArray();
            set = null;
            Array.Sort<long>(this.itemIDs);
            this.userIDs = new long[userData.size()];
            int num3 = 0;
            IEnumerator<long> enumerator2 = userData.Keys.GetEnumerator();
            while (enumerator2.MoveNext())
            {
                this.userIDs[num3++] = enumerator2.Current;
            }
            Array.Sort<long>(this.userIDs);
            this.timestamps = timestamps;
        }

        public override IEnumerator<long> getItemIDs()
        {
            return ((IEnumerable<long>)this.itemIDs).GetEnumerator();
        }

        public override FastIDSet getItemIDsFromUser(long userID)
        {
            FastIDSet set = this.preferenceFromUsers.get(userID);
            if (set == null)
            {
                throw new NoSuchUserException(userID);
            }
            return set;
        }

        public override int getNumItems()
        {
            return this.itemIDs.Length;
        }

        public override int getNumUsers()
        {
            return this.userIDs.Length;
        }

        public override int getNumUsersWithPreferenceFor(long itemID)
        {
            FastIDSet set = this.preferenceForItems.get(itemID);
            return ((set == null) ? 0 : set.size());
        }

        public override int getNumUsersWithPreferenceFor(long itemID1, long itemID2)
        {
            FastIDSet other = this.preferenceForItems.get(itemID1);
            if (other == null)
            {
                return 0;
            }
            FastIDSet set2 = this.preferenceForItems.get(itemID2);
            if (set2 == null)
            {
                return 0;
            }
            return ((other.size() < set2.size()) ? set2.intersectionSize(other) : other.intersectionSize(set2));
        }

        public override PreferenceArray getPreferencesForItem(long itemID)
        {
            FastIDSet set = this.preferenceForItems.get(itemID);
            if (set == null)
            {
                throw new NoSuchItemException(itemID);
            }
            PreferenceArray array = new BooleanItemPreferenceArray(set.size());
            int i = 0;
            IEnumerator<long> enumerator = set.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.setUserID(i, enumerator.Current);
                array.setItemID(i, itemID);
                i++;
            }
            return array;
        }

        public override PreferenceArray getPreferencesFromUser(long userID)
        {
            FastIDSet set = this.preferenceFromUsers.get(userID);
            if (set == null)
            {
                throw new NoSuchUserException(userID);
            }
            PreferenceArray array = new BooleanUserPreferenceArray(set.size());
            int i = 0;
            IEnumerator<long> enumerator = set.GetEnumerator();
            while (enumerator.MoveNext())
            {
                array.setUserID(i, userID);
                array.setItemID(i, enumerator.Current);
                i++;
            }
            return array;
        }

        public override DateTime? getPreferenceTime(long userID, long itemID)
        {
            if (this.timestamps == null)
            {
                return null;
            }
            FastByIDMap<DateTime?> map = this.timestamps.get(userID);
            if (map == null)
            {
                throw new NoSuchUserException(userID);
            }
            return map.get(itemID);
        }

        public override float? getPreferenceValue(long userID, long itemID)
        {
            FastIDSet set = this.preferenceFromUsers.get(userID);
            if (set == null)
            {
                throw new NoSuchUserException(userID);
            }
            if (set.contains(itemID))
            {
                return 1f;
            }
            return null;
        }

        public FastByIDMap<FastIDSet> getRawItemData()
        {
            return this.preferenceForItems;
        }

        public FastByIDMap<FastIDSet> getRawUserData()
        {
            return this.preferenceFromUsers;
        }

        public override IEnumerator<long> getUserIDs()
        {
            return ((IEnumerable<long>)this.userIDs).GetEnumerator();
        }

        public override bool hasPreferenceValues()
        {
            return false;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
        }

        public override void removePreference(long userID, long itemID)
        {
            throw new NotSupportedException();
        }

        public override void setPreference(long userID, long itemID, float value)
        {
            throw new NotSupportedException();
        }

        public static FastByIDMap<FastIDSet> toDataMap(FastByIDMap<PreferenceArray> data)
        {
            FastByIDMap<FastIDSet> map = new FastByIDMap<FastIDSet>(data.size());
            foreach (KeyValuePair<long, PreferenceArray> pair in data.entrySet())
            {
                PreferenceArray array = pair.Value;
                int size = array.length();
                FastIDSet set = new FastIDSet(size);
                for (int i = 0; i < size; i++)
                {
                    set.add(array.getItemID(i));
                }
                map.put(pair.Key, set);
            }
            return map;
        }

        public static FastByIDMap<FastIDSet> toDataMap(DataModel dataModel)
        {
            FastByIDMap<FastIDSet> map = new FastByIDMap<FastIDSet>(dataModel.getNumUsers());
            IEnumerator<long> enumerator = dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                map.put(current, dataModel.getItemIDsFromUser(current));
            }
            return map;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(200);
            builder.Append("GenericBooleanPrefDataModel[users:");
            for (int i = 0; i < Math.Min(3, this.userIDs.Length); i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(this.userIDs[i]);
            }
            if (this.userIDs.Length > 3)
            {
                builder.Append("...");
            }
            builder.Append(']');
            return builder.ToString();
        }
    }
}