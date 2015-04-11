namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using org.apache.mahout.common;
    using System.Collections.Generic;

    public sealed class RandomRecommender : AbstractRecommender
    {
        private float maxPref;
        private float minPref;
        private RandomWrapper random;

        public RandomRecommender(DataModel dataModel)
            : base(dataModel)
        {
            this.random = RandomUtils.getRandom();
            float negativeInfinity = float.NegativeInfinity;
            float positiveInfinity = float.PositiveInfinity;
            IEnumerator<long> enumerator = dataModel.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                PreferenceArray array = dataModel.getPreferencesFromUser(current);
                for (int i = 0; i < array.length(); i++)
                {
                    float num5 = array.getValue(i);
                    if (num5 < positiveInfinity)
                    {
                        positiveInfinity = num5;
                    }
                    if (num5 > negativeInfinity)
                    {
                        negativeInfinity = num5;
                    }
                }
            }
            this.minPref = positiveInfinity;
            this.maxPref = negativeInfinity;
        }

        public override float estimatePreference(long userID, long itemID)
        {
            return this.randomPref();
        }

        private float randomPref()
        {
            return (this.minPref + (((float)this.random.nextDouble()) * (this.maxPref - this.minPref)));
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            DataModel model = this.getDataModel();
            int n = model.getNumItems();
            List<RecommendedItem> list = new List<RecommendedItem>(howMany);
            while (list.Count < howMany)
            {
                IEnumerator<long> enumerator = model.getItemIDs();
                enumerator.MoveNext();
                int num2 = this.random.nextInt(n);
                for (int i = 0; i < num2; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                }
                long current = enumerator.Current;
                if (!model.getPreferenceValue(userID, current).HasValue)
                {
                    list.Add(new GenericRecommendedItem(current, this.randomPref()));
                }
            }
            return list;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.getDataModel().refresh(alreadyRefreshed);
        }
    }
}