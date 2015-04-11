namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Generic;

    public class PlusAnonymousUserDataModel : DataModel, Refreshable
    {
        private DataModel _delegate;
        private static Logger log = LoggerFactory.getLogger(typeof(PlusAnonymousUserDataModel));
        private FastIDSet prefItemIDs;
        public const long TEMP_USER_ID = -9223372036854775808L;
        private PreferenceArray tempPrefs;

        public PlusAnonymousUserDataModel(DataModel deleg)
        {
            this._delegate = deleg;
            this.prefItemIDs = new FastIDSet();
        }

        public void clearTempPrefs()
        {
            this.tempPrefs = null;
            this.prefItemIDs.clear();
        }

        private static PreferenceArray cloneAndMergeInto(PreferenceArray delegatePrefs, long itemID, long newUserID, float value)
        {
            int num4;
            int num = (delegatePrefs == null) ? 0 : delegatePrefs.length();
            int size = num + 1;
            PreferenceArray array = new GenericItemPreferenceArray(size);
            array.setItemID(0, itemID);
            int i = 0;
            while ((i < num) && (newUserID > delegatePrefs.getUserID(i)))
            {
                i++;
            }
            for (num4 = 0; num4 < i; num4++)
            {
                array.setUserID(num4, delegatePrefs.getUserID(num4));
                array.setValue(num4, delegatePrefs.getValue(num4));
            }
            array.setUserID(i, newUserID);
            array.setValue(i, value);
            for (num4 = i + 1; num4 < size; num4++)
            {
                array.setUserID(num4, delegatePrefs.getUserID(num4 - 1));
                array.setValue(num4, delegatePrefs.getValue(num4 - 1));
            }
            return array;
        }

        protected DataModel getDelegate()
        {
            return this._delegate;
        }

        public virtual IEnumerator<long> getItemIDs()
        {
            return this._delegate.getItemIDs();
        }

        public virtual FastIDSet getItemIDsFromUser(long userID)
        {
            if (userID == -9223372036854775808L)
            {
                if (this.tempPrefs == null)
                {
                    throw new NoSuchUserException(-9223372036854775808L);
                }
                return this.prefItemIDs;
            }
            return this._delegate.getItemIDsFromUser(userID);
        }

        public virtual float getMaxPreference()
        {
            return this._delegate.getMaxPreference();
        }

        public virtual float getMinPreference()
        {
            return this._delegate.getMinPreference();
        }

        public virtual int getNumItems()
        {
            return this._delegate.getNumItems();
        }

        public virtual int getNumUsers()
        {
            return (this._delegate.getNumUsers() + ((this.tempPrefs == null) ? 0 : 1));
        }

        public virtual int getNumUsersWithPreferenceFor(long itemID)
        {
            if (this.tempPrefs == null)
            {
                return this._delegate.getNumUsersWithPreferenceFor(itemID);
            }
            bool flag = false;
            for (int i = 0; i < this.tempPrefs.length(); i++)
            {
                if (this.tempPrefs.getItemID(i) == itemID)
                {
                    flag = true;
                    break;
                }
            }
            return (this._delegate.getNumUsersWithPreferenceFor(itemID) + (flag ? 1 : 0));
        }

        public virtual int getNumUsersWithPreferenceFor(long itemID1, long itemID2)
        {
            if (this.tempPrefs == null)
            {
                return this._delegate.getNumUsersWithPreferenceFor(itemID1, itemID2);
            }
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; (i < this.tempPrefs.length()) && (!flag || !flag2); i++)
            {
                long num2 = this.tempPrefs.getItemID(i);
                if (num2 == itemID1)
                {
                    flag = true;
                }
                if (num2 == itemID2)
                {
                    flag2 = true;
                }
            }
            return (this._delegate.getNumUsersWithPreferenceFor(itemID1, itemID2) + ((flag && flag2) ? 1 : 0));
        }

        public virtual PreferenceArray getPreferencesForItem(long itemID)
        {
            if (this.tempPrefs == null)
            {
                return this._delegate.getPreferencesForItem(itemID);
            }
            PreferenceArray delegatePrefs = null;
            try
            {
                delegatePrefs = this._delegate.getPreferencesForItem(itemID);
            }
            catch (NoSuchItemException)
            {
                log.debug("Item {} unknown", new object[] { itemID });
            }
            for (int i = 0; i < this.tempPrefs.length(); i++)
            {
                if (this.tempPrefs.getItemID(i) == itemID)
                {
                    return cloneAndMergeInto(delegatePrefs, itemID, this.tempPrefs.getUserID(i), this.tempPrefs.getValue(i));
                }
            }
            if (delegatePrefs == null)
            {
                throw new NoSuchItemException(itemID);
            }
            return delegatePrefs;
        }

        public virtual PreferenceArray getPreferencesFromUser(long userID)
        {
            if (userID == -9223372036854775808L)
            {
                if (this.tempPrefs == null)
                {
                    throw new NoSuchUserException(-9223372036854775808L);
                }
                return this.tempPrefs;
            }
            return this._delegate.getPreferencesFromUser(userID);
        }

        public virtual DateTime? getPreferenceTime(long userID, long itemID)
        {
            if (userID == -9223372036854775808L)
            {
                if (this.tempPrefs == null)
                {
                    throw new NoSuchUserException(-9223372036854775808L);
                }
                return null;
            }
            return this._delegate.getPreferenceTime(userID, itemID);
        }

        public virtual float? getPreferenceValue(long userID, long itemID)
        {
            if (userID == -9223372036854775808L)
            {
                if (this.tempPrefs == null)
                {
                    throw new NoSuchUserException(-9223372036854775808L);
                }
                for (int i = 0; i < this.tempPrefs.length(); i++)
                {
                    if (this.tempPrefs.getItemID(i) == itemID)
                    {
                        return new float?(this.tempPrefs.getValue(i));
                    }
                }
                return null;
            }
            return this._delegate.getPreferenceValue(userID, itemID);
        }

        public virtual IEnumerator<long> getUserIDs()
        {
            if (this.tempPrefs == null)
            {
                return this._delegate.getUserIDs();
            }
            return new PlusAnonymousUserlongPrimitiveIterator(this._delegate.getUserIDs(), -9223372036854775808L);
        }

        public virtual bool hasPreferenceValues()
        {
            return this._delegate.hasPreferenceValues();
        }

        public virtual void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this._delegate.refresh(alreadyRefreshed);
        }

        public virtual void removePreference(long userID, long itemID)
        {
            if (userID == -9223372036854775808L)
            {
                if (this.tempPrefs == null)
                {
                    throw new NoSuchUserException(-9223372036854775808L);
                }
                throw new NotSupportedException();
            }
            this._delegate.removePreference(userID, itemID);
        }

        public virtual void setPreference(long userID, long itemID, float value)
        {
            if (userID == -9223372036854775808L)
            {
                if (this.tempPrefs == null)
                {
                    throw new NoSuchUserException(-9223372036854775808L);
                }
                throw new NotSupportedException();
            }
            this._delegate.setPreference(userID, itemID, value);
        }

        public void setTempPrefs(PreferenceArray prefs)
        {
            this.tempPrefs = prefs;
            this.prefItemIDs.clear();
            for (int i = 0; i < prefs.length(); i++)
            {
                this.prefItemIDs.add(prefs.getItemID(i));
            }
        }
    }
}