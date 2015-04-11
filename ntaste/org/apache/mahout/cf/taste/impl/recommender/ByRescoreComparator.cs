namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.recommender;
    using System.Collections.Generic;

    public sealed class ByRescoreComparator : IComparer<RecommendedItem>
    {
        private IDRescorer rescorer;

        public ByRescoreComparator(IDRescorer rescorer)
        {
            this.rescorer = rescorer;
        }

        public int Compare(RecommendedItem o1, RecommendedItem o2)
        {
            double num;
            double num2;
            if (this.rescorer == null)
            {
                num = o1.getValue();
                num2 = o2.getValue();
            }
            else
            {
                num = this.rescorer.rescore(o1.getItemID(), (double)o1.getValue());
                num2 = this.rescorer.rescore(o2.getItemID(), (double)o2.getValue());
            }
            if (num < num2)
            {
                return 1;
            }
            if (num > num2)
            {
                return -1;
            }
            return 0;
        }

        public override string ToString()
        {
            return ("ByRescoreComparator[rescorer:" + this.rescorer + ']');
        }
    }
}