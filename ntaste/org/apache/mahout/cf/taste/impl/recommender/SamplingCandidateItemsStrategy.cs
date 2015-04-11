namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.commons.math3.util;
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using System.Collections.Generic;

    public class SamplingCandidateItemsStrategy : AbstractCandidateItemsStrategy
    {
        public const int DEFAULT_FACTOR = 30;
        private static Logger log = LoggerFactory.getLogger(typeof(SamplingCandidateItemsStrategy));
        private static double LOG2 = MathUtil.Log(2.0);
        private static int MAX_LIMIT = 0x7fffffff;
        private int maxItems;
        private int maxItemsPerUser;
        private int maxUsersPerItem;
        public const int NO_LIMIT_FACTOR = 0x7fffffff;

        public SamplingCandidateItemsStrategy(int numUsers, int numItems)
            : this(30, 30, 30, numUsers, numItems)
        {
        }

        public SamplingCandidateItemsStrategy(int itemsFactor, int usersPerItemFactor, int candidatesPerUserFactor, int numUsers, int numItems)
        {
            this.maxItems = computeMaxFrom(itemsFactor, numItems);
            this.maxUsersPerItem = computeMaxFrom(usersPerItemFactor, numUsers);
            this.maxItemsPerUser = computeMaxFrom(candidatesPerUserFactor, numItems);
            log.debug("maxItems {0}, maxUsersPerItem {0}, maxItemsPerUser {0}", new object[] { this.maxItems, this.maxUsersPerItem, this.maxItemsPerUser });
        }

        private void addSomeOf(FastIDSet possibleItemIDs, FastIDSet itemIDs)
        {
            if (itemIDs.size() > this.maxItemsPerUser)
            {
                SamplingLongPrimitiveIterator iterator = new SamplingLongPrimitiveIterator(itemIDs.GetEnumerator(), ((double)this.maxItemsPerUser) / ((double)itemIDs.size()));
                while (iterator.MoveNext())
                {
                    possibleItemIDs.add(iterator.Current);
                }
            }
            else
            {
                possibleItemIDs.addAll(itemIDs);
            }
        }

        private static int computeMaxFrom(int factor, int numThings)
        {
            if (factor == 0x7fffffff)
            {
                return MAX_LIMIT;
            }
            long num = (long)(factor * (1.0 + (MathUtil.Log((double)numThings) / LOG2)));
            return ((num > MAX_LIMIT) ? MAX_LIMIT : ((int)num));
        }

        protected override FastIDSet doGetCandidateItems(long[] preferredItemIDs, DataModel dataModel)
        {
            IEnumerator<long> enumerator = ((IEnumerable<long>)preferredItemIDs).GetEnumerator();
            if (preferredItemIDs.Length > this.maxItems)
            {
                double samplingRate = ((double)this.maxItems) / ((double)preferredItemIDs.Length);
                log.info("preferredItemIDs.Length {0}, samplingRate {1}", new object[] { preferredItemIDs.Length, samplingRate });
                enumerator = new SamplingLongPrimitiveIterator(enumerator, samplingRate);
            }
            FastIDSet possibleItemIDs = new FastIDSet();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                PreferenceArray array = dataModel.getPreferencesForItem(current);
                int num3 = array.length();
                if (num3 > this.maxUsersPerItem)
                {
                    FixedSizeSamplingIterator<Preference> iterator = new FixedSizeSamplingIterator<Preference>(this.maxUsersPerItem, array.GetEnumerator());
                    while (iterator.MoveNext())
                    {
                        this.addSomeOf(possibleItemIDs, dataModel.getItemIDsFromUser(iterator.Current.getUserID()));
                    }
                }
                else
                {
                    for (int i = 0; i < num3; i++)
                    {
                        this.addSomeOf(possibleItemIDs, dataModel.getItemIDsFromUser(array.getUserID(i)));
                    }
                }
            }
            possibleItemIDs.removeAll(preferredItemIDs);
            return possibleItemIDs;
        }
    }
}