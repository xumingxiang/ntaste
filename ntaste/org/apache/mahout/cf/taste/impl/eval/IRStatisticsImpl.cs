namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste.eval;

    public sealed class IRStatisticsImpl : IRStatistics
    {
        private double fallOut;
        private double ndcg;
        private double precision;
        private double reach;
        private double recall;

        public IRStatisticsImpl(double precision, double recall, double fallOut, double ndcg, double reach)
        {
            this.precision = precision;
            this.recall = recall;
            this.fallOut = fallOut;
            this.ndcg = ndcg;
            this.reach = reach;
        }

        public double getF1Measure()
        {
            return this.getFNMeasure(1.0);
        }

        public double getFallOut()
        {
            return this.fallOut;
        }

        public double getFNMeasure(double b)
        {
            double num = b * b;
            double num2 = (num * this.precision) + this.recall;
            return ((num2 == 0.0) ? double.NaN : ((((1.0 + num) * this.precision) * this.recall) / num2));
        }

        public double getNormalizedDiscountedCumulativeGain()
        {
            return this.ndcg;
        }

        public double getPrecision()
        {
            return this.precision;
        }

        public double getReach()
        {
            return this.reach;
        }

        public double getRecall()
        {
            return this.recall;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { "IRStatisticsImpl[precision:", this.precision, ",recall:", this.recall, ",fallOut:", this.fallOut, ",nDCG:", this.ndcg, ",reach:", this.reach, ']' });
        }
    }
}