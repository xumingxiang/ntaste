namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class BooleanUserPreferenceArray : PreferenceArray, IEnumerable<Preference>, IEnumerable
    {
        private long id;
        private long[] ids;

        public BooleanUserPreferenceArray(List<Preference> prefs)
            : this(prefs.Count)
        {
            int count = prefs.Count;
            for (int i = 0; i < count; i++)
            {
                this.ids[i] = prefs[i].getItemID();
            }
            if (count > 0)
            {
                this.id = prefs[0].getUserID();
            }
        }

        public BooleanUserPreferenceArray(int size)
        {
            this.ids = new long[size];
            this.id = -9223372036854775808L;
        }

        private BooleanUserPreferenceArray(long[] ids, long id)
        {
            this.ids = ids;
            this.id = id;
        }

        public PreferenceArray clone()
        {
            return new BooleanUserPreferenceArray((long[])this.ids.Clone(), this.id);
        }

        public override bool Equals(object other)
        {
            if (!(other is BooleanUserPreferenceArray))
            {
                return false;
            }
            BooleanUserPreferenceArray array = (BooleanUserPreferenceArray)other;
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
            return this.ids[i];
        }

        public long getUserID(int i)
        {
            return this.id;
        }

        public float getValue(int i)
        {
            return 1f;
        }

        public bool hasPrefWithItemID(long itemID)
        {
            foreach (long num in this.ids)
            {
                if (itemID == num)
                {
                    return true;
                }
            }
            return false;
        }

        public bool hasPrefWithUserID(long userID)
        {
            return (this.id == userID);
        }

        public int length()
        {
            return this.ids.Length;
        }

        public void set(int i, Preference pref)
        {
            this.id = pref.getUserID();
            this.ids[i] = pref.getItemID();
        }

        public void setItemID(int i, long itemID)
        {
            this.ids[i] = itemID;
        }

        public void setUserID(int i, long userID)
        {
            this.id = userID;
        }

        public void setValue(int i, float value)
        {
            throw new NotSupportedException();
        }

        public void sortByItem()
        {
            Array.Sort<long>(this.ids);
        }

        public void sortByUser()
        {
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
            builder.Append("BooleanUserPreferenceArray[userID:");
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
            private BooleanUserPreferenceArray arr;
            private int i;

            internal PreferenceView(BooleanUserPreferenceArray arr, int i)
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