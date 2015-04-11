namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class BooleanItemPreferenceArray : PreferenceArray, IEnumerable<Preference>, IEnumerable
    {
        private long id;
        private long[] ids;

        public BooleanItemPreferenceArray(int size)
        {
            this.ids = new long[size];
            this.id = -9223372036854775808L;
        }

        public BooleanItemPreferenceArray(List<Preference> prefs, bool forOneUser)
            : this(prefs.Count)
        {
            int count = prefs.Count;
            for (int i = 0; i < count; i++)
            {
                Preference preference = prefs[i];
                this.ids[i] = forOneUser ? preference.getItemID() : preference.getUserID();
            }
            if (count > 0)
            {
                this.id = forOneUser ? prefs[0].getUserID() : prefs[0].getItemID();
            }
        }

        private BooleanItemPreferenceArray(long[] ids, long id)
        {
            this.ids = ids;
            this.id = id;
        }

        public PreferenceArray clone()
        {
            return new BooleanItemPreferenceArray((long[])this.ids.Clone(), this.id);
        }

        public override bool Equals(object other)
        {
            if (!(other is BooleanItemPreferenceArray))
            {
                return false;
            }
            BooleanItemPreferenceArray array = (BooleanItemPreferenceArray)other;
            return ((this.id == array.id) && this.ids.SequenceEqual<long>(array.ids));
        }

        public Preference get(int i)
        {
            return new PreferenceView(this, i);
        }

        public IEnumerator<Preference> GetEnumerator()
        {
            for (int i = 0; i < this.length(); i++)
            {
                yield return new PreferenceView(this, i);
            }
        }

        public override int GetHashCode()
        {
            return ((((int)(this.id >> 0x20)) ^ ((int)this.id)) ^ Utils.GetArrayHashCode(this.ids));
        }

        public long[] getIDs()
        {
            return this.ids;
        }

        public long getItemID(int i)
        {
            return this.id;
        }

        public long getUserID(int i)
        {
            return this.ids[i];
        }

        public float getValue(int i)
        {
            return 1f;
        }

        public bool hasPrefWithItemID(long itemID)
        {
            return (this.id == itemID);
        }

        public bool hasPrefWithUserID(long userID)
        {
            foreach (long num in this.ids)
            {
                if (userID == num)
                {
                    return true;
                }
            }
            return false;
        }

        public int length()
        {
            return this.ids.Length;
        }

        public void set(int i, Preference pref)
        {
            this.id = pref.getItemID();
            this.ids[i] = pref.getUserID();
        }

        public void setItemID(int i, long itemID)
        {
            this.id = itemID;
        }

        public void setUserID(int i, long userID)
        {
            this.ids[i] = userID;
        }

        public void setValue(int i, float value)
        {
            throw new NotSupportedException();
        }

        public void sortByItem()
        {
        }

        public void sortByUser()
        {
            Array.Sort<long>(this.ids);
        }

        public void sortByValue()
        {
        }

        public void sortByValueReversed()
        {
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(10 * this.ids.Length);
            builder.Append("BooleanItemPreferenceArray[itemID:");
            builder.Append(this.id);
            builder.Append(",{");
            for (int i = 0; i < this.ids.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(this.ids[i]);
            }
            builder.Append("}]");
            return builder.ToString();
        }

        private sealed class PreferenceView : Preference
        {
            private BooleanItemPreferenceArray arr;
            private int i;

            internal PreferenceView(BooleanItemPreferenceArray arr, int i)
            {
                this.i = i;
                this.arr = arr;
            }

            public long getItemID()
            {
                return this.arr.getItemID(this.i);
            }

            public long getUserID()
            {
                return this.arr.getUserID(this.i);
            }

            public float getValue()
            {
                return 1f;
            }

            public void setValue(float value)
            {
                throw new NotSupportedException();
            }
        }
    }
}