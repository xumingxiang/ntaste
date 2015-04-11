namespace org.apache.mahout.cf.taste.impl.model.file
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using org.apache.mahout.cf.taste.impl.model;
    using org.apache.mahout.cf.taste.model;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    public class FileDataModel : AbstractDataModel
    {
        private DataModel _delegate;
        private static char COMMENT_CHAR = '#';
        private string dataFile;
        public const long DEFAULT_MIN_RELOAD_INTERVAL_MS = 0xea60L;
        private static char[] DELIMIETERS = new char[] { ',', '\t' };
        private char delimiter;
        private bool hasPrefValues;
        private DateTime lastModified;
        private DateTime lastUpdateFileModified;
        private static Logger log = LoggerFactory.getLogger(typeof(FileDataModel));
        private long minReloadIntervalMS;
        private bool transpose;
        private bool uniqueUserItemCheck;
        private static DateTime unixTimestampEpochStart = new DateTime(0x7b2, 1, 1, 0, 0, 0, 0).ToLocalTime();

        public FileDataModel(string dataFile)
            : this(dataFile, false, 0xea60L, true)
        {
        }

        public FileDataModel(string dataFile, bool transpose, long minReloadIntervalMS, bool uniqueUserItemCheck = true)
        {
            this.dataFile = Path.GetFullPath(dataFile);
            if (!(File.Exists(dataFile) && !Directory.Exists(dataFile)))
            {
                throw new FileNotFoundException(dataFile);
            }
            if (new FileInfo(dataFile).Length <= 0L)
            {
                throw new ArgumentException("dataFile is empty");
            }
            log.info("Creating FileDataModel for file {0}", new object[] { dataFile });
            this.lastModified = File.GetLastWriteTime(dataFile);
            this.lastUpdateFileModified = this.readLastUpdateFileModified();

            StreamReader reader = new StreamReader(dataFile);
            string line = reader.ReadLine();
            while ((line == string.Empty) || (line[0] == COMMENT_CHAR))
            {
                line = reader.ReadLine();
            }
            reader.Close();

            /*** java Code
            //FileLineIterator iterator = new FileLineIterator(dataFile, false);
            //String firstLine = iterator.peek();
            //while (firstLine.isEmpty() || firstLine.charAt(0) == COMMENT_CHAR)
            //{
            //    iterator.next();
            //    firstLine = iterator.peek();
            //}
            //Closeables.closeQuietly(iterator);
            ***/

            this.delimiter = determineDelimiter(line);
            string[] strArray = line.Split(new char[] { this.delimiter }).ToArray<string>();
            this.hasPrefValues = (strArray.Length >= 3) && !string.IsNullOrWhiteSpace(strArray[2]);
            this.transpose = transpose;
            this.minReloadIntervalMS = minReloadIntervalMS;
            this.uniqueUserItemCheck = uniqueUserItemCheck;
            this.reload();
        }

        private void addTimestamp(long userID, long itemID, string timestampString, FastByIDMap<FastByIDMap<DateTime?>> timestamps)
        {
            if (timestampString != null)
            {
                FastByIDMap<DateTime?> map = timestamps.get(userID);
                if (map == null)
                {
                    map = new FastByIDMap<DateTime?>();
                    timestamps.put(userID, map);
                }
                DateTime time = this.readTimestampFromString(timestampString);
                map.put(itemID, new DateTime?(time));
            }
        }

        protected DataModel buildModel()
        {
            StreamReader reader;
            StreamReader reader2;
            DateTime lastWriteTime = File.GetLastWriteTime(this.dataFile);
            DateTime time2 = this.readLastUpdateFileModified();
            bool flag = (this._delegate == null) || (lastWriteTime > this.lastModified.AddMilliseconds((double)this.minReloadIntervalMS));
            DateTime lastUpdateFileModified = this.lastUpdateFileModified;
            this.lastModified = lastWriteTime;
            this.lastUpdateFileModified = time2;
            FastByIDMap<FastByIDMap<DateTime?>> timestamps = new FastByIDMap<FastByIDMap<DateTime?>>();
            if (this.hasPrefValues)
            {
                if (flag)
                {
                    FastByIDMap<List<Preference>> map2 = new FastByIDMap<List<Preference>>();
                    using (reader = new StreamReader(this.dataFile))
                    {
                        this.processFile<List<Preference>>(reader, map2, timestamps, false);
                    }
                    foreach (string str in this.findUpdateFilesAfter(lastWriteTime))
                    {
                        using (reader2 = new StreamReader(str))
                        {
                            this.processFile<List<Preference>>(reader2, map2, timestamps, false);
                        }
                    }
                    return new GenericDataModel(GenericDataModel.toDataMap(map2, true), timestamps);
                }
                FastByIDMap<PreferenceArray> map3 = ((GenericDataModel)this._delegate).getRawUserData();
                DateTime time4 = (lastUpdateFileModified > lastWriteTime) ? lastUpdateFileModified : lastWriteTime;
                foreach (string str in this.findUpdateFilesAfter(time4))
                {
                    using (reader2 = new StreamReader(str))
                    {
                        this.processFile<PreferenceArray>(reader2, map3, timestamps, true);
                    }
                }
                return new GenericDataModel(map3, timestamps);
            }
            if (flag)
            {
                FastByIDMap<FastIDSet> map4 = new FastByIDMap<FastIDSet>();
                using (reader = new StreamReader(this.dataFile))
                {
                    this.processFileWithoutID(reader, map4, timestamps);
                }
                foreach (string str in this.findUpdateFilesAfter(lastWriteTime))
                {
                    using (reader2 = new StreamReader(str))
                    {
                        this.processFileWithoutID(reader2, map4, timestamps);
                    }
                }
                return new GenericBooleanPrefDataModel(map4, timestamps);
            }
            FastByIDMap<FastIDSet> data = ((GenericBooleanPrefDataModel)this._delegate).getRawUserData();
            DateTime minimumLastModified = (lastUpdateFileModified > lastWriteTime) ? lastUpdateFileModified : lastWriteTime;
            foreach (string str in this.findUpdateFilesAfter(minimumLastModified))
            {
                using (reader = new StreamReader(str))
                {
                    this.processFileWithoutID(reader, data, timestamps);
                }
            }
            return new GenericBooleanPrefDataModel(data, timestamps);
        }

        public static char determineDelimiter(string line)
        {
            foreach (char ch in DELIMIETERS)
            {
                if (line.IndexOf(ch) >= 0)
                {
                    return ch;
                }
            }
            throw new ArgumentException("Did not find a delimiter in first line");
        }

        private IList<string> findUpdateFilesAfter(DateTime minimumLastModified)
        {
            string fileName = Path.GetFileName(this.dataFile);
            int index = fileName.IndexOf('.');
            string str2 = (index < 0) ? fileName : fileName.Substring(0, index);
            string directoryName = Path.GetDirectoryName(this.dataFile);
            IDictionary<DateTime, string> dictionary = new Dictionary<DateTime, string>();
            foreach (string str4 in Directory.GetFiles(directoryName))
            {
                string str5 = Path.GetFileName(str4);
                if ((str5.StartsWith(str2) && !str5.Equals(fileName)) && (File.GetLastWriteTime(str4) >= minimumLastModified))
                {
                    dictionary[File.GetLastWriteTime(str4)] = str4;
                }
            }
            return dictionary.Values.ToList<string>();
        }

        public string getDataFile()
        {
            return this.dataFile;
        }

        public char getDelimiter()
        {
            return this.delimiter;
        }

        public override IEnumerator<long> getItemIDs()
        {
            return this._delegate.getItemIDs();
        }

        public override FastIDSet getItemIDsFromUser(long userID)
        {
            return this._delegate.getItemIDsFromUser(userID);
        }

        public override float getMaxPreference()
        {
            return this._delegate.getMaxPreference();
        }

        public override float getMinPreference()
        {
            return this._delegate.getMinPreference();
        }

        public override int getNumItems()
        {
            return this._delegate.getNumItems();
        }

        public override int getNumUsers()
        {
            return this._delegate.getNumUsers();
        }

        public override int getNumUsersWithPreferenceFor(long itemID)
        {
            return this._delegate.getNumUsersWithPreferenceFor(itemID);
        }

        public override int getNumUsersWithPreferenceFor(long itemID1, long itemID2)
        {
            return this._delegate.getNumUsersWithPreferenceFor(itemID1, itemID2);
        }

        public override PreferenceArray getPreferencesForItem(long itemID)
        {
            return this._delegate.getPreferencesForItem(itemID);
        }

        public override PreferenceArray getPreferencesFromUser(long userID)
        {
            return this._delegate.getPreferencesFromUser(userID);
        }

        public override DateTime? getPreferenceTime(long userID, long itemID)
        {
            return this._delegate.getPreferenceTime(userID, itemID);
        }

        public override float? getPreferenceValue(long userID, long itemID)
        {
            return this._delegate.getPreferenceValue(userID, itemID);
        }

        public override IEnumerator<long> getUserIDs()
        {
            return this._delegate.getUserIDs();
        }

        public override bool hasPreferenceValues()
        {
            return this._delegate.hasPreferenceValues();
        }

        protected void processFile<T>(TextReader dataOrUpdateFileIterator, FastByIDMap<T> data, FastByIDMap<FastByIDMap<DateTime?>> timestamps, bool fromPriorData)
        {
            string str;
            log.info("Reading file info...", new object[0]);
            int num = 0;
            while ((str = dataOrUpdateFileIterator.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    this.processLine<T>(str, data, timestamps, fromPriorData);
                    if ((++num % 0xf4240) == 0)
                    {
                        log.info("Processed {0} lines", new object[] { num });
                    }
                }
            }
            log.info("Read lines: {0}", new object[] { num });
        }

        protected void processFileWithoutID(TextReader dataOrUpdateFileIterator, FastByIDMap<FastIDSet> data, FastByIDMap<FastByIDMap<DateTime?>> timestamps)
        {
            string str;
            log.info("Reading file info...", new object[0]);
            int num = 0;
            while ((str = dataOrUpdateFileIterator.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    this.processLineWithoutID(str, data, timestamps);
                    if ((++num % 0x186a0) == 0)
                    {
                        log.info("Processed {0} lines", new object[] { num });
                    }
                }
            }
            log.info("Read lines: {0}", new object[] { num });
        }

        protected void processLine<T>(string line, FastByIDMap<T> data, FastByIDMap<FastByIDMap<DateTime?>> timestamps, bool fromPriorData)
        {
            bool flag2;
            int num5;
            PreferenceArray array2;
            int num6;
            float num7;
            if ((line.Length == 0) || (line[0] == COMMENT_CHAR))
            {
                return;
            }
            string[] strArray = line.Split(new char[] { this.delimiter });
            string str = strArray[0];
            string str2 = strArray[1];
            string str3 = strArray[2];
            bool flag = strArray.Length > 3;
            string timestampString = flag ? strArray[3] : null;
            long key = this.readUserIDFromString(str);
            long itemID = this.readItemIDFromString(str2);
            if (this.transpose)
            {
                long num3 = key;
                key = itemID;
                itemID = num3;
            }
            T local = data.get(key);
            if (!fromPriorData)
            {
                IEnumerable<Preference> source = (IEnumerable<Preference>)local;
                if (flag || !string.IsNullOrWhiteSpace(str3))
                {
                    num7 = float.Parse(str3, CultureInfo.InvariantCulture);
                    flag2 = false;
                    if (this.uniqueUserItemCheck && (source != null))
                    {
                        foreach (Preference preference in source)
                        {
                            if (preference.getItemID() == itemID)
                            {
                                flag2 = true;
                                preference.setValue(num7);
                                break;
                            }
                        }
                    }
                    if (!flag2)
                    {
                        if (source == null)
                        {
                            source = new List<Preference>(5);
                            data.put(key, (T)source);
                        }
                        if (source is IList<Preference>)
                        {
                            ((IList<Preference>)source).Add(new GenericPreference(key, itemID, num7));
                        }
                    }
                    this.addTimestamp(key, itemID, timestampString, timestamps);
                    return;
                }
                if (source != null)
                {
                    IEnumerator<Preference> enumerator = ((IEnumerable<Preference>)source.ToArray<Preference>()).GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        Preference current = enumerator.Current;
                        if (current.getItemID() == itemID)
                        {
                            if (source is IList<Preference>)
                            {
                                ((IList<Preference>)local).Remove(current);
                            }
                            break;
                        }
                    }
                }
                removeTimestamp(key, itemID, timestamps);
                return;
            }
            PreferenceArray array = (PreferenceArray)local;
            if (flag || !string.IsNullOrWhiteSpace(str3))
            {
                num7 = float.Parse(str3, CultureInfo.InvariantCulture);
                flag2 = false;
                if (this.uniqueUserItemCheck && (array != null))
                {
                    for (num5 = 0; num5 < array.length(); num5++)
                    {
                        if (array.getItemID(num5) == itemID)
                        {
                            flag2 = true;
                            array.setValue(num5, num7);
                            break;
                        }
                    }
                }
            }
            else
            {
                if (array != null)
                {
                    flag2 = false;
                    int num4 = array.length();
                    for (num5 = 0; num5 < num4; num5++)
                    {
                        if (array.getItemID(num5) == itemID)
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        if (num4 == 1)
                        {
                            data.remove(key);
                        }
                        else
                        {
                            array2 = new GenericUserPreferenceArray(num4 - 1);
                            num5 = 0;
                            for (num6 = 0; num5 < num4; num6++)
                            {
                                if (array.getItemID(num5) == itemID)
                                {
                                    num6--;
                                }
                                else
                                {
                                    array2.set(num6, array.get(num5));
                                }
                                num5++;
                            }
                            data.put(key, (T)array2);
                        }
                    }
                }
                removeTimestamp(key, itemID, timestamps);
                goto Label_02F1;
            }
            if (!flag2)
            {
                if (array == null)
                {
                    array = new GenericUserPreferenceArray(1);
                }
                else
                {
                    array2 = new GenericUserPreferenceArray(array.length() + 1);
                    num5 = 0;
                    for (num6 = 1; num5 < array.length(); num6++)
                    {
                        array2.set(num6, array.get(num5));
                        num5++;
                    }
                    array = array2;
                }
                array.setUserID(0, key);
                array.setItemID(0, itemID);
                array.setValue(0, num7);
                data.put(key, (T)array);
            }
        Label_02F1:
            this.addTimestamp(key, itemID, timestampString, timestamps);
        }

        protected void processLineWithoutID(string line, FastByIDMap<FastIDSet> data, FastByIDMap<FastByIDMap<DateTime?>> timestamps)
        {
            if (!string.IsNullOrWhiteSpace(line) && (line[0] != COMMENT_CHAR))
            {
                FastIDSet set;
                string[] strArray = line.Split(new char[] { this.delimiter });
                string str = strArray[0];
                string str2 = strArray[1];
                bool flag = strArray.Length > 2;
                string str3 = flag ? strArray[2] : "";
                bool flag2 = strArray.Length > 3;
                string timestampString = flag2 ? strArray[3] : null;
                long key = this.readUserIDFromString(str);
                long num2 = this.readItemIDFromString(str2);
                if (this.transpose)
                {
                    long num3 = key;
                    key = num2;
                    num2 = num3;
                }
                if ((flag && !flag2) && string.IsNullOrEmpty(str3))
                {
                    set = data.get(key);
                    if (set != null)
                    {
                        set.remove(num2);
                    }
                    removeTimestamp(key, num2, timestamps);
                }
                else
                {
                    set = data.get(key);
                    if (set == null)
                    {
                        set = new FastIDSet(2);
                        data.put(key, set);
                    }
                    set.add(num2);
                    this.addTimestamp(key, num2, timestampString, timestamps);
                }
            }
        }

        protected long readItemIDFromString(string value)
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        private DateTime readLastUpdateFileModified()
        {
            DateTime minValue = DateTime.MinValue;
            foreach (string str in this.findUpdateFilesAfter(DateTime.MinValue))
            {
                DateTime lastWriteTime = File.GetLastWriteTime(str);
                if (lastWriteTime > minValue)
                {
                    minValue = lastWriteTime;
                }
            }
            return minValue;
        }

        protected DateTime readTimestampFromString(string value)
        {
            long num = long.Parse(value, CultureInfo.InvariantCulture);
            return unixTimestampEpochStart.AddMilliseconds((double)num);
        }

        protected long readUserIDFromString(string value)
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        public override void refresh(IList<Refreshable> alreadyRefreshed)
        {
            if ((File.GetLastWriteTime(this.dataFile) > this.lastModified.AddMilliseconds((double)this.minReloadIntervalMS)) || (this.readLastUpdateFileModified() > this.lastUpdateFileModified.AddMilliseconds((double)this.minReloadIntervalMS)))
            {
                log.debug("File has changed; reloading...", new object[0]);
                this.reload();
            }
        }

        protected void reload()
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    this._delegate = this.buildModel();
                }
                catch (IOException exception)
                {
                    log.warn("Exception while reloading", new object[] { exception });
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }

        public override void removePreference(long userID, long itemID)
        {
            this._delegate.removePreference(userID, itemID);
        }

        private static void removeTimestamp(long userID, long itemID, FastByIDMap<FastByIDMap<DateTime?>> timestamps)
        {
            FastByIDMap<DateTime?> map = timestamps.get(userID);
            if (map != null)
            {
                map.remove(itemID);
            }
        }

        public override void setPreference(long userID, long itemID, float value)
        {
            this._delegate.setPreference(userID, itemID, value);
        }

        public override string ToString()
        {
            return ("FileDataModel[dataFile:" + this.dataFile + ']');
        }
    }
}