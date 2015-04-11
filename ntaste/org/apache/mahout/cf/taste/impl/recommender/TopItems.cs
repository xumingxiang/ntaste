namespace org.apache.mahout.cf.taste.impl.recommender
{
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.similarity;
    using org.apache.mahout.cf.taste.recommender;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class TopItems
    {
        private static long[] NO_IDS = new long[0];

        private TopItems()
        {
        }

        public static List<GenericItemSimilarity.ItemItemSimilarity> getTopItemItemSimilarities(int howMany, IEnumerator<GenericItemSimilarity.ItemItemSimilarity> allSimilarities)
        {
            SortedSet<GenericItemSimilarity.ItemItemSimilarity> collection = new SortedSet<GenericItemSimilarity.ItemItemSimilarity>();
            bool flag = false;
            double negativeInfinity = double.NegativeInfinity;
            while (allSimilarities.MoveNext())
            {
                GenericItemSimilarity.ItemItemSimilarity current = allSimilarities.Current;
                double d = current.getValue();
                if (!double.IsNaN(d) && (!flag || (d > negativeInfinity)))
                {
                    collection.Add(current);
                    if (flag)
                    {
                        collection.Remove(collection.Max);
                    }
                    else if (collection.Count > howMany)
                    {
                        flag = true;
                        collection.Remove(collection.Max);
                    }
                    negativeInfinity = collection.Max.getValue();
                }
            }
            List<GenericItemSimilarity.ItemItemSimilarity> list = new List<GenericItemSimilarity.ItemItemSimilarity>(collection.Count);
            list.AddRange(collection);
            return list;
        }

        public static List<RecommendedItem> getTopItems(int howMany, IEnumerator<long> possibleItemIDs, IDRescorer rescorer, Estimator<long> estimator)
        {
            SortedSet<RecommendedItem> collection = new SortedSet<RecommendedItem>(ByValueRecommendedItemComparator.getReverseInstance());
            bool flag = false;
            double negativeInfinity = double.NegativeInfinity;
            while (possibleItemIDs.MoveNext())
            {
                long current = possibleItemIDs.Current;
                if ((rescorer == null) || !rescorer.isFiltered(current))
                {
                    double num3;
                    try
                    {
                        num3 = estimator.estimate(current);
                    }
                    catch (NoSuchItemException)
                    {
                        continue;
                    }
                    double d = (rescorer == null) ? num3 : rescorer.rescore(current, num3);
                    if (!double.IsNaN(d) && (!flag || (d > negativeInfinity)))
                    {
                        collection.Add(new GenericRecommendedItem(current, (float)d));
                        if (flag)
                        {
                            collection.Remove(collection.Min);
                        }
                        else if (collection.Count > howMany)
                        {
                            flag = true;
                            collection.Remove(collection.Min);
                        }
                        negativeInfinity = collection.Min.getValue();
                    }
                }
            }
            int count = collection.Count;
            if (count == 0)
            {
                return new List<RecommendedItem>();
            }
            List<RecommendedItem> list = new List<RecommendedItem>(count);
            list.AddRange(collection);
            list.Reverse();
            return list;
        }

        public static long[] getTopUsers(int howMany, IEnumerator<long> allUserIDs, IDRescorer rescorer, Estimator<long> estimator)
        {
            SortedSet<SimilarUser> set = new SortedSet<SimilarUser>();
            bool flag = false;
            double negativeInfinity = double.NegativeInfinity;
            while (allUserIDs.MoveNext())
            {
                long current = allUserIDs.Current;
                if ((rescorer == null) || !rescorer.isFiltered(current))
                {
                    double num3;
                    try
                    {
                        num3 = estimator.estimate(current);
                    }
                    catch (NoSuchUserException)
                    {
                        continue;
                    }
                    double d = (rescorer == null) ? num3 : rescorer.rescore(current, num3);
                    if (!double.IsNaN(d) && (!flag || (d > negativeInfinity)))
                    {
                        set.Add(new SimilarUser(current, d));
                        if (flag)
                        {
                            set.Remove(set.Max);
                        }
                        else if (set.Count > howMany)
                        {
                            flag = true;
                            set.Remove(set.Max);
                        }
                        negativeInfinity = set.Max.getSimilarity();
                    }
                }
            }
            int count = set.Count;
            if (count == 0)
            {
                return NO_IDS;
            }
            List<SimilarUser> list = new List<SimilarUser>(count);
            return (from s in set select s.getUserID()).ToArray<long>();
        }

        public static List<GenericUserSimilarity.UserUserSimilarity> getTopUserUserSimilarities(int howMany, IEnumerator<GenericUserSimilarity.UserUserSimilarity> allSimilarities)
        {
            SortedSet<GenericUserSimilarity.UserUserSimilarity> collection = new SortedSet<GenericUserSimilarity.UserUserSimilarity>();
            bool flag = false;
            double negativeInfinity = double.NegativeInfinity;
            while (allSimilarities.MoveNext())
            {
                GenericUserSimilarity.UserUserSimilarity current = allSimilarities.Current;
                double d = current.getValue();
                if (!double.IsNaN(d) && (!flag || (d > negativeInfinity)))
                {
                    collection.Add(current);
                    if (flag)
                    {
                        collection.Remove(collection.Max);
                    }
                    else if (collection.Count > howMany)
                    {
                        flag = true;
                        collection.Remove(collection.Max);
                    }
                    negativeInfinity = collection.Max.getValue();
                }
            }
            List<GenericUserSimilarity.UserUserSimilarity> list = new List<GenericUserSimilarity.UserUserSimilarity>(collection.Count);
            list.AddRange(collection);
            return list;
        }

        public interface Estimator<T>
        {
            double estimate(T thing);
        }
    }
}