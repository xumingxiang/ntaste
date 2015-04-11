namespace org.apache.mahout.cf.taste.impl.similarity
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.similarity;
    using System.Collections.Generic;

    public sealed class AveragingPreferenceInferrer : PreferenceInferrer, Refreshable
    {
        private Cache<long, float> averagePreferenceValue;
        private DataModel dataModel;
        private static float ZERO = 0f;

        public AveragingPreferenceInferrer(DataModel dataModel)
        {
            this.dataModel = dataModel;
            Retriever<long, float> retriever = new PrefRetriever(this);
            this.averagePreferenceValue = new Cache<long, float>(retriever, dataModel.getNumUsers());
            this.refresh(null);
        }

        public float inferPreference(long userID, long itemID)
        {
            return this.averagePreferenceValue.get(userID);
        }

        public void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.averagePreferenceValue.clear();
        }

        public override string ToString()
        {
            return "AveragingPreferenceInferrer";
        }

        private sealed class PrefRetriever : Retriever<long, float>
        {
            private AveragingPreferenceInferrer inf;

            public PrefRetriever(AveragingPreferenceInferrer inf)
            {
                this.inf = inf;
            }

            public float get(long key)
            {
                PreferenceArray array = this.inf.dataModel.getPreferencesFromUser(key);
                int num = array.length();
                if (num == 0)
                {
                    return AveragingPreferenceInferrer.ZERO;
                }
                RunningAverage average = new FullRunningAverage();
                for (int i = 0; i < num; i++)
                {
                    average.addDatum((double)array.getValue(i));
                }
                return (float)average.getAverage();
            }
        }
    }
}