namespace org.apache.mahout.cf.taste.impl.common
{
    using org.apache.mahout.common;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class FixedSizeSamplingIterator<T> : IEnumerator<T>, IDisposable, IEnumerator
    {
        private List<T> buf;
        private IEnumerator<T> enumerator;

        public FixedSizeSamplingIterator(int size, IEnumerator<T> source)
        {
            this.buf = new List<T>(size);
            int n = 0;
            RandomWrapper wrapper = RandomUtils.getRandom();
            while (source.MoveNext())
            {
                T current = source.Current;
                n++;
                if (this.buf.Count < size)
                {
                    this.buf.Add(current);
                }
                else
                {
                    int num2 = wrapper.nextInt(n);
                    if (num2 < this.buf.Count)
                    {
                        this.buf[num2] = current;
                    }
                }
            }
            this.enumerator = this.buf.GetEnumerator();
        }

        public void Dispose()
        {
            this.enumerator.Dispose();
        }

        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        public void Reset()
        {
            this.enumerator.Reset();
        }

        public T Current
        {
            get
            {
                return this.enumerator.Current;
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
}