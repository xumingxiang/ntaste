namespace org.apache.mahout.cf.taste.impl.model
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class PlusAnonymousUserlongPrimitiveIterator : IEnumerator<long>, IDisposable, IEnumerator
    {
        private bool currentDatum = false;
        private bool datumConsumed;
        private IEnumerator<long> enumerator;
        private long extraDatum;
        private bool prevMoveNext = false;

        public PlusAnonymousUserlongPrimitiveIterator(IEnumerator<long> enumerator, long extraDatum)
        {
            this.enumerator = enumerator;
            this.extraDatum = extraDatum;
            this.datumConsumed = false;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (this.currentDatum)
            {
                this.currentDatum = false;
                return this.prevMoveNext;
            }
            this.prevMoveNext = this.enumerator.MoveNext();
            if ((this.prevMoveNext && !this.datumConsumed) && (this.extraDatum <= this.Current))
            {
                this.datumConsumed = true;
                this.currentDatum = true;
                return true;
            }
            if (!(this.prevMoveNext || this.datumConsumed))
            {
                this.datumConsumed = true;
                this.currentDatum = true;
                return true;
            }
            return this.prevMoveNext;
        }

        public void Reset()
        {
            this.datumConsumed = false;
            this.enumerator.Reset();
        }

        public long Current
        {
            get
            {
                return (this.currentDatum ? this.extraDatum : this.enumerator.Current);
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