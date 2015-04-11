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

    public sealed class GenericUserSimilarity : UserSimilarity, Refreshable
    {
        private FastByIDMap<FastByIDMap<double>> similarityMaps;

        public GenericUserSimilarity(IEnumerable<UserUserSimilarity> similarities)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double>>();
            this.initSimilarityMaps(similarities.GetEnumerator());
        }

        public GenericUserSimilarity(IEnumerable<UserUserSimilarity> similarities, int maxToKeep)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double>>();
            this.initSimilarityMaps(TopItems.getTopUserUserSimilarities(maxToKeep, similarities.GetEnumerator()).GetEnumerator());
        }

        public GenericUserSimilarity(UserSimilarity otherSimilarity, DataModel dataModel)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double>>();
            long[] itemIDs = longIteratorToList(dataModel.getUserIDs());
            this.initSimilarityMaps(new DataModelSimilaritiesIterator(otherSimilarity, itemIDs));
        }

        public GenericUserSimilarity(UserSimilarity otherSimilarity, DataModel dataModel, int maxToKeep)
        {
            this.similarityMaps = new FastByIDMap<FastByIDMap<double>>();
            long[] itemIDs = longIteratorToList(dataModel.getUserIDs());
            IEnumerator<UserUserSimilarity> allSimilarities = new DataModelSimilaritiesIterator(otherSimilarity, itemIDs);
            this.initSimilarityMaps(TopItems.getTopUserUserSimilarities(maxToKeep, allSimilarities).GetEnumerator());
        }

        private void initSimilarityMaps(IEnumerator<UserUserSimilarity> similarities)
        {
            while (similarities.MoveNext())
            {
                UserUserSimilarity current = similarities.Current;
                long num = current.getUserID1();
                long num2 = current.getUserID2();
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
                    FastByIDMap<double> map = this.similarityMaps.get(num3);
                    if (map == null)
                    {
                        map = new FastByIDMap<double>();
                        this.similarityMaps.put(num3, map);
                    }
                    map.put(num4, current.getValue());
                }
            }
        }

        public static long[] longIteratorToList(IEnumerator<long> iterator)
        {
            long[] numArray2;
            long[] sourceArray = new long[5];
            int length = 0;
            while (iterator.MoveNext())
            {
                if (length == sourceArray.Length)
                {
                    numArray2 = new long[sourceArray.Length << 1];
                    Array.Copy(sourceArray, 0, numArray2, 0, sourceArray.Length);
                    sourceArray = numArray2;
                }
                sourceArray[length++] = iterator.Current;
            }
            if (length != sourceArray.Length)
            {
                numArray2 = new long[length];
                Array.Copy(sourceArray, 0, numArray2, 0, length);
                sourceArray = numArray2;
            }
            return sourceArray;
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
        }

        public void setPreferenceInferrer(PreferenceInferrer inferrer)
        {
            throw new NotSupportedException();
        }

        public double userSimilarity(long userID1, long userID2)
        {
            long num;
            long num2;
            if (userID1 == userID2)
            {
                return 1.0;
            }
            if (userID1 < userID2)
            {
                num = userID1;
                num2 = userID2;
            }
            else
            {
                num = userID2;
                num2 = userID1;
            }
            FastByIDMap<double> map = this.similarityMaps.get(num);
            if (map == null)
            {
                return double.NaN;
            }
            return map.get(num2);
        }

        private sealed class DataModelSimilaritiesIterator : IEnumerator<GenericUserSimilarity.UserUserSimilarity>, IDisposable, IEnumerator
        {
            private GenericUserSimilarity.UserUserSimilarity _Current;
            private int i;
            private long itemID1;
            private long[] itemIDs;
            private int j;
            private UserSimilarity otherSimilarity;

            internal DataModelSimilaritiesIterator(UserSimilarity otherSimilarity, long[] itemIDs)
            {
                this.otherSimilarity = otherSimilarity;
                this.itemIDs = itemIDs;
                this.i = 0;
                this.itemID1 = itemIDs[0];
                this.j = 1;
            }

            protected GenericUserSimilarity.UserUserSimilarity computeNext()
            {
                int length = this.itemIDs.Length;
                while (this.i < (length - 1))
                {
                    double num3;
                    long num2 = this.itemIDs[this.j];
                    try
                    {
                        num3 = this.otherSimilarity.userSimilarity(this.itemID1, num2);
                    }
                    catch (TasteException exception)
                    {
                        throw new InvalidOperationException(exception.Message, exception);
                    }
                    if (!double.IsNaN(num3))
                    {
                        return new GenericUserSimilarity.UserUserSimilarity(this.itemID1, num2, num3);
                    }
                    if (++this.j == length)
                    {
                        this.itemID1 = this.itemIDs[++this.i];
                        this.j = this.i + 1;
                    }
                }
                return null;
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

            public GenericUserSimilarity.UserUserSimilarity Current
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

        public class UserUserSimilarity : IComparable<GenericUserSimilarity.UserUserSimilarity>
        {
            private long userID1;
            private long userID2;
            private double value;

            public UserUserSimilarity(long userID1, long userID2, double value)
            {
                this.userID1 = userID1;
                this.userID2 = userID2;
                this.value = value;
            }

            public int CompareTo(GenericUserSimilarity.UserUserSimilarity other)
            {
                double num = other.getValue();
                return ((this.value > num) ? -1 : ((this.value < num) ? 1 : 0));
            }

            public override bool Equals(object other)
            {
                if (!(other is GenericUserSimilarity.UserUserSimilarity))
                {
                    return false;
                }
                GenericUserSimilarity.UserUserSimilarity similarity = (GenericUserSimilarity.UserUserSimilarity)other;
                return (((similarity.getUserID1() == this.userID1) && (similarity.getUserID2() == this.userID2)) && (similarity.getValue() == this.value));
            }

            public override int GetHashCode()
            {
                return ((((int)this.userID1) ^ ((int)this.userID2)) ^ RandomUtils.hashDouble(this.value));
            }

            public long getUserID1()
            {
                return this.userID1;
            }

            public long getUserID2()
            {
                return this.userID2;
            }

            public double getValue()
            {
                return this.value;
            }

            public override string ToString()
            {
                return string.Concat(new object[] { "UserUserSimilarity[", this.userID1, ',', this.userID2, ':', this.value, ']' });
            }
        }
    }
}