namespace org.apache.mahout.cf.taste.impl.similarity
{
    using System;

    internal sealed class longPairMatchPredicate
    {
        private long id;

        internal longPairMatchPredicate(long id)
        {
            this.id = id;
        }

        public bool matches(Tuple<long, long> pair)
        {
            return ((pair.Item1 == this.id) || (pair.Item2 == this.id));
        }
    }
}