using org.apache.mahout.common;
using System;
using System.Collections.Generic;
using System.Text;

namespace org.apache.mahout.cf.taste.impl.common
{
    public sealed class FastByIDMap<V>
    {
        public const int NO_MAX_SIZE = 2147483647;
        private static float DEFAULT_LOAD_FACTOR = 1.5f;
        private static long REMOVED = 9223372036854775807L;
        private static long NULL = -9223372036854775808L;
        private long[] keys;
        private V[] values;
        private float loadFactor;
        private int numEntries;
        private int numSlotsUsed;
        private int maxSize;
        private BitSet recentlyAccessed;
        private bool countingAccesses;

        public IEnumerable<long> Keys
        {
            get
            {
                int num = 0;
                while (num < this.keys.Length && num < this.values.Length)
                {
                    if (this.values[num] != null)
                    {
                        yield return this.keys[num];
                    }
                    num++;
                }
                yield break;
            }
        }

        public IEnumerable<V> Values
        {
            get
            {
                for (int i = 0; i < this.values.Length; i++)
                {
                    if (this.values[i] != null)
                    {
                        yield return this.values[i];
                    }
                }
                yield break;
            }
        }

        public FastByIDMap()
            : this(2, 2147483647)
        {
        }

        public FastByIDMap(int size)
            : this(size, 2147483647)
        {
        }

        public FastByIDMap(int size, float loadFactor)
            : this(size, 2147483647, loadFactor)
        {
        }

        public FastByIDMap(int size, int maxSize)
            : this(size, maxSize, FastByIDMap<V>.DEFAULT_LOAD_FACTOR)
        {
        }

        public FastByIDMap(int size, int maxSize, float loadFactor)
        {
            this.loadFactor = loadFactor;
            int num = (int)(2.147483E+09f / loadFactor);
            int num2 = RandomUtils.nextTwinPrime((int)(loadFactor * (float)size));
            this.keys = new long[num2];
            this.ArrayFill<long>(this.keys, FastByIDMap<V>.NULL);
            this.values = new V[num2];
            this.maxSize = maxSize;
            this.countingAccesses = (maxSize != 2147483647);
            this.recentlyAccessed = (this.countingAccesses ? new BitSet((uint)num2) : null);
        }

