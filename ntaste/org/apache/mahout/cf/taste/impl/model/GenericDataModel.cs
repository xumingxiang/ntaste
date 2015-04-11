namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public sealed class GenericDataModel : AbstractDataModel
    {
        private long[] itemIDs;
        private static Logger log = LoggerFactory.getLogger(typeof(GenericDataModel));
        private FastByIDMap<PreferenceArray> preferenceForItems;
        private FastByIDMap<PreferenceArray> preferenceFromUsers;
        private FastByIDMap<FastByIDMap<DateTime?>> timestamps;
        private long[] userIDs;

        public GenericDataModel(FastByIDMap<PreferenceArray> userData)
            : this(userData, null)
        {
        }

        [Obsolete]
        public GenericDataModel(DataModel dataModel)
            : this(toDataMap(dataModel))
        {
        }

        public GenericDataModel(FastByIDMap<PreferenceArray> userData, FastByIDMap<FastByIDMap<DateTime?>> timestamps)
        {
            this.preferenceFromUsers = userData;
            FastByIDMap<List<Preference>> data = new FastByIDMap<List<Preference>>();
            FastIDSet set = new FastIDSet();
            int num = 0;
            float negativeInfinity = float.NegativeInfinity;
            float positiveInfinity = float.PositiveInfinity;
            foreach (KeyValuePair<long, PreferenceArray> pair in this.preferenceFromUsers.entrySet())
            {
                PreferenceArray array = pair.Value;
                array.sortByItem();
                foreach (Preference preference in array)
                {
                    long key = preference.getItemID();
                    set.add(key);
                    List<Preference> list = data.get(key);
                    if (list == null)
                    {
                        list = new List<Preference>(2);
                        data.put(key, list);
                    }
                    list.Add(preference);
                    float num5 = preference.getValue();
                    if (num5 > negativeInfinity)
                    {
                        negativeInfinity = num5;
                    }
                    if (num5 < positiveInfinity)
                    {
                        positiveInfinity = num5;
                    }
                }
                if ((++num % 0x2710) == 0)
                {
                    log.info("Processed {0} users", new object[] { num });
                }
            }
            log.info("Processed {0} users", new object[] { num });
            this.setMinPreference(positiveInfinity);
            this.setMaxPreference(negativeInfinity);
            this.itemIDs = set.toArray();
            set = null;
            Array.Sort<long>(this.itemIDs);
            this.preferenceForItems = toDataMap(data, false);
            foreach (KeyValuePair<long, PreferenceArray> pair in this.preferenceForItems.entrySet())
            {
                pair.Value.sortByUser();
            }
            this.userIDs = new long[userData.size()];
            int num6 = 0;
            foreach (long num7 in userData.Keys)
            {
                this.userIDs[num6++] = num7;
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
            PreferenceArray array = this.getPreferencesFromUser(userID);
            int size = array.length();
            FastIDSet set = new FastIDSet(size);
            for (int i = 0; i < size; i++)
            {
                set.add(array.getItemID(i));
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
            PreferenceArray array = this.preferenceForItems.get(itemID);
            return ((array == null) ? 0 : array.length());
        }

        public override int getNumUsersWithPreferenceFor(long itemID1, long itemID2)
        {
            PreferenceArray array = this.preferenceForItems.get(itemID1);
            if (array == null)
            {
                return 0;
            }
            PreferenceArray array2 = this.preferenceForItems.get(itemID2);
            if (array2 == null)
            {
                return 0;
            }
            int num = array.length();
            int num2 = array2.length();
            int num3 = 0;
            int i = 0;
            int num5 = 0;
            long num6 = array.getUserID(0);
            long num7 = array2.getUserID(0);
            while (true)
            {
                if (num6 < num7)
                {
                    if (++i == num)
                    {
                        return num3;
                    }
                    num6 = array.getUserID(i);
                }
                else if (num6 > num7)
                {
                    if (++num5 == num2)
                    {
                        return num3;
                    }
                    num7 = array2.getUserID(num5);
                }
                else
                {
                    num3++;
                    if ((++i == num) || (++num5 == num2))
                    {
                        return num3;
                    }
                    num6 = array.getUserID(i);
                    num7 = array2.getUserID(num5);
                }
            }
        }

        public override PreferenceArray getPreferencesForItem(long itemID)
        {
            PreferenceArray array = this.preferenceForItems.get(itemID);
            if (array == null)
            {
                throw new NoSuchItemException(itemID);
            }
            return array;
        }

        public override PreferenceArray getPreferencesFromUser(long userID)
        {
            PreferenceArray array = this.preferenceFromUsers.get(userID);
            if (array == null)
            {
                throw new NoSuchUserException(userID);
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
            PreferenceArray array = this.getPreferencesFromUser(userID);
            int num = array.length();
            for (int i = 0; i < num; i++)
            {
                if (array.getItemID(i) == itemID)
                {
                    return new float?(array.getValue(i));
                }
            }
            return null;
        }

        public FastByIDMap<PreferenceArray> getRawItemData()
        {
            return this.preferenceForItems;
        }

        public FastByIDMap<PreferenceArray> getRawUserData()
        {
            return this.preferenceFromUsers;
        }

        public override IEnumerator<long> getUserIDs()
        {
            return ((IEnumerable<long>)this.userIDs).GetEnumerator();
        }

        public override bool hasPreferenceValues()
        {
            return true;
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

        public static FastByIDMap<PreferenceArray> toDataMap(DataModel dataModel)
        {
            FastByIDMap<PreferenceArray> map = new FastByIDMap<PreferenceArray>(dataModel.getNumUsers());
            IEnumerator<long> enumerator = dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                map.put(current, dataModel.getPreferencesFromUser(current));
            }
            return map;
        }

        public static FastByIDMap<PreferenceArray> toDataMap(FastByIDMap<List<Preference>> data, bool byUser)
        {
            FastByIDMap<PreferenceArray> map = new FastByIDMap<PreferenceArray>(data.size());
            foreach (KeyValuePair<long, List<Preference>> pair in data.entrySet())
            {
                List<Preference> prefs = pair.Value;
                map.put(pair.Key, byUser ? ((PreferenceArray)new GenericUserPreferenceArray(prefs)) : ((PreferenceArray)new GenericItemPreferenceArray(prefs)));
            }
            return map;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(200);
            builder.Append("GenericDataModel[users:");
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