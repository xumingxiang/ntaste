namespace org.apache.mahout.cf.taste.impl.common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class Cache<K, V> : Retriever<K, V>
    {
        private IDictionary<K, V> cache;
        private static object NULL;
        private Retriever<K, V> retriever;

        static Cache()
        {
            Cache<K, V>.NULL = new object();
        }

        public Cache(Retriever<K, V> retriever)
            : this(retriever, 0x7fffffff)
        {
        }

        public Cache(Retriever<K, V> retriever, int maxEntries)
        {
            this.cache = new Dictionary<K, V>(11);
            this.retriever = retriever;
        }

        public void clear()
        {
            lock (this.cache)
            {
                this.cache.Clear();
            }
        }

        public V get(K key)
        {
            V local;
            bool flag;
            lock (this.cache)
            {
                flag = this.cache.TryGetValue(key, out local);
            }
            if (!flag)
            {
                return this.getAndCacheValue(key);
            }
            return (Cache<K, V>.NULL.Equals(local) ? default(V) : local);
        }

        private V getAndCacheValue(K key)
        {
            V nULL = this.retriever.get(key);
            if (nULL == null)
            {
                nULL = (V)Cache<K, V>.NULL;
            }
            lock (this.cache)
            {
                this.cache[key] = nULL;
            }
            return nULL;
        }

        public void remove(K key)
        {
            lock (this.cache)
            {
                this.cache.Remove(key);
            }
        }

        public void removeKeysMatching(Func<K, bool> predicate)
        {
            lock (this.cache)
            {
                K[] localArray = this.cache.Keys.ToArray<K>();
                foreach (K local in localArray)
                {
                    if (predicate(local))
                    {
                        this.cache.Remove(local);
                    }
                }
            }
        }

        public void removeValueMatching(Func<V, bool> predicate)
        {
            lock (this.cache)
            {
                KeyValuePair<K, V>[] pairArray = this.cache.ToArray<KeyValuePair<K, V>>();
                foreach (KeyValuePair<K, V> pair in pairArray)
                {
                    if (predicate(pair.Value))
                    {
                        this.cache.Remove(pair);
                    }
                }
            }
        }

        public override string ToString()
        {
            return ("Cache[retriever:" + this.retriever.ToString() + ']');
        }
    }
}