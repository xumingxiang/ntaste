using org.apache.mahout.cf.taste.impl.model;
using org.apache.mahout.cf.taste.impl.model.file;
using org.apache.mahout.cf.taste.impl.neighborhood;
using org.apache.mahout.cf.taste.impl.recommender;
using org.apache.mahout.cf.taste.impl.recommender.knn;
using org.apache.mahout.cf.taste.impl.recommender.slopeone;
using org.apache.mahout.cf.taste.impl.similarity;
using org.apache.mahout.cf.taste.model;
using org.apache.mahout.cf.taste.similarity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ntaste.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            SlopeOneRecommenderTest();
            Console.WriteLine();
            GenericUserBasedRecommenderTest();
            Console.WriteLine();
            KnnItemBasedRecommenderTest();
            Console.WriteLine();
            GenericItemBasedRecommenderTestByTanimotoCoefficientSimilarity();
            Console.ReadLine();
        }

        static string filePath = @"E:\WorkStudio\ntaste\ntaste.Test\datafile\item.csv";
        static void SlopeOneRecommenderTest()
        {
            Console.WriteLine("SlopeOneRecommenderTest");
            var model = new FileDataModel(filePath);


            var recommender = new SlopeOneRecommender(model);
            var ids = model.getUserIDs();
            while (ids.MoveNext())
            {
                var userId = ids.Current;
                var recommendedItems = recommender.recommend(userId, 5);

                Console.Write("uid:" + userId);
                foreach (var ritem in recommendedItems)
                {
                    Console.Write("(" + ritem.getItemID() + "," + ritem.getValue() + ")");
                }
                Console.WriteLine();
            }
        }

        static void GenericUserBasedRecommenderTest()
        {
            Console.WriteLine("GenericUserBasedRecommenderTest");
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var model = new FileDataModel(filePath);
            var similarity = new PearsonCorrelationSimilarity(model);
            var neighborhood = new NearestNUserNeighborhood(4, similarity, model);
            var recommender = new GenericUserBasedRecommender(model, neighborhood, similarity);
            var iter = model.getUserIDs();
            while (iter.MoveNext())
            {
                var userId = iter.Current;
                var recommendedItems = recommender.recommend(userId, 5);
                Console.Write("uid:" + userId);
                foreach (var ritem in recommendedItems)
                {
                    Console.Write("(" + ritem.getItemID() + "," + ritem.getValue() + ")");
                }
                Console.WriteLine();
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

        }

        static void KnnItemBasedRecommenderTest()
        {
            Console.WriteLine("KnnItemBasedRecommenderTest");
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var model = new FileDataModel(filePath);

            ItemSimilarity similarity = new LogLikelihoodSimilarity(model);

            Optimizer optimizer = new ConjugateGradientOptimizer();

            var recommender = new KnnItemBasedRecommender(model, similarity, optimizer, 10);

            var iter = model.getUserIDs();

            while (iter.MoveNext())
            {
                var userId = iter.Current;
                var recommendedItems = recommender.recommend(userId, 5);

                Console.Write("uid:" + userId);
                foreach (var ritem in recommendedItems)
                {
                    Console.Write("(" + ritem.getItemID() + "," + ritem.getValue() + ")");
                }
                Console.WriteLine();
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

        }

        static void GenericItemBasedRecommenderTestByTanimotoCoefficientSimilarity()
        {
            Console.WriteLine("GenericItemBasedRecommenderTestByTanimotoCoefficientSimilarity");
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            var model = new FileDataModel(filePath);

            ItemSimilarity similarity = new TanimotoCoefficientSimilarity(model);

            var recommender = new GenericItemBasedRecommender(model, similarity);

            var iter = model.getUserIDs();

            while (iter.MoveNext())
            {
                var userId = iter.Current;
                var recommendedItems = recommender.recommend(userId, 5);

                Console.Write("uid:" + userId);
                foreach (var ritem in recommendedItems)
                {
                    Console.Write("(" + ritem.getItemID() + "," + ritem.getValue() + ")");
                }
                Console.WriteLine();
            }
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

        }

    }


}
