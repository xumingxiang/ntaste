using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.recommender.slopeone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace org.apache.mahout.cf.taste.impl.recommender.slopeone.file
{
    public sealed class FileDiffStorage : DiffStorage
    {
        private static Logger log = LoggerFactory.getLogger(typeof(FileDiffStorage));

        private static long MIN_RELOAD_INTERVAL_MS = 60 * 1000L; // 1 minute?
        private static char COMMENT_CHAR = '#';

        //  private static Pattern SEPARATOR = Pattern.compile("[\t,]");
        private static Regex SEPARATOR = new Regex("[\t,]");

        private string dataFile;
        private long lastModified;
        private long maxEntries;
        private FastByIDMap<FastByIDMap<RunningAverage>> averageDiffs;
        private FastIDSet allRecommendableItemIDs;
        private ReaderWriterLockSlim buildAverageDiffsLock;

        public FileDiffStorage(string dataFile, long maxEntries)
        {
            //Preconditions.checkArgument(dataFile != null, "dataFile is null");
            if (!(File.Exists(dataFile) && !Directory.Exists(dataFile)))
            {
                throw new FileNotFoundException(dataFile);
            }
            if (new FileInfo(dataFile).Length <= 0L)
            {
                throw new ArgumentException("dataFile is empty");
            }

            //Preconditions.checkArgument(maxEntries > 0L, "maxEntries must be positive");
            log.info("Creating FileDataModel for file {}", dataFile);
            this.dataFile = dataFile;
            this.lastModified = ConvertTimeStamp(File.GetLastWriteTime(dataFile));
            this.maxEntries = maxEntries;
            this.averageDiffs = new FastByIDMap<FastByIDMap<RunningAverage>>();
            this.allRecommendableItemIDs = new FastIDSet();
            this.buildAverageDiffsLock = new ReaderWriterLockSlim();

            buildDiffs();
        }

        private void buildDiffs()
        {
            if (buildAverageDiffsLock.TryEnterWriteLock(-1))
            {
                try
                {
                    averageDiffs.clear();
                    allRecommendableItemIDs.clear();

                    //FileLineIterator iterator = new FileLineIterator(dataFile, false);
                    //String firstLine = iterator.peek();
                    //while (string.IsNullOrEmpty(firstLine) || firstLine[0] == COMMENT_CHAR)
                    //{
                    //    iterator.next();
                    //    firstLine = iterator.peek();
                    //}

                    StreamReader reader = new StreamReader(dataFile);
                    string firstLine = reader.ReadLine();
                    while ((firstLine == string.Empty) || (firstLine[0] == COMMENT_CHAR))
                    {
                        firstLine = reader.ReadLine();
                    }
                    long averageCount = 0L;
                    while (!reader.EndOfStream)
                    {
                        averageCount = processLine(reader.ReadLine(), averageCount);
                    }
                    reader.Close();

                    pruneInconsequentialDiffs();
                    updateAllRecommendableItems();
                }
                catch (IOException ioe)
                {
                    log.warn("Exception while reloading", ioe);
                }
                finally
                {
                    buildAverageDiffsLock.ExitWriteLock();
                }
            }
        }

        private long processLine(String line, long averageCount)
        {
            if (string.IsNullOrEmpty(line) || line[0] == COMMENT_CHAR)
            {
                return averageCount;
            }

            String[] tokens = SEPARATOR.Split(line);
            //Preconditions.checkArgument(tokens.length >= 3 && tokens.length != 5, "Bad line: %s", line);

            long itemID1 = long.Parse(tokens[0]);
            long itemID2 = long.Parse(tokens[1]);
            double diff = double.Parse(tokens[2]);
            int count = tokens.Length >= 4 ? int.Parse(tokens[3]) : 1;
            bool hasMkSk = tokens.Length >= 5;

            if (itemID1 > itemID2)
            {
                long temp = itemID1;
                itemID1 = itemID2;
                itemID2 = temp;
            }

            FastByIDMap<RunningAverage> level1Map = averageDiffs.get(itemID1);
            if (level1Map == null)
            {
                level1Map = new FastByIDMap<RunningAverage>();
                averageDiffs.put(itemID1, level1Map);
            }
            RunningAverage average = level1Map.get(itemID2);
            if (average != null)
            {
                throw new Exception("Duplicated line for item-item pair " + itemID1 + " / " + itemID2);
            }
            if (averageCount < maxEntries)
            {
                if (hasMkSk)
                {
                    double mk = Double.Parse(tokens[4]);
                    double sk = Double.Parse(tokens[5]);
                    average = new FullRunningAverageAndStdDev(count, diff, mk, sk);
                }
                else
                {
                    average = new FullRunningAverage(count, diff);
                }
                level1Map.put(itemID2, average);
                averageCount++;
            }

            allRecommendableItemIDs.add(itemID1);
            allRecommendableItemIDs.add(itemID2);

            return averageCount;
        }

        private void pruneInconsequentialDiffs()
        {
            // Go back and prune inconsequential diffs. "Inconsequential" means, here, only represented by one
            // data point, so possibly unreliable
            var it1 = averageDiffs.entrySet().ToList();

            for (int i = 0; i < it1.Count; i++)
            {
                FastByIDMap<RunningAverage> map = it1[i].Value;

                var it2 = map.entrySet().ToList();
                for (int j = 0; j < it2.Count; j++)
                {
                    RunningAverage average = it2[j].Value;
                    if (average.getCount() <= 1)
                    {
                        map.remove(it2[j].Key);
                    }
                }
                if (map.isEmpty())
                {
                    averageDiffs.remove(it1[i].Key);
                }
                else
                {
                    map.rehash();
                }
            }
            averageDiffs.rehash();
        }

        private void updateAllRecommendableItems()
        {
            foreach (var entry in averageDiffs.entrySet())
            {
                allRecommendableItemIDs.add(entry.Key);
                var keys = entry.Value.Keys;
                foreach (var key in keys)
                {
                    allRecommendableItemIDs.add(key);
                }
            }
            allRecommendableItemIDs.rehash();
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
                buildAverageDiffsLock.EnterReadLock();
                level2Map = averageDiffs.get(itemID1);
            }
            finally
            {
                buildAverageDiffsLock.ExitReadLock();
            }
            RunningAverage average = null;
            if (level2Map != null)
            {
                average = level2Map.get(itemID2);
            }
            if (inverted)
            {
                return average == null ? null : average.inverse();
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
            return null; // TODO can't do this without a DataModel
        }

        public void addItemPref(long userID, long itemID, float prefValue)
        {
            // Can't do this without a DataModel; should it just be a no-op?
            throw new NotImplementedException("Can't do this without a DataModel; should it just be a no-op?");
        }

        public void updateItemPref(long itemID, float prefDelta)
        {
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
                // RunningAverage itemAverage = averageItemPref.get(itemID);
                // if (itemAverage != null) {
                // itemAverage.changeDatum(prefDelta);
                // }
            }
            finally
            {
                buildAverageDiffsLock.ExitReadLock();
            }
        }

        public void removeItemPref(long userID, long itemID, float prefValue)
        {
            throw new NotImplementedException("Can't do this without a DataModel; should it just be a no-op?");
        }

        public common.FastIDSet getRecommendableItemIDs(long userID)
        {
            try
            {
                buildAverageDiffsLock.EnterReadLock();
                return (FastIDSet)allRecommendableItemIDs.Clone();
            }
            finally
            {
                buildAverageDiffsLock.ExitReadLock();
            }
        }

        public void refresh(IList<taste.common.Refreshable> alreadyRefreshed)
        {
            long mostRecentModification = ConvertTimeStamp(File.GetLastWriteTime(dataFile));
            if (mostRecentModification > lastModified + MIN_RELOAD_INTERVAL_MS)
            {
                log.debug("File has changed; reloading...");
                lastModified = mostRecentModification;
                buildDiffs();
            }
        }

        /// <summary>
        /// DateTime时间格式转换为Unix时间戳格式
        /// </summary>
        /// <param name="time"> DateTime时间格式</param>
        /// <returns>Unix时间戳格式</returns>
        private static long ConvertTimeStamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (long)(time - startTime).TotalMilliseconds;
        }
    }
}