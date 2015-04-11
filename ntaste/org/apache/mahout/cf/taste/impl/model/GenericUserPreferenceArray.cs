namespace org.apache.mahout.cf.taste.impl.model
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public sealed class GenericUserPreferenceArray : PreferenceArray, IEnumerable<Preference>, IEnumerable
    {
        private long id;
        private long[] ids;
        private const int ITEM = 1;
        private const int VALUE = 2;
        private const int VALUE_REVERSED = 3;
        private float[] values;

        public GenericUserPreferenceArray(List<Preference> prefs)
            : this(prefs.Count)
        {
            int count = prefs.Count;
            long num2 = -9223372036854775808L;
            for (int i = 0; i < count; i++)
            {
                Preference preference = prefs[i];
                if (i == 0)
                {
                    num2 = preference.getUserID();
                }
                else if (num2 != preference.getUserID())
                {
                    throw new ArgumentException("Not all user IDs are the same");
                }
                this.ids[i] = preference.getItemID();
                this.values[i] = preference.getValue();
            }
            this.id = num2;
        }

        public GenericUserPreferenceArray(int size)
        {
            this.ids = new long[size];
            this.values = new float[size];
            this.id = -9223372036854775808L;
        }

        private GenericUserPreferenceArray(long[] ids, long id, float[] values)
        {
            this.ids = ids;
            this.id = id;
            this.values = values;
        }

        public PreferenceArray clone()
        {
            return new GenericUserPreferenceArray((long[])this.ids.Clone(), this.id, (float[])this.values.Clone());
        }

        public override bool Equals(object other)
        {
            if (!(other is GenericUserPreferenceArray))
            {
                return false;
            }
            GenericUserPreferenceArray array = (GenericUserPreferenceArray)other;
            return (((this.id == array.id) && this.ids.SequenceEqual<long>(array.ids)) && this.values.SequenceEqual<float>(array.values));
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
            return (((((int)(this.id >> 0x20)) ^ ((int)this.id)) ^ Utils.GetArrayHashCode(this.ids)) ^ Utils.GetArrayHashCode(this.values));
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
            return this.values[i];
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

        private bool isLess(int i, int j, int type)
        {
            switch (type)
            {
                case 1:
                    return (this.ids[i] < this.ids[j]);

                case 2:
                    return (this.values[i] < this.values[j]);

                case 3:
                    return (this.values[i] > this.values[j]);
            }
            throw new InvalidOperationException();
        }

        private void lateralSort(int type)
        {
            int num = this.length();
            int num2 = num;
            bool flag = false;
            while ((num2 > 1) || flag)
            {
                if (num2 > 1)
                {
                    num2 = (int)(((double)num2) / 1.2473309501039791);
                }
                flag = false;
                int num3 = num - num2;
                for (int i = 0; i < num3; i++)
                {
                    int num5 = i + num2;
                    if (this.isLess(num5, i, type))
                    {
                        this.swap(i, num5);
                        flag = true;
                    }
                }
            }
        }

        public int length()
        {
            return this.ids.Length;
        }

        public void set(int i, Preference pref)
        {
            this.id = pref.getUserID();
            this.ids[i] = pref.getItemID();
            this.values[i] = pref.getValue();
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
            this.values[i] = value;
        }

        public void sortByItem()
        {
            this.lateralSort(1);
        }

        public void sortByUser()
        {
        }

        public void sortByValue()
        {
            this.lateralSort(2);
        }

        public void sortByValueReversed()
        {
            this.lateralSort(3);
        }

        private void swap(int i, int j)
        {
            long num = this.ids[i];
            float num2 = this.values[i];
            this.ids[i] = this.ids[j];
            this.values[i] = this.values[j];
            this.ids[j] = num;
            this.values[j] = num2;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public override string ToString()
        {
            if ((this.ids == null) || (this.ids.Length == 0))
            {
                return "GenericUserPreferenceArray[{}]";
            }
            StringBuilder builder = new StringBuilder(20 * this.ids.Length);
            builder.Append("GenericUserPreferenceArray[userID:");
            builder.Append(this.id);
            builder.Append(",{");
            for (int i = 0; i < this.ids.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(',');
                }
                builder.Append(this.ids[i]);
                builder.Append('=');
                builder.Append(this.values[i]);
            }
            builder.Append("}]");
            return builder.ToString();
        }

        private sealed class PreferenceView : Preference
        {
            private GenericUserPreferenceArray arr;
            private int i;

            internal PreferenceView(GenericUserPreferenceArray arr, int i)
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
                return this.arr.values[this.i];
            }

            public void setValue(float value)
            {
                this.arr.values[this.i] = value;
            }
        }
    }
}