namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.recommender;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using org.apache.mahout.common;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class GenericItemSimilarity : ItemSimilarity, Refreshable
    {
        private static long[] NO_IDS = new long[0];
        private FastByIDMap<FastIDSet> similarItemIDsIndex;
        private FastByIDMap<FastByIDMap<double?>> similarityMaps;

        public GenericItemSimilarity(IEnumerable<ItemItemSimilarity> similarities)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double?>>();
            this.similarItemIDsIndex = new FastByIDMap<FastIDSet>();
            this.initSimilarityMaps(similarities.GetEnumerator());
        }

        public GenericItemSimilarity(IEnumerable<ItemItemSimilarity> similarities, int maxToKeep)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double?>>();
            this.similarItemIDsIndex = new FastByIDMap<FastIDSet>();
            this.initSimilarityMaps(TopItems.getTopItemItemSimilarities(maxToKeep, similarities.GetEnumerator()).GetEnumerator());
        }

        public GenericItemSimilarity(ItemSimilarity otherSimilarity, DataModel dataModel)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double?>>();
            this.similarItemIDsIndex = new FastByIDMap<FastIDSet>();
            long[] itemIDs = GenericUserSimilarity.longIteratorToList(dataModel.getItemIDs());
            this.initSimilarityMaps(new DataModelSimilaritiesIterator(otherSimilarity, itemIDs));
        }

        public GenericItemSimilarity(ItemSimilarity otherSimilarity, DataModel dataModel, int maxToKeep)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double?>>();
            this.similarItemIDsIndex = new FastByIDMap<FastIDSet>();
            long[] itemIDs = GenericUserSimilarity.longIteratorToList(dataModel.getItemIDs());
            DataModelSimilaritiesIterator allSimilarities = new DataModelSimilaritiesIterator(otherSimilarity, itemIDs);
            this.initSimilarityMaps(TopItems.getTopItemItemSimilarities(maxToKeep, allSimilarities).GetEnumerator());
        }

        public long[] allSimilarItemIDs(long itemID)
        {
            FastIDSet set = this.similarItemIDsIndex.get(itemID);
            return ((set != null) ? set.toArray() : NO_IDS);
        }

        private void doIndex(long fromItemID, long toItemID)
        {
            FastIDSet set = this.similarItemIDsIndex.get(fromItemID);
            if (set == null)
            {
                set = new FastIDSet();
                this.similarItemIDsIndex.put(fromItemID, set);
            }
            set.add(toItemID);
        }

        private void initSimilarityMaps(IEnumerator<ItemItemSimilarity> similarities)
        {
            while (similarities.MoveNext())
            {
                ItemItemSimilarity current = similarities.Current;
                long num = current.getItemID1();
                long num2 = current.getItemID2();
                if (num != num2)
                {
                    long num3;
                    long num4;
                    if (num < num2)
                    {
                        num3 = num;
                        num4 = num2;
                    }
                    else
                    {
                        num3 = num2;
                        num4 = num;
                    }
                    FastByIDMap<double?> map = this.similarityMaps.get(num3);
                    if (map == null)
                    {
                        map = new FastByIDMap<double?>();
                        this.similarityMaps.put(num3, map);
                    }
                    map.put(num4, new double?(current.getValue()));
                    this.doIndex(num3, num4);
                    this.doIndex(num4, num3);
                }
            }
        }

        public double[] itemSimilarities(long itemID1, long[] itemID2s)
        {
            int length = itemID2s.Length;
            double[] numArray = new double[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = this.itemSimilarity(itemID1, itemID2s[i]);
            }
            return numArray;
        }

        public double itemSimilarity(long itemID1, long itemID2)
        {
            long num;
            long num2;
            if (itemID1 == itemID2)
            {
                return 1.0;
            }
            if (itemID1 < itemID2)
            {
                num = itemID1;
                num2 = itemID2;
            }
            else
            {
                num = itemID2;
                num2 = itemID1;
            }
            FastByIDMap<double?> map = this.similarityMaps.get(num);
            if (map == null)
            {
                return double.NaN;
            }
            double? nullable = map.get(num2);
            return (!nullable.HasValue ? double.NaN : nullable.Value);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
        }

        private sealed class DataModelSimilaritiesIterator : IEnumerator<GenericItemSimilarity.ItemItemSimilarity>, IDisposable, IEnumerator
        {
            private GenericItemSimilarity.ItemItemSimilarity _Current;
            private int i;
            private long itemID1;
            private long[] itemIDs;
            private int j;
            private ItemSimilarity otherSimilarity;

            internal DataModelSimilaritiesIterator(ItemSimilarity otherSimilarity, long[] itemIDs)
            {
                this.otherSimilarity = otherSimilarity;
                this.itemIDs = itemIDs;
                this.i = 0;
                this.itemID1 = itemIDs[0];
                this.j = 1;
            }

            protected GenericItemSimilarity.ItemItemSimilarity computeNext()
            {
                int length = this.itemIDs.Length;
                GenericItemSimilarity.ItemItemSimilarity similarity = null;
                while ((similarity == null) && (this.i < (length - 1)))
                {
                    double num3;
                    long num2 = this.itemIDs[this.j];
                    try
                    {
                        num3 = this.otherSimilarity.itemSimilarity(this.itemID1, num2);
                    }
                    catch (TasteException exception)
                    {
                        throw new InvalidOperationException(exception.Message, exception);
                    }
                    if (!double.IsNaN(num3))
                    {
                        similarity = new GenericItemSimilarity.ItemItemSimilarity(this.itemID1, num2, num3);
                    }
                    if (++this.j == length)
                    {
                        this.itemID1 = this.itemIDs[++this.i];
                        this.j = this.i + 1;
                    }
                }
                return similarity;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                this._Current = this.computeNext();
                return (this._Current != null);
            }

            public void Reset()
            {
                this._Current = null;
                this.i = 0;
                this.j = 1;
            }

            public GenericItemSimilarity.ItemItemSimilarity Current
            {
                get
                {
                    if (this._Current == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return this._Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }

        public class ItemItemSimilarity : IComparable<GenericItemSimilarity.ItemItemSimilarity>
        {
            private long itemID1;
            private long itemID2;
            private double value;

            public ItemItemSimilarity(long itemID1, long itemID2, double value)
            {
                this.itemID1 = itemID1;
                this.itemID2 = itemID2;
                this.value = value;
            }

            public int CompareTo(GenericItemSimilarity.ItemItemSimilarity other)
            {
                double num = other.getValue();
                return ((this.value > num) ? -1 : ((this.value < num) ? 1 : 0));
            }

            public override bool Equals(object other)
            {
                if (!(other is GenericItemSimilarity.ItemItemSimilarity))
                {
                    return false;
                }
                GenericItemSimilarity.ItemItemSimilarity similarity = (GenericItemSimilarity.ItemItemSimilarity)other;
                return (((similarity.getItemID1() == this.itemID1) && (similarity.getItemID2() == this.itemID2)) && (similarity.getValue() == this.value));
            }

            public override int GetHashCode()
            {
                return ((((int)this.itemID1) ^ ((int)this.itemID2)) ^ RandomUtils.hashDouble(this.value));
            }

            public long getItemID1()
            {
                return this.itemID1;
            }

            public long getItemID2()
            {
                return this.itemID2;
            }

            public double getValue()
            {
                return this.value;
            }

            public override string ToString()
            {
                return string.Concat(new object[] { "ItemItemSimilarity[", this.itemID1, ',', this.itemID2, ':', this.value, ']' });
            }
        }
    }
}