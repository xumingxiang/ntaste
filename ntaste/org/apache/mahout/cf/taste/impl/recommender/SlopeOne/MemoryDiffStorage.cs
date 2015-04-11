using org.apache.mahout.cf.taste.common;
using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.model;
using org.apache.mahout.cf.taste.recommender.slopeone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace org.apache.mahout.cf.taste.impl.recommender.slopeone
{
    public class MemoryDiffStorage : DiffStorage
    {
        private static Logger log = LoggerFactory.getLogger(typeof(MemoryDiffStorage));

        private readonly DataModel dataModel;
        private bool stdDevWeighted;
        private long maxEntries;
        private FastByIDMap<FastByIDMap<RunningAverage>> averageDiffs;
        private FastByIDMap<RunningAverage> averageItemPref;
        private FastIDSet allRecommendableItemIDs;
        private ReaderWriterLockSlim buildAverageDiffsLock;

        private RefreshHelper refreshHelper;

        public MemoryDiffStorage(DataModel dataModel,
                           Weighting stdDevWeighted,
                           long maxEntries)
        {
            //Preconditions.checkArgument(dataModel != null, "dataModel is null");
            //Preconditions.checkArgument(dataModel.getNumItems() >= 1, "dataModel has no items");
            //Preconditions.checkArgument(maxEntries > 0L, "maxEntries must be positive");
            this.dataModel = dataModel;
            this.stdDevWeighted = stdDevWeighted == Weighting.WEIGHTED;
            this.maxEntries = maxEntries;
            this.averageDiffs = new FastByIDMap<FastByIDMap<RunningAverage>>();
            this.averageItemPref = new FastByIDMap<RunningAverage>();
            this.buildAverageDiffsLock = new ReaderWriterLockSlim();
            this.allRecommendableItemIDs = new FastIDSet(dataModel.getNumItems());
            this.refreshHelper = new RefreshHelper(buildAverageDiffs);
            refreshHelper.addDependency(dataModel);
            buildAverageDiffs();
        }

        public common.RunningAverage getDiff(long itemID1, long itemID2)
        {
            bool inverted = false;
            if (itemID1 > itemID2)
            {
                inverted = true;
                long temp = itemID1;
                itemID1 = itemID2;
                itemID2 = temp;
            }

            FastByIDMap<RunningAverage> level2Map;
            try
            {
                //buildAverageDiffsLock.EnterReadLock();
                lock (this)
                {
                    level2Map = averageDiffs.get(itemID1);
                }
            }
            finally
            {
                //buildAverageDiffsLock.ExitReadLock();
            }
            RunningAverage average = null;
            if (level2Map != null)
            {
                average = level2Map.get(itemID2);
            }
            if (inverted)
            {
                if (average == null)
                {
                    return null;
                }
                else
                {
                    return average.inverse();
                };

                //return average == null ? null : average.inverse();
            }
            else
            {
                return average;
            }
        }

        public common.RunningAverage[] getDiffs(long userID, long itemID, taste.model.PreferenceArray prefs)
        {
            try
            {
                buildAverageDiffsLock.EnterReadLock();
                int size = prefs.length();
                RunningAverage[] result = new RunningAverage[size];
                for (int i = 0; i < size; i++)
                {
                    result[i] = getDiff(prefs.getItemID(i), itemID);
                }
                return result;
            }
            finally
            {
                buildAverageDiffsLock.ExitReadLock();
            }
        }

        public common.RunningAverage getAverageItemPref(long itemID)
        {
            return averageItemPref.get(itemID);
        }

        public void addItemPref(long userID, long itemIDA, float prefValue)
        {
            PreferenceArray userPreferences = dataModel.getPreferencesFromUser(userID);
            try
            {
                buildAverageDiffsLock.EnterWriteLock();

                FastByIDMap<RunningAverage> aMap = averageDiffs.get(itemIDA);
                if (aMap == null)
                {
                    aMap = new FastByIDMap<RunningAverage>();
                    averageDiffs.put(itemIDA, aMap);
                }
                int length = userPreferences.length();
                for (int i = 0; i < length; i++)
                {
                    long itemIDB = userPreferences.getItemID(i);
                    float bValue = userPreferences.getValue(i);
                    if (itemIDA < itemIDB)
                    {
                        RunningAverage average = aMap.get(itemIDB);
                        if (average == null)
                        {
                            average = buildRunningAverage();
                            aMap.put(itemIDB, average);
                        }
                        average.addDatum(bValue - prefValue);
                    }
                    else
                    {
                        FastByIDMap<RunningAverage> bMap = averageDiffs.get(itemIDB);
                        if (bMap == null)
                        {
                            bMap = new FastByIDMap<RunningAverage>();
                            averageDiffs.put(itemIDB, bMap);
                        }
                        RunningAverage average = bMap.get(itemIDA);
                        if (average == null)
                        {
                            average = buildRunningAverage();
                            bMap.put(itemIDA, average);
                        }
                        average.addDatum(prefValue - bValue);
                    }
                }
            }
            finally
            {
                buildAverageDiffsLock.ExitWriteLock();
            }
        }

        public void updateItemPref(long itemID, float prefDelta)
        {
            if (stdDevWeighted)
            {
                throw new Exception("Can't update only when stdDevWeighted is set");
            }
            try
            {
                buildAverageDiffsLock.EnterReadLock();
                foreach (var entry in averageDiffs.entrySet())
                {
                    bool matchesItemID1 = itemID == entry.Key;
                    foreach (var entry2 in entry.Value.entrySet())
                    {
                        RunningAverage average = entry2.Value;
                        if (matchesItemID1)
                        {
                            average.changeDatum(-prefDelta);
                        }
                        else if (itemID == entry2.Key)
                        {
                            average.changeDatum(prefDelta);
                        }
                    }
                }
                RunningAverage itemAverage = averageItemPref.get(itemID);
                if (itemAverage != null)
                {
                    itemAverage.changeDatum(prefDelta);
                }
            }
            finally
            {
                buildAverageDiffsLock.ExitReadLock();
            }
        }

        public void removeItemPref(long userID, long itemIDA, float prefValue)
        {
            PreferenceArray userPreferences = dataModel.getPreferencesFromUser(userID);
            try
            {
                buildAverageDiffsLock.EnterWriteLock();

                FastByIDMap<RunningAverage> aMap = averageDiffs.get(itemIDA);

                int length = userPreferences.length();
                for (int i = 0; i < length; i++)
                {
                    long itemIDB = userPreferences.getItemID(i);
                    float bValue = userPreferences.getValue(i);

                    if (itemIDA < itemIDB)
                    {
                        if (aMap != null)
                        {
                            RunningAverage average = aMap.get(itemIDB);
                            if (average != null)
                            {
                                if (average.getCount() <= 1)
                                {
                                    aMap.remove(itemIDB);
                                }
                                else
                                {
                                    average.removeDatum(bValue - prefValue);
                                }
                            }
                        }
                    }
                    else if (itemIDA > itemIDB)
                    {
                        FastByIDMap<RunningAverage> bMap = averageDiffs.get(itemIDB);
                        if (bMap != null)
                        {
                            RunningAverage average = bMap.get(itemIDA);
                            if (average != null)
                            {
                                if (average.getCount() <= 1)
                                {
                                    aMap.remove(itemIDA);
                                }
                                else
                                {
                                    average.removeDatum(prefValue - bValue);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                buildAverageDiffsLock.ExitWriteLock();
            }
        }

        public common.FastIDSet getRecommendableItemIDs(long userID)
        {
            FastIDSet result;
            try
            {
                buildAverageDiffsLock.EnterReadLock();
                result = (FastIDSet)allRecommendableItemIDs.Clone();
            }
            finally
            {
                buildAverageDiffsLock.ExitReadLock();
            }
            for (int i = 0; i < result.Count(); i++)
            {
                var item = result.ElementAt(i);
                if (dataModel.getPreferenceValue(userID, item) != null)
                {
                    result.remove(item);
                    i--;
                }
            }
            return result;
        }

        public void refresh(IList<taste.common.Refreshable> alreadyRefreshed)
        {
            refreshHelper.refresh(alreadyRefreshed);
        }

        private void buildAverageDiffs()
        {
            log.info("Building average diffs...");
            try
            {
                buildAverageDiffsLock.EnterWriteLock();
                averageDiffs.clear();
                long averageCount = 0L;
                var it = dataModel.getUserIDs();
                while (it.MoveNext())
                {
                    averageCount = processOneUser(averageCount, it.Current);
                }
                pruneInconsequentialDiffs();
                updateAllRecommendableItems();
            }
            finally
            {
                buildAverageDiffsLock.ExitWriteLock();
            }
        }

        private void updateAllRecommendableItems()
        {
            FastIDSet ids = new FastIDSet(dataModel.getNumItems());
            foreach (var entry in averageDiffs.entrySet())
            {
                ids.add(entry.Key);
                var it = entry.Value.Keys;
                foreach (var item in it)
                {
                    ids.add(item);
                }
            }
            allRecommendableItemIDs.clear();
            allRecommendableItemIDs.addAll(ids);
            allRecommendableItemIDs.rehash();
        }

        private void pruneInconsequentialDiffs()
        {
            // Go back and prune inconsequential diffs. "Inconsequential" means, here, only represented by one
            // data point, so possibly unreliable
            //Iterator<Map.Entry<long, FastByIDMap<RunningAverage>>> it1 = averageDiffs.entrySet().iterator();
            var it1 = averageDiffs.entrySet().ToList();
            for (int k = 0; k < it1.Count; k++)
            {
                FastByIDMap<RunningAverage> map = it1[k].Value;
                var it2 = map.entrySet().ToList();
                for (int i = 0; i < it2.Count; i++)
                {
                    RunningAverage average = it2[i].Value;
                    if (average.getCount() <= 1)
                    {
                        map.remove(it2[i].Key);
                    }
                }
                if (map.isEmpty())
                {
                    averageDiffs.remove(it1[k].Key);
                }
                else
                {
                    map.rehash();
                }
            }
            averageDiffs.rehash();
        }

        private long processOneUser(long averageCount, long userID)
        {
            log.debug("Processing prefs for user {}", userID);
            // Save off prefs for the life of this loop iteration
            PreferenceArray userPreferences = dataModel.getPreferencesFromUser(userID);
            int length = userPreferences.length();
            for (int i = 0; i < length - 1; i++)
            {
                float prefAValue = userPreferences.getValue(i);
                long itemIDA = userPreferences.getItemID(i);
                FastByIDMap<RunningAverage> aMap = averageDiffs.get(itemIDA);
                if (aMap == null)
                {
                    aMap = new FastByIDMap<RunningAverage>();
                    averageDiffs.put(itemIDA, aMap);
                }
                for (int j = i + 1; j < length; j++)
                {
                    // This is a performance-critical block
                    long itemIDB = userPreferences.getItemID(j);
                    RunningAverage average = aMap.get(itemIDB);
                    if (average == null && averageCount < maxEntries)
                    {
                        average = buildRunningAverage();
                        aMap.put(itemIDB, average);
                        averageCount++;
                    }
                    if (average != null)
                    {
                        average.addDatum(userPreferences.getValue(j) - prefAValue);
                    }
                }
                RunningAverage itemAverage = averageItemPref.get(itemIDA);
                if (itemAverage == null)
                {
                    itemAverage = buildRunningAverage();
                    averageItemPref.put(itemIDA, itemAverage);
                }
                itemAverage.addDatum(prefAValue);
            }
            return averageCount;
        }

        private RunningAverage buildRunningAverage()
        {
            return stdDevWeighted ? new FullRunningAverageAndStdDev() : new FullRunningAverage();
        }
    }
}