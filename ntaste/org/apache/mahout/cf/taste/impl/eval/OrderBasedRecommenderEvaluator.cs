namespace org.apache.mahout.cf.taste.impl.eval
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.model;
    using org.apache.mahout.cf.taste.recommender;
    using System;
    using System.Collections.Generic;

    public sealed class OrderBasedRecommenderEvaluator
    {
        private static Logger log = LoggerFactory.getLogger(typeof(OrderBasedRecommenderEvaluator));

        private OrderBasedRecommenderEvaluator()
        {
        }

        public static void evaluate(DataModel model1, DataModel model2, int samples, RunningAverage tracker, string tag)
        {
            printHeader();
            IEnumerator<long> enumerator = model1.getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                PreferenceArray prefs = model1.getPreferencesFromUser(current);
                PreferenceArray array2 = model2.getPreferencesFromUser(current);
                prefs.sortByValueReversed();
                array2.sortByValueReversed();
                FastIDSet modelSet = new FastIDSet();
                long num2 = setBits(modelSet, prefs, samples);
                FastIDSet set2 = new FastIDSet();
                num2 = Math.Max(num2, setBits(set2, array2, samples));
                int max = Math.Min(mask(modelSet, set2, num2), samples);
                if (max >= 2)
                {
                    long[] itemsL = getCommonItems(modelSet, prefs, max);
                    long[] itemsR = getCommonItems(modelSet, array2, max);
                    double datum = scoreCommonSubset(tag, current, samples, max, itemsL, itemsR);
                    tracker.addDatum(datum);
                }
            }
        }

        public static void evaluate(Recommender recommender, DataModel model, int samples, RunningAverage tracker, string tag)
        {
            printHeader();
            IEnumerator<long> enumerator = recommender.getDataModel().getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                List<RecommendedItem> items = recommender.recommend(current, model.getNumItems());
                PreferenceArray prefs = model.getPreferencesFromUser(current);
                prefs.sortByValueReversed();
                FastIDSet modelSet = new FastIDSet();
                long num2 = setBits(modelSet, items, samples);
                FastIDSet set2 = new FastIDSet();
                num2 = Math.Max(num2, setBits(set2, prefs, samples));
                int max = Math.Min(mask(modelSet, set2, num2), samples);
                if (max >= 2)
                {
                    long[] itemsL = getCommonItems(modelSet, items, max);
                    long[] itemsR = getCommonItems(modelSet, prefs, max);
                    double datum = scoreCommonSubset(tag, current, samples, max, itemsL, itemsR);
                    tracker.addDatum(datum);
                }
            }
        }

        public static void evaluate(Recommender recommender1, Recommender recommender2, int samples, RunningAverage tracker, string tag)
        {
            printHeader();
            IEnumerator<long> enumerator = recommender1.getDataModel().getUserIDs();
            while (enumerator.MoveNext())
            {
                long current = enumerator.Current;
                List<RecommendedItem> items = recommender1.recommend(current, samples);
                List<RecommendedItem> list2 = recommender2.recommend(current, samples);
                FastIDSet modelSet = new FastIDSet();
                long num2 = setBits(modelSet, items, samples);
                FastIDSet set2 = new FastIDSet();
                num2 = Math.Max(num2, setBits(set2, list2, samples));
                int max = Math.Min(mask(modelSet, set2, num2), samples);
                if (max >= 2)
                {
                    long[] itemsL = getCommonItems(modelSet, items, max);
                    long[] itemsR = getCommonItems(modelSet, list2, max);
                    double datum = scoreCommonSubset(tag, current, samples, max, itemsL, itemsR);
                    tracker.addDatum(datum);
                }
            }
        }

        private static long[] getCommonItems(FastIDSet commonSet, IEnumerable<RecommendedItem> recs, int max)
        {
            long[] numArray = new long[max];
            int num = 0;
            foreach (RecommendedItem item in recs)
            {
                long key = item.getItemID();
                if (commonSet.contains(key))
                {
                    numArray[num++] = key;
                }
                if (num == max)
                {
                    return numArray;
                }
            }
            return numArray;
        }

        private static long[] getCommonItems(FastIDSet commonSet, PreferenceArray prefs1, int max)
        {
            long[] numArray = new long[max];
            int num = 0;
            for (int i = 0; i < prefs1.length(); i++)
            {
                long key = prefs1.getItemID(i);
                if (commonSet.contains(key))
                {
                    numArray[num++] = key;
                }
                if (num == max)
                {
                    return numArray;
                }
            }
            return numArray;
        }

        private static double getMeanRank(int[] ranks)
        {
            int length = ranks.Length;
            double num2 = 0.0;
            foreach (int num3 in ranks)
            {
                num2 += num3;
            }
            return (num2 / ((double)length));
        }

        private static double getMeanWminus(double[] ranks)
        {
            int length = ranks.Length;
            double num2 = 0.0;
            foreach (double num3 in ranks)
            {
                if (num3 < 0.0)
                {
                    num2 -= num3;
                }
            }
            return (num2 / ((double)length));
        }

        private static double getMeanWplus(double[] ranks)
        {
            int length = ranks.Length;
            double num2 = 0.0;
            foreach (double num3 in ranks)
            {
                if (num3 > 0.0)
                {
                    num2 += num3;
                }
            }
            return (num2 / ((double)length));
        }

        private static void getVectorZ(long[] itemsR, long[] itemsL, int[] vectorZ, int[] vectorZabs)
        {
            int num;
            bool[] flagArray = new bool[itemsL.Length];
            for (num = 0; num < flagArray.Length; num++)
            {
                flagArray[num] = false;
            }
            int length = itemsR.Length;
            int num3 = 0;
            int num4 = length - 1;
            for (num = 0; num < length; num++)
            {
                long num5 = itemsR[num];
                for (int i = num3; i <= num4; i++)
                {
                    if (!flagArray[i])
                    {
                        long num7 = itemsL[i];
                        if (num5 == num7)
                        {
                            vectorZ[num] = num - i;
                            vectorZabs[num] = Math.Abs((int)(num - i));
                            if (i == num3)
                            {
                                num3++;
                            }
                            else if (i == num4)
                            {
                                num4--;
                            }
                            else
                            {
                                flagArray[i] = true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private static int mask(FastIDSet commonSet, FastIDSet otherSet, long maxItemID)
        {
            int num = 0;
            for (int i = 0; i <= maxItemID; i++)
            {
                if (commonSet.contains((long)i))
                {
                    if (otherSet.contains((long)i))
                    {
                        num++;
                    }
                    else
                    {
                        commonSet.remove((long)i);
                    }
                }
            }
            return num;
        }

        private static double normalWilcoxon(int[] vectorZ, int[] vectorZabs)
        {
            int length = vectorZ.Length;
            double[] ranks = new double[length];
            double[] ranksAbs = new double[length];
            wilcoxonRanks(vectorZ, vectorZabs, ranks, ranksAbs);
            return Math.Min(getMeanWplus(ranks), getMeanWminus(ranks));
        }

        private static void printHeader()
        {
            log.info("tag,user,samples,common,hamming,bubble,rank,normal,score", new object[0]);
        }

        private static double scoreCommonSubset(string tag, long userID, int samples, int subset, long[] itemsL, long[] itemsR)
        {
            int[] vectorZ = new int[subset];
            int[] vectorZabs = new int[subset];
            long num = sort(itemsL, itemsR);
            int num2 = slidingWindowHamming(itemsR, itemsL);
            if (num2 > samples)
            {
                throw new InvalidOperationException();
            }
            getVectorZ(itemsR, itemsL, vectorZ, vectorZabs);
            double num3 = normalWilcoxon(vectorZ, vectorZabs);
            double d = getMeanRank(vectorZabs);
            double num5 = Math.Sqrt(d);
            log.info("{},{},{},{},{},{},{},{},{}", new object[] { tag, userID, samples, subset, num2, num, d, num3, num5 });
            return num5;
        }

        private static long setBits(FastIDSet modelSet, List<RecommendedItem> items, int max)
        {
            long num = -1L;
            for (int i = 0; (i < items.Count) && (i < max); i++)
            {
                long key = items[i].getItemID();
                modelSet.add(key);
                if (key > num)
                {
                    num = key;
                }
            }
            return num;
        }

        private static long setBits(FastIDSet modelSet, PreferenceArray prefs, int max)
        {
            long num = -1L;
            for (int i = 0; (i < prefs.length()) && (i < max); i++)
            {
                long key = prefs.getItemID(i);
                modelSet.add(key);
                if (key > num)
                {
                    num = key;
                }
            }
            return num;
        }

        private static int slidingWindowHamming(long[] itemsR, long[] itemsL)
        {
            int num = 0;
            int length = itemsR.Length;
            if (itemsR[0].Equals(itemsL[0]) || itemsR[0].Equals(itemsL[1]))
            {
                num++;
            }
            for (int i = 1; i < (length - 1); i++)
            {
                long num4 = itemsL[i];
                if (((itemsR[i] == num4) || (itemsR[i - 1] == num4)) || (itemsR[i + 1] == num4))
                {
                    num++;
                }
            }
            if (itemsR[length - 1].Equals(itemsL[length - 1]) || itemsR[length - 1].Equals(itemsL[length - 2]))
            {
                num++;
            }
            return num;
        }

        private static long sort(long[] itemsL, long[] itemsR)
        {
            int length = itemsL.Length;
            if (length < 2)
            {
                return 0L;
            }
            if (length == 2)
            {
                return ((itemsL[0] == itemsR[0]) ? ((long)0) : ((long)1));
            }
            long[] numArray = new long[length];
            long[] numArray2 = new long[length];
            for (int i = 0; i < length; i++)
            {
                numArray[i] = itemsL[i];
                numArray2[i] = itemsR[i];
            }
            int index = 0;
            long num4 = 0L;
            while (index < (length - 1))
            {
                while ((length > 0) && (numArray[length - 1] == numArray2[length - 1]))
                {
                    length--;
                }
                if (length == 0)
                {
                    return num4;
                }
                if (numArray[index] == numArray2[index])
                {
                    index++;
                }
                else
                {
                    for (int j = index; j < (length - 1); j++)
                    {
                        int num6 = 1;
                        if (numArray[j] == numArray2[j])
                        {
                            while (((j + num6) < length) && (numArray[j + num6] == numArray2[j + num6]))
                            {
                                num6++;
                            }
                        }
                        if (((j + num6) < length) && ((numArray[j] != numArray2[j]) || (numArray[j + num6] != numArray2[j + num6])))
                        {
                            long num7 = numArray2[j];
                            numArray2[j] = numArray2[j + 1];
                            numArray2[j + 1] = num7;
                            num4 += 1L;
                        }
                    }
                }
            }
            return num4;
        }

        private static void wilcoxonRanks(int[] vectorZ, int[] vectorZabs, double[] ranks, double[] ranksAbs)
        {
            int length = vectorZ.Length;
            int[] array = (int[])vectorZabs.Clone();
            Array.Sort<int>(array);
            int index = 0;
            while (index < length)
            {
                if (array[index] > 0)
                {
                    break;
                }
                index++;
            }
            for (int i = 0; i < length; i++)
            {
                double num4 = 0.0;
                int num5 = 0;
                int num6 = vectorZabs[i];
                for (int j = 0; j < length; j++)
                {
                    if (num6 == array[j])
                    {
                        num4 += (j + 1) - index;
                        num5++;
                    }
                    else if (num6 < array[j])
                    {
                        break;
                    }
                }
                if (vectorZ[i] != 0)
                {
                    ranks[i] = (num4 / ((double)num5)) * ((vectorZ[i] < 0) ? ((double)(-1)) : ((double)1));
                    ranksAbs[i] = Math.Abs(ranks[i]);
                }
            }
        }
    }
}