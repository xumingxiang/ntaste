namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.recommender;
    using System.Collections.Generic;

    public sealed class ByValueRecommendedItemComparator : IComparer<RecommendedItem>
    {
        private static IComparer<RecommendedItem> INSTANCE = new ByValueRecommendedItemComparator();

        public int Compare(RecommendedItem o1, RecommendedItem o2)
        {
            float num = o1.getValue();
            float num2 = o2.getValue();
            return ((num > num2) ? -1 : ((num < num2) ? 1 : o1.getItemID().CompareTo(o2.getItemID())));
        }

        public static IComparer<RecommendedItem> getInstance()
        {
            return INSTANCE;
        }

        public static IComparer<RecommendedItem> getReverseInstance()
        {
            return new ReverseComparer<RecommendedItem>(INSTANCE);
        }

        internal class ReverseComparer<T> : IComparer<T>
        {
            private IComparer<T> comparer;

            internal ReverseComparer(IComparer<T> comparer)
            {
                this.comparer = comparer;
            }

            public int Compare(T obj1, T obj2)
            {
                return -this.comparer.Compare(obj1, obj2);
            }
        }
    }
}