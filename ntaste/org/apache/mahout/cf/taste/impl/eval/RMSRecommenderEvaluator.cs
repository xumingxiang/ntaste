namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;

    public sealed class RMSRecommenderEvaluator : AbstractDifferenceRecommenderEvaluator
    {
        private RunningAverage average;

        protected override double computeFinalEvaluation()
        {
            return Math.Sqrt(this.average.getAverage());
        }

        protected override void processOneEstimate(float estimatedPreference, Preference realPref)
        {
            double num = realPref.getValue() - estimatedPreference;
            this.average.addDatum(num * num);
        }

        protected override void reset()
        {
            this.average = new FullRunningAverage();
        }

        public override string ToString()
        {
            return "RMSRecommenderEvaluator";
        }
    }
}