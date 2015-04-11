namespace org.apache.mahout.cf.taste.impl.recommender.svd
{
    using org.apache.mahout.cf.taste;
    using org.apache.mahout.cf.taste.common;
    using org.apache.mahout.cf.taste.impl.common;
    using System.Collections.Generic;
    using System.IO;

    public class FilePersistenceStrategy : PersistenceStrategy
    {
        private string file;
        private static Logger log = LoggerFactory.getLogger(typeof(FilePersistenceStrategy));

        public FilePersistenceStrategy(string file)
        {
            this.file = file;
        }

        public Factorization load()
        {
            Factorization factorization;
            if (!File.Exists(this.file))
            {
                log.info("{0} does not yet exist, no factorization found", new object[] { this.file });
                return null;
            }
            Stream inFile = null;
            try
            {
                log.info("Reading factorization from {0}...", new object[] { this.file });
                inFile = new FileStream(this.file, FileMode.Open, FileAccess.Read);
                factorization = readBinary(inFile);
            }
            finally
            {
                inFile.Close();
            }
            return factorization;
        }

        public void maybePersist(Factorization factorization)
        {
            Stream outFile = null;
            try
            {
                log.info("Writing factorization to {0}...", new object[] { this.file });
                outFile = new FileStream(this.file, FileMode.OpenOrCreate, FileAccess.Write);
                writeBinary(factorization, outFile);
            }
            finally
            {
                outFile.Close();
            }
        }

        public static Factorization readBinary(Stream inFile)
        {
            int num4;
            int num7;
            BinaryReader reader = new BinaryReader(inFile);
            int num = reader.ReadInt32();
            int size = reader.ReadInt32();
            int num3 = reader.ReadInt32();
            FastByIDMap<int?> userIDMapping = new FastByIDMap<int?>(size);
            double[][] userFeatures = new double[size][];
            for (num4 = 0; num4 < size; num4++)
            {
                int index = reader.ReadInt32();
                long key = reader.ReadInt64();
                userFeatures[index] = new double[num];
                userIDMapping.put(key, new int?(index));
                num7 = 0;
                while (num7 < num)
                {
                    userFeatures[index][num7] = reader.ReadDouble();
                    num7++;
                }
            }
            FastByIDMap<int?> itemIDMapping = new FastByIDMap<int?>(num3);
            double[][] itemFeatures = new double[num3][];
            for (num4 = 0; num4 < num3; num4++)
            {
                int num8 = reader.ReadInt32();
                long num9 = reader.ReadInt64();
                itemFeatures[num8] = new double[num];
                itemIDMapping.put(num9, new int?(num8));
                for (num7 = 0; num7 < num; num7++)
                {
                    itemFeatures[num8][num7] = reader.ReadDouble();
                }
            }
            return new Factorization(userIDMapping, itemIDMapping, userFeatures, itemFeatures);
        }

        protected static void writeBinary(Factorization factorization, Stream outFile)
        {
            int num2;
            BinaryWriter writer = new BinaryWriter(outFile);
            writer.Write(factorization.numFeatures());
            writer.Write(factorization.numUsers());
            writer.Write(factorization.numItems());
            foreach (KeyValuePair<long, int?> pair in factorization.getUserIDMappings())
            {
                if (pair.Value.HasValue)
                {
                    long key = pair.Key;
                    writer.Write(pair.Value.Value);
                    writer.Write(key);
                    try
                    {
                        double[] numArray = factorization.getUserFeatures(key);
                        num2 = 0;
                        while (num2 < factorization.numFeatures())
                        {
                            writer.Write(numArray[num2]);
                            num2++;
                        }
                    }
                    catch (NoSuchUserException exception)
                    {
                        throw new IOException("Unable to persist factorization", exception);
                    }
                }
            }
            foreach (KeyValuePair<long, int?> pair2 in factorization.getItemIDMappings())
            {
                if (pair2.Value.HasValue)
                {
                    long num3 = pair2.Key;
                    writer.Write(pair2.Value.Value);
                    writer.Write(num3);
                    try
                    {
                        double[] numArray2 = factorization.getItemFeatures(num3);
                        for (num2 = 0; num2 < factorization.numFeatures(); num2++)
                        {
                            writer.Write(numArray2[num2]);
                        }
                    }
                    catch (NoSuchItemException exception2)
                    {
                        throw new IOException("Unable to persist factorization", exception2);
                    }
                }
            }
        }
    }
}