        private void ArrayFill<T>(T[] a, T val)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = val;
            }
        }

        private int find(long key)
        {
            int num = (int)key & 2147483647;
            long[] array = this.keys;
            int num2 = array.Length;
            int num3 = 1 + num % (num2 - 2);
            int num4 = num % num2;
            long num5 = array[num4];
            while (num5 != FastByIDMap<V>.NULL && key != num5)
            {
                num4 -= ((num4 < num3) ? (num3 - num2) : num3);
                num5 = array[num4];
            }
            return num4;
        }

        private int findForAdd(long key)
        {
            int num = (int)key & 2147483647;
            long[] array = this.keys;
            int num2 = array.Length;
            int num3 = 1 + num % (num2 - 2);
            int num4 = num % num2;
            long num5 = array[num4];
            while (num5 != FastByIDMap<V>.NULL && num5 != FastByIDMap<V>.REMOVED && key != num5)
            {
                num4 -= ((num4 < num3) ? (num3 - num2) : num3);
                num5 = array[num4];
            }
            int result;
            if (num5 != FastByIDMap<V>.REMOVED)
            {
                result = num4;
            }
            else
            {
                int num6 = num4;
                while (num5 != FastByIDMap<V>.NULL && key != num5)
                {
                    num4 -= ((num4 < num3) ? (num3 - num2) : num3);
                    num5 = array[num4];
                }
                result = ((key == num5) ? num4 : num6);
            }
            return result;
        }

        public V get(long key)
        {
            V result;
            if (key == FastByIDMap<V>.NULL)
            {
                result = default(V);
            }
            else
            {
                int num = this.find(key);
                if (this.countingAccesses)
                {
                    this.recentlyAccessed.set(num);
                }
                result = this.values[num];
            }
            return result;
        }

        public int size()
        {
            return this.numEntries;
        }

        public bool isEmpty()
        {
            return this.numEntries == 0;
        }

        public bool containsKey(long key)
        {
            return key != FastByIDMap<V>.NULL && key != FastByIDMap<V>.REMOVED && this.keys[this.find(key)] != FastByIDMap<V>.NULL;
        }

        public bool containsValue(object value)
        {
            bool result;
            if (value == null)
            {
                result = false;
            }
            else
            {
                V[] array = this.values;
                for (int i = 0; i < array.Length; i++)
                {
                    V v = array[i];
                    if (v != null && value.Equals(v))
                    {
                        result = true;
                        return result;
                    }
                }
                result = false;
            }
            return result;
        }

        public V put(long key, V value)
        {
            if ((float)this.numSlotsUsed * this.loadFactor >= (float)this.keys.Length)
            {
                if ((float)this.numEntries * this.loadFactor >= (float)this.numSlotsUsed)
                {
                    this.growAndRehash();
                }
                else
                {
                    this.rehash();
                }
            }
            int num = this.findForAdd(key);
            long num2 = this.keys[num];
            V result;
            if (num2 == key)
            {
                V v = this.values[num];
                this.values[num] = value;
                result = v;
            }
            else
            {
                if (this.countingAccesses && this.numEntries >= this.maxSize)
                {
                    this.clearStaleEntry(num);
                }
                this.keys[num] = key;
                this.values[num] = value;
                this.numEntries++;
                if (num2 == FastByIDMap<V>.NULL)
                {
                    this.numSlotsUsed++;
                }
                result = default(V);
            }
            return result;
        }

        private void clearStaleEntry(int index)
        {
            while (true)
            {
                long num;
                do
                {
                    if (index == 0)
                    {
                        index = this.keys.Length - 1;
                    }
                    else
                    {
                        index--;
                    }
                    num = this.keys[index];
                }
                while (num == FastByIDMap<V>.NULL || num == FastByIDMap<V>.REMOVED);
                if (!this.recentlyAccessed.get(index))
                {
                    break;
                }
                this.recentlyAccessed.clear(index);
            }
            this.keys[index] = FastByIDMap<V>.REMOVED;
            this.numEntries--;
            this.values[index] = default(V);
        }

        public V remove(long key)
        {
            V result;
            if (key == FastByIDMap<V>.NULL || key == FastByIDMap<V>.REMOVED)
            {
                result = default(V);
            }
            else
            {
                int num = this.find(key);
                if (this.keys[num] == FastByIDMap<V>.NULL)
                {
                    result = default(V);
                }
                else
                {
                    this.keys[num] = FastByIDMap<V>.REMOVED;
                    this.numEntries--;
                    V v = this.values[num];
                    this.values[num] = default(V);
                    result = v;
                }
            }
            return result;
        }

        public void clear()
        {
            this.numEntries = 0;
            this.numSlotsUsed = 0;
            this.ArrayFill<long>(this.keys, FastByIDMap<V>.NULL);
            this.ArrayFill<V>(this.values, default(V));
            if (this.countingAccesses)
            {
                this.recentlyAccessed.clear();
            }
        }

        public IEnumerable<KeyValuePair<long, V>> entrySet()
        {
            int num = 0;
            while (num < this.keys.Length && num < this.values.Length)
            {
                if (this.values[num] != null)
                {
                    yield return new KeyValuePair<long, V>(this.keys[num], this.values[num]);
                }
                num++;
            }
            yield break;
        }

        public void rehash()
        {
            this.rehash(RandomUtils.nextTwinPrime((int)(this.loadFactor * (float)this.numEntries)));
        }

        private void growAndRehash()
        {
            if ((float)this.keys.Length * this.loadFactor >= 2.147483E+09f)
            {
                throw new InvalidOperationException("Can't grow any more");
            }
            this.rehash(RandomUtils.nextTwinPrime((int)(this.loadFactor * (float)this.keys.Length)));
        }

        private void rehash(int newHashSize)
        {
            long[] array = this.keys;
            V[] array2 = this.values;
            this.numEntries = 0;
            this.numSlotsUsed = 0;
            if (this.countingAccesses)
            {
                this.recentlyAccessed = new BitSet(newHashSize);
            }
            this.keys = new long[newHashSize];
            this.ArrayFill<long>(this.keys, FastByIDMap<V>.NULL);
            this.values = new V[newHashSize];
            int num = array.Length;
            for (int i = 0; i < num; i++)
            {
                long num2 = array[i];
                if (num2 != FastByIDMap<V>.NULL && num2 != FastByIDMap<V>.REMOVED)
                {
                    this.put(num2, array2[i]);
                }
            }
        }

        private void iteratorRemove(int lastNext)
        {
            if (lastNext >= this.values.Length)
            {
                throw new InvalidOperationException();
            }
            if (lastNext < 0)
            {
                throw new InvalidOperationException();
            }
            this.values[lastNext] = default(V);
            this.keys[lastNext] = FastByIDMap<V>.REMOVED;
            this.numEntries--;
        }

        public override string ToString()
        {
            string result;
            if (this.isEmpty())
            {
                result = "{}";
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append('{');
                for (int i = 0; i < this.keys.Length; i++)
                {
                    long num = this.keys[i];
                    if (num != FastByIDMap<V>.NULL && num != FastByIDMap<V>.REMOVED)
                    {
                        stringBuilder.Append(num).Append('=').Append(this.values[i].ToString()).Append(',');
                    }
                }
                stringBuilder[stringBuilder.Length - 1] = '}';
                result = stringBuilder.ToString();
            }
            return result;
        }

        public override int GetHashCode()
        {
            int num = 0;
            long[] array = this.keys;
            int num2 = array.Length;
            for (int i = 0; i < num2; i++)
            {
                long num3 = array[i];
                if (num3 != FastByIDMap<V>.NULL && num3 != FastByIDMap<V>.REMOVED)
                {
                    num = 31 * num + ((int)(num3 >> 32) ^ (int)num3);
                    num = 31 * num + this.values[i].GetHashCode();
                }
            }
            return num;
        }

        public override bool Equals(object other)
        {
            bool result;
            if (!(other is FastByIDMap<V>))
            {
                result = false;
            }
            else
            {
                FastByIDMap<V> fastByIDMap = (FastByIDMap<V>)other;
                long[] array = fastByIDMap.keys;
                V[] array2 = fastByIDMap.values;
                int num = this.keys.Length;
                int num2 = array.Length;
                int num3 = Math.Min(num, num2);
                int i;
                for (i = 0; i < num3; i++)
                {
                    long num4 = this.keys[i];
                    long num5 = array[i];
                    if (num4 == FastByIDMap<V>.NULL || num4 == FastByIDMap<V>.REMOVED)
                    {
                        if (num5 != FastByIDMap<V>.NULL && num5 != FastByIDMap<V>.REMOVED)
                        {
                            result = false;
                            return result;
                        }
                    }
                    else
                    {
                        if (num4 != num5 || !this.values[i].Equals(array2[i]))
                        {
                            result = false;
                            return result;
                        }
                    }
                }
                while (i < num)
                {
                    long num4 = this.keys[i];
                    if (num4 != FastByIDMap<V>.NULL && num4 != FastByIDMap<V>.REMOVED)
                    {
                        result = false;
                        return result;
                    }
                    i++;
                }
                while (i < num2)
                {
                    long num4 = array[i];
                    if (num4 != FastByIDMap<V>.NULL && num4 != FastByIDMap<V>.REMOVED)
                    {
                        result = false;
                        return result;
                    }
                    i++;
                }
                result = true;
            }
            return result;
        }
    }
}