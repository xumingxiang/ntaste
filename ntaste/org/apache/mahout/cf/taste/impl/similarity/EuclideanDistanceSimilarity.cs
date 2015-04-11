namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.model;
    using System;

    public sealed class EuclideanDistanceSimilarity : AbstractSimilarity
    {
        public EuclideanDistanceSimilarity(DataModel dataModel)
            : this(dataModel, Weighting.UNWEIGHTED)
        {
        }

        public EuclideanDistanceSimilarity(DataModel dataModel, Weighting weighting)
            : base(dataModel, weighting, false)
        {
        }

        protected override double computeResult(int n, double sumXY, double sumX2, double sumY2, double sumXYdiff2)
        {
            return (1.0 / (1.0 + (Math.Sqrt(sumXYdiff2) / Math.Sqrt((double)n))));
        }
    }
}