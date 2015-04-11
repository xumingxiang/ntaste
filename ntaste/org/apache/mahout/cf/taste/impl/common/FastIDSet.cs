namespace org.apache.mahout.cf.taste.impl.common
{
    using org.apache.mahout.common;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text;

    public sealed class FastIDSet : ICloneable, IEnumerable<long>, IEnumerable
    {
        private static float DEFAULT_LOAD_FACTOR = 1.5f;
        private long[] keys;
        private float loadFactor;
        private static long NULL = -9223372036854775808L;
        private int numEntries;
        private int numSlotsUsed;
        private static long REMOVED = 0x7fffffffffffffffL;

        public FastIDSet()
            : this(2)
        {
        }

        public FastIDSet(long[] initialKeys)
            : this(initialKeys.Length)
        {
            this.addAll(initialKeys);
        }

        private FastIDSet(FastIDSet copyFrom)
        {
            this.keys = new long[copyFrom.keys.Length];
            for (int i = 0; i < copyFrom.keys.Length; i++)
            {
                this.keys[i] = copyFrom.keys[i];
            }
            this.loadFactor = copyFrom.loadFactor;
            this.numEntries = copyFrom.numEntries;
            this.numSlotsUsed = copyFrom.numSlotsUsed;
        }

        public FastIDSet(int size)
            : this(size, DEFAULT_LOAD_FACTOR)
        {
        }

        public FastIDSet(int size, float loadFactor)
        {
            this.loadFactor = loadFactor;
            int num = (int)(2.147483E+09f / loadFactor);
            int num2 = RandomUtils.nextTwinPrime((int)(loadFactor * size));
            this.keys = new long[num2];
            this.ArrayFill<long>(this.keys, NULL);
        }

        public bool add(long key)
        {
            if ((key == NULL) || (key == REMOVED))
            {
                throw new ArgumentException();
            }
            if ((this.numSlotsUsed * this.loadFactor) >= this.keys.Length)
            {
                if ((this.numEntries * this.loadFactor) >= this.numSlotsUsed)
                {
                    this.growAndRehash();
                }
                else
                {
                    this.rehash();
                }
            }
            int index = this.findForAdd(key);
            long num2 = this.keys[index];
            if (num2 != key)
            {
                this.keys[index] = key;
                this.numEntries++;
                if (num2 == NULL)
                {
                    this.numSlotsUsed++;
                }
                return true;
            }
            return false;
        }

        public bool addAll(long[] c)
        {
            bool flag = false;
            foreach (long num in c)
            {
                if (this.add(num))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public bool addAll(FastIDSet c)
        {
            bool flag = false;
            foreach (long num in c.keys)
            {
                if (((num != NULL) && (num != REMOVED)) && this.add(num))
                {
                    flag = true;
                }
            }
            return flag;
        }

        private void ArrayFill<T>(T[] a, T val)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = val;
            }
        }

        public void clear()
        {
            this.numEntries = 0;
            this.numSlotsUsed = 0;
            this.ArrayFill<long>(this.keys, NULL);
        }

        public object Clone()
        {
            return new FastIDSet(this);
        }

        public bool contains(long key)
        {
            return (((key != NULL) && (key != REMOVED)) && (this.keys[this.find(key)] != NULL));
        }

        public override bool Equals(object other)
        {
            long num5;
            if (!(other is FastIDSet))
            {
                return false;
            }
            FastIDSet set = (FastIDSet)other;
            long[] keys = set.keys;
            int length = this.keys.Length;
            int num2 = keys.Length;
            int num3 = Math.Min(length, num2);
            int index = 0;
            while (index < num3)
            {
                num5 = this.keys[index];
                long num6 = keys[index];
                if ((num5 == NULL) || (num5 == REMOVED))
                {
                    if ((num6 != NULL) && (num6 != REMOVED))
                    {
                        return false;
                    }
                }
                else if (num5 != num6)
                {
                    return false;
                }
                index++;
            }
            while (index < length)
            {
                num5 = this.keys[index];
                if ((num5 != NULL) && (num5 != REMOVED))
                {
                    return false;
                }
                index++;
            }
            while (index < num2)
            {
                num5 = keys[index];
                if ((num5 != NULL) && (num5 != REMOVED))
                {
                    return false;
                }
                index++;
            }
            return true;
        }

        private int find(long key)
        {
            int num = ((int)key) & 0x7fffffff;
            long[] keys = this.keys;
            int length = keys.Length;
            int num3 = 1 + (num % (length - 2));
            int index = num % length;
            for (long i = keys[index]; (i != NULL) && (key != i); i = keys[index])
            {
                index -= (index < num3) ? (num3 - length) : num3;
            }
            return index;
        }

        private int findForAdd(long key)
        {
            int num = ((int)key) & 0x7fffffff;
            long[] keys = this.keys;
            int length = keys.Length;
            int num3 = 1 + (num % (length - 2));
            int index = num % length;
            long num5 = keys[index];
            while (((num5 != NULL) && (num5 != REMOVED)) && (key != num5))
            {
                index -= (index < num3) ? (num3 - length) : num3;
                num5 = keys[index];
            }
            if (num5 != REMOVED)
            {
                return index;
            }
            int num6 = index;
            while ((num5 != NULL) && (key != num5))
            {
                index -= (index < num3) ? (num3 - length) : num3;
                num5 = keys[index];
            }
            return ((key == num5) ? index : num6);
        }

        public IEnumerator<long> GetEnumerator()
        {
            for (int i = 0; i < this.keys.Length; i++)
            {
                if ((this.keys[i] != NULL) && (this.keys[i] != REMOVED))
                {
                    yield return this.keys[i];
                }
            }
        }

        public override int GetHashCode()
        {
            int num = 0;
            long[] keys = this.keys;
            foreach (long num2 in keys)
            {
                if ((num2 != NULL) && (num2 != REMOVED))
                {
                    num = (0x1f * num) + (((int)(num2 >> 0x20)) ^ ((int)num2));
                }
            }
            return num;
        }

        private void growAndRehash()
        {
            if ((this.keys.Length * this.loadFactor) >= 2.147483E+09f)
            {
                throw new InvalidOperationException("Can't grow any more");
            }
            this.rehash(RandomUtils.nextTwinPrime((int)(this.loadFactor * this.keys.Length)));
        }

        public int intersectionSize(FastIDSet other)
        {
            int num = 0;
            foreach (long num2 in other.keys)
            {
                if (((num2 != NULL) && (num2 != REMOVED)) && (this.keys[this.find(num2)] != NULL))
                {
                    num++;
                }
            }
            return num;
        }

        public bool isEmpty()
        {
            return (this.numEntries == 0);
        }

        public void rehash()
        {
            this.rehash(RandomUtils.nextTwinPrime((int)(this.loadFactor * this.numEntries)));
        }

        private void rehash(int newHashSize)
        {
            long[] keys = this.keys;
            this.numEntries = 0;
            this.numSlotsUsed = 0;
            this.keys = new long[newHashSize];
            this.ArrayFill<long>(this.keys, NULL);
            foreach (long num in keys)
            {
                if ((num != NULL) && (num != REMOVED))
                {
                    this.add(num);
                }
            }
        }

        public bool remove(long key)
        {
            if ((key == NULL) || (key == REMOVED))
            {
                return false;
            }
            int index = this.find(key);
            if (this.keys[index] == NULL)
            {
                return false;
            }
            this.keys[index] = REMOVED;
            this.numEntries--;
            return true;
        }

        public bool removeAll(FastIDSet c)
        {
            bool flag = false;
            foreach (long num in c.keys)
            {
                if (((num != NULL) && (num != REMOVED)) && this.remove(num))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public bool removeAll(long[] c)
        {
            bool flag = false;
            foreach (long num in c)
            {
                if (this.remove(num))
                {
                    flag = true;
                }
            }
            return flag;
        }

        public bool retainAll(FastIDSet c)
        {
            bool flag = false;
            for (int i = 0; i < this.keys.Length; i++)
            {
                long key = this.keys[i];
                if (!(((key == NULL) || (key == REMOVED)) || c.contains(key)))
                {
                    this.keys[i] = REMOVED;
                    this.numEntries--;
                    flag = true;
                }
            }
            return flag;
        }

        public int size()
        {
            return this.numEntries;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public long[] toArray()
        {
            long[] numArray = new long[this.numEntries];
            int index = 0;
            int num2 = 0;
            while (index < numArray.Length)
            {
                while ((this.keys[num2] == NULL) || (this.keys[num2] == REMOVED))
                {
                    num2++;
                }
                numArray[index] = this.keys[num2++];
                index++;
            }
            return numArray;
        }

        public override string ToString()
        {
            if (this.isEmpty())
            {
                return "[]";
            }
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            foreach (long num in this.keys)
            {
                if ((num != NULL) && (num != REMOVED))
                {
                    builder.Append(num).Append(',');
                }
            }
            builder[builder.Length - 1] = ']';
            return builder.ToString();
        }
    }
}