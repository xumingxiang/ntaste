﻿namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.model;
    using System;

    public sealed class UncenteredCosineSimilarity : AbstractSimilarity
    {
        public UncenteredCosineSimilarity(DataModel dataModel)
            : this(dataModel, Weighting.UNWEIGHTED)
        {
        }

        public UncenteredCosineSimilarity(DataModel dataModel, Weighting weighting)
            : base(dataModel, weighting, false)
        {
        }

        protected override double computeResult(int n, double sumXY, double sumX2, double sumY2, double sumXYdiff2)
        {
            if (n == 0)
            {
                return double.NaN;
            }
            double num = Math.Sqrt(sumX2) * Math.Sqrt(sumY2);
            if (num == 0.0)
            {
                return double.NaN;
            }
            return (sumXY / num);
        }
    }
}