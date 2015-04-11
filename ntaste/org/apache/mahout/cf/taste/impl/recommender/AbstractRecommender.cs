namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System.Collections.Generic;

    public abstract class AbstractRecommender : Recommender, Refreshable
    {
        private CandidateItemsStrategy candidateItemsStrategy;
        private DataModel dataModel;
        private static Logger log = LoggerFactory.getLogger(typeof(AbstractRecommender));

        protected AbstractRecommender(DataModel dataModel)
            : this(dataModel, getDefaultCandidateItemsStrategy())
        {
        }

        protected AbstractRecommender(DataModel dataModel, CandidateItemsStrategy candidateItemsStrategy)
        {
            this.dataModel = dataModel;
            this.candidateItemsStrategy = candidateItemsStrategy;
        }

        public abstract float estimatePreference(long userID, long itemID);

        protected virtual FastIDSet getAllOtherItems(long userID, PreferenceArray preferencesFromUser)
        {
            return this.candidateItemsStrategy.getCandidateItems(userID, preferencesFromUser, this.dataModel);
        }

        public virtual DataModel getDataModel()
        {
            return this.dataModel;
        }

        protected static CandidateItemsStrategy getDefaultCandidateItemsStrategy()
        {
            return new PreferredItemsNeighborhoodCandidateItemsStrategy();
        }

        public virtual List<RecommendedItem> recommend(long userID, int howMany)
        {
            return this.recommend(userID, howMany, null);
        }

        public abstract List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer);

        public abstract void refresh(IList<Refreshable> alreadyRefreshed);

        public virtual void removePreference(long userID, long itemID)
        {
            log.debug("Remove preference for user '{}', item '{}'", new object[] { userID, itemID });
            this.dataModel.removePreference(userID, itemID);
        }

        public virtual void setPreference(long userID, long itemID, float value)
        {
            log.debug("Setting preference for user {}, item {}", new object[] { userID, itemID });
            this.dataModel.setPreference(userID, itemID, value);
        }
    }
}