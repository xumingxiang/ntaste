namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.recommender;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System;
    using System.Collections.Generic;
    using System.IO;

    public sealed class SVDRecommender : AbstractRecommender
    {
        private Factorization factorization;
        private Factorizer factorizer;
        private static Logger log = LoggerFactory.getLogger(typeof(SVDRecommender));
        private PersistenceStrategy persistenceStrategy;
        private RefreshHelper refreshHelper;

        public SVDRecommender(DataModel dataModel, Factorizer factorizer)
            : this(dataModel, factorizer, AbstractRecommender.getDefaultCandidateItemsStrategy(), getDefaultPersistenceStrategy())
        {
        }

        public SVDRecommender(DataModel dataModel, Factorizer factorizer, PersistenceStrategy persistenceStrategy)
            : this(dataModel, factorizer, AbstractRecommender.getDefaultCandidateItemsStrategy(), persistenceStrategy)
        {
        }

        public SVDRecommender(DataModel dataModel, Factorizer factorizer, CandidateItemsStrategy candidateItemsStrategy)
            : this(dataModel, factorizer, candidateItemsStrategy, getDefaultPersistenceStrategy())
        {
        }

        public SVDRecommender(DataModel dataModel, Factorizer factorizer, CandidateItemsStrategy candidateItemsStrategy, PersistenceStrategy persistenceStrategy)
            : base(dataModel, candidateItemsStrategy)
        {
            Action refreshRunnable = null;
            this.factorizer = factorizer;
            this.persistenceStrategy = persistenceStrategy;
            try
            {
                this.factorization = persistenceStrategy.load();
            }
            catch (IOException exception)
            {
                throw new TasteException("Error loading factorization", exception);
            }
            if (this.factorization == null)
            {
                this.train();
            }
            if (refreshRunnable == null)
            {
                refreshRunnable = () => this.train();
            }
            this.refreshHelper = new RefreshHelper(refreshRunnable);
            this.refreshHelper.addDependency(this.getDataModel());
            this.refreshHelper.addDependency(factorizer);
            this.refreshHelper.addDependency(candidateItemsStrategy);
        }

        public override float estimatePreference(long userID, long itemID)
        {
            double[] numArray = this.factorization.getUserFeatures(userID);
            double[] numArray2 = this.factorization.getItemFeatures(itemID);
            double num = 0.0;
            for (int i = 0; i < numArray.Length; i++)
            {
                num += numArray[i] * numArray2[i];
            }
            return (float)num;
        }

        private static PersistenceStrategy getDefaultPersistenceStrategy()
        {
            return new NoPersistenceStrategy();
        }

        public override List<RecommendedItem> recommend(long userID, int howMany, IDRescorer rescorer)
        {
            log.debug("Recommending items for user ID '{}'", new object[] { userID });
            PreferenceArray preferencesFromUser = this.getDataModel().getPreferencesFromUser(userID);
            List<RecommendedItem> list = TopItems.getTopItems(howMany, this.getAllOtherItems(userID, preferencesFromUser).GetEnumerator(), rescorer, new Estimator(this, userID));
            log.debug("Recommendations are: {}", new object[] { list });
            return list;
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            this.refreshHelper.refresh(alreadyRefreshed);
        }

        private void train()
        {
            this.factorization = this.factorizer.factorize();
            try
            {
                this.persistenceStrategy.maybePersist(this.factorization);
            }
            catch (IOException exception)
            {
                throw new TasteException("Error persisting factorization", exception);
            }
        }

        private sealed class Estimator : TopItems.Estimator<long>
        {
            private SVDRecommender svdRecommender;
            private long theUserID;

            internal Estimator(SVDRecommender svdRecommender, long theUserID)
            {
                this.theUserID = theUserID;
                this.svdRecommender = svdRecommender;
            }

            public double estimate(long itemID)
            {
                return (double)this.svdRecommender.estimatePreference(this.theUserID, itemID);
            }
        }
    }
}