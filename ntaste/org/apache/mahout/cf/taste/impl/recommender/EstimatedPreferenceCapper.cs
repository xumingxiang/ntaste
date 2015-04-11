namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.model;

    public sealed class EstimatedPreferenceCapper
    {
        private float max;
        private float min;

        public EstimatedPreferenceCapper(DataModel model)
        {
            this.min = model.getMinPreference();
            this.max = model.getMaxPreference();
        }

        public float capEstimate(float estimate)
        {
            if (estimate > this.max)
            {
                estimate = this.max;
                return estimate;
            }
            if (estimate < this.min)
            {
                estimate = this.min;
            }
            return estimate;
        }
    }
}