namespace org.apache.mahout.cf.taste.impl.neighborhood
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System.Collections.Generic;

    public sealed class ThresholdUserNeighborhood : AbstractUserNeighborhood
    {
        private double threshold;

        public ThresholdUserNeighborhood(double threshold, UserSimilarity userSimilarity, DataModel dataModel)
            : this(threshold, userSimilarity, dataModel, 1.0)
        {
        }

        public ThresholdUserNeighborhood(double threshold, UserSimilarity userSimilarity, DataModel dataModel, double samplingRate)
            : base(userSimilarity, dataModel, samplingRate)
        {
            this.threshold = threshold;
        }

        public override long[] getUserNeighborhood(long userID)
        {
            DataModel model = this.getDataModel();
            FastIDSet set = new FastIDSet();
            IEnumerator<long> enumerator = SamplingLongPrimitiveIterator.maybeWrapIterator(model.getUserIDs(), this.getSamplingRate());
            UserSimilarity similarity = this.getUserSimilarity();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                if (userID != current)
                {
                    double d = similarity.userSimilarity(userID, current);
                    if (!(double.IsNaN(d) || (d < this.threshold)))
                    {
                        set.add(current);
                    }
                }
            }
            return set.toArray();
        }

        public override string ToString()
        {
            return "ThresholdUserNeighborhood";
        }
    }
}