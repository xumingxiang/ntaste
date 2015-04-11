namespace org.apache.mahout.cf.taste.impl.common
{
    using org.apache.commons.math3.distribution;
    using org.apache.mahout.common;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public sealed class SamplingLongPrimitiveIterator : IEnumerator<long>, IDisposable, IEnumerator
    {
        private IEnumerator<long> enumerator;
        private PascalDistribution geometricDistribution;

        public SamplingLongPrimitiveIterator(IEnumerator<long> enumerator, double samplingRate)
            : this(RandomUtils.getRandom(), enumerator, samplingRate)
        {
        }

        public SamplingLongPrimitiveIterator(RandomWrapper random, IEnumerator<long> enumerator, double samplingRate)
        {
            if (enumerator == null)
            {
                throw new ArgumentException("enumerator");
            }
            if ((samplingRate <= 0.0) || (samplingRate > 1.0))
            {
                throw new ArgumentException("samplingRate");
            }
            this.geometricDistribution = new PascalDistribution(random.getRandomGenerator(), 1, samplingRate);
            this.enumerator = enumerator;
            this.SkipNext();
        }

        public void Dispose()
        {
        }

        public static IEnumerator<long> maybeWrapIterator(IEnumerator<long> enumerator, double samplingRate)
        {
            return ((samplingRate >= 1.0) ? enumerator : new SamplingLongPrimitiveIterator(enumerator, samplingRate));
        }

        public bool MoveNext()
        {
            this.SkipNext();
            return this.enumerator.MoveNext();
        }

        public void remove()
        {
            throw new NotSupportedException();
        }

        public void Reset()
        {
            this.enumerator.Reset();
        }

        public void skip(int n)
        {
            int num2;
            int num = 0;
            for (num2 = 0; num2 < n; num2++)
            {
                num += this.geometricDistribution.sample();
            }
            for (num2 = 0; num2 < num; num2++)
            {
                if (!this.enumerator.MoveNext())
                {
                    break;
                }
            }
        }

        protected void SkipNext()
        {
            int num = this.geometricDistribution.sample();
            for (int i = 0; i < num; i++)
            {
                if (!this.enumerator.MoveNext())
                {
                    break;
                }
            }
        }

        public long Current
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