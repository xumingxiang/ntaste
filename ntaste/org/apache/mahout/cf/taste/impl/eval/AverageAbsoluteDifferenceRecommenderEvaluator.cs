namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System;

    public sealed class AverageAbsoluteDifferenceRecommenderEvaluator : AbstractDifferenceRecommenderEvaluator
    {
        private RunningAverage average;

        protected override double computeFinalEvaluation()
        {
            return this.average.getAverage();
        }

        protected override void processOneEstimate(float estimatedPreference, Preference realPref)
        {
            this.average.addDatum((double)Math.Abs((float)(realPref.getValue() - estimatedPreference)));
        }

        protected override void reset()
        {
            this.average = new FullRunningAverage();
        }

        public override string ToString()
        {
            return "AverageAbsoluteDifferenceRecommenderEvaluator";
        }
    }
}