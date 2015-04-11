using org.apache.mahout.cf.taste.impl.common;
using org.apache.mahout.cf.taste.model;
using org.apache.mahout.cf.taste.recommender;
using org.apache.mahout.cf.taste.similarity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace org.apache.mahout.cf.taste.impl.recommender.knn
{
    public class KnnItemBasedRecommender : GenericItemBasedRecommender
    {
        private static double BETA = 500.0;
        private Optimizer optimizer;
        private int neighborhoodSize;

        public KnnItemBasedRecommender(DataModel dataModel,
                                       ItemSimilarity similarity,
                                       Optimizer optimizer,
                                       CandidateItemsStrategy candidateItemsStrategy,
                                       MostSimilarItemsCandidateItemsStrategy mostSimilarItemsCandidateItemsStrategy,
                                       int neighborhoodSize) :
            base(dataModel, similarity, candidateItemsStrategy, mostSimilarItemsCandidateItemsStrategy)
        {
            this.optimizer = optimizer;
            this.neighborhoodSize = neighborhoodSize;
        }

        public KnnItemBasedRecommender(DataModel dataModel, ItemSimilarity similarity, Optimizer optimizer, int neighborhoodSize) :
            this(dataModel, similarity, optimizer, getDefaultCandidateItemsStrategy(), getDefaultMostSimilarItemsCandidateItemsStrategy(), neighborhoodSize)
        {
        }

        private List<RecommendedItem> mostSimilarItems(long itemID,
                                                       IEnumerator<long> possibleItemIDs,
                                                       int howMany,
                                                       Rescorer<Tuple<long, long>> rescorer)
        {
            TopItems.Estimator<long> estimator = new MostSimilarEstimator(itemID, getSimilarity(), rescorer);
            return TopItems.getTopItems(howMany, possibleItemIDs, null, estimator);
        }

        private double[] getInterpolations(long itemID, long[] itemNeighborhood, List<long> usersRatedNeighborhood)
        {
            int length = 0;
            for (int m = 0; m < itemNeighborhood.Length; m++)
            {
                if (itemNeighborhood[m] == itemID)
                {
                    itemNeighborhood[m] = -1;
                    length = itemNeighborhood.Length - 1;
                    break;
                }
            }

            int k = length;
            double[][] aMatrix = new double[k][];
            double[] b = new double[k];
            int i = 0;

            DataModel dataModel = getDataModel();

            int numUsers = usersRatedNeighborhood.Count;
            foreach (long iitem in itemNeighborhood)
            {
                if (iitem == -1)
                {
                    break;
                }
                int j = 0;
                double value = 0.0;
                aMatrix[i] = new double[k];
                foreach (long jitem in itemNeighborhood)
                {
                    if (jitem == -1)
                    {
                        continue;
                    }
                    foreach (long user in usersRatedNeighborhood)
                    {
                        float? prefVJ = dataModel.getPreferenceValue(user, iitem);
                        float? prefVK = dataModel.getPreferenceValue(user, jitem);
                        value += prefVJ.Value * prefVK.Value;
                    }
                    aMatrix[i][j] = value / numUsers;
                    j++;
                }
                i++;
            }

            i = 0;
            foreach (long jitem in itemNeighborhood)
            {
                if (jitem == -1)
                {
                    break;
                }
                double value = 0.0;
                foreach (long user in usersRatedNeighborhood)
                {
                    float? prefVJ = dataModel.getPreferenceValue(user, jitem);
                    float? prefVI = dataModel.getPreferenceValue(user, itemID);
                    value += prefVJ.Value * prefVI.Value;
                }
                b[i] = value / numUsers;
                i++;
            }

            // Find the larger diagonal and calculate the average
            double avgDiagonal = 0.0;
            if (k > 1)
            {
                double diagonalA = 0.0;
                for (i = 0; i < k; i++)
                {
                    diagonalA += aMatrix[i][i];
                }
                double diagonalB = 0.0;
                for (i = k - 1; i >= 0; i--)
                {
                    for (int j = 0; j < k; j++)
                    {
                        diagonalB += aMatrix[i--][j];
                    }
                }
                avgDiagonal = Math.Max(diagonalA, diagonalB) / k;
            }
            // Calculate the average of non-diagonal values
            double avgMatrixA = 0.0;
            double avgVectorB = 0.0;
            for (i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    if (i != j || k <= 1)
                    {
                        avgMatrixA += aMatrix[i][j];
                    }
                }
                avgVectorB += b[i];
            }
            if (k > 1)
            {
                avgMatrixA /= k * k - k;
            }
            avgVectorB /= k;

            double numUsersPlusBeta = numUsers + BETA;
            for (i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                {
                    double average;
                    if (i == j && k > 1)
                    {
                        average = avgDiagonal;
                    }
                    else
                    {
                        average = avgMatrixA;
                    }
                    aMatrix[i][j] = (numUsers * aMatrix[i][j] + BETA * average) / numUsersPlusBeta;
                }
                b[i] = (numUsers * b[i] + BETA * avgVectorB) / numUsersPlusBeta;
            }

            return optimizer.optimize(aMatrix, b);
        }

        protected override float doEstimatePreference(long theUserID, PreferenceArray preferencesFromUser, long itemID)
        {
            DataModel dataModel = getDataModel();
            int size = preferencesFromUser.length();
            FastIDSet possibleItemIDs = new FastIDSet(size);
            for (int i = 0; i < size; i++)
            {
                possibleItemIDs.add(preferencesFromUser.getItemID(i));
            }
            possibleItemIDs.remove(itemID);

            List<RecommendedItem> mostSimilar = mostSimilarItems(itemID, possibleItemIDs.GetEnumerator(), neighborhoodSize, null);
            long[] theNeighborhood = new long[mostSimilar.Count() + 1];
            theNeighborhood[0] = -1;

            List<long> usersRatedNeighborhood = new List<long>();
            int nOffset = 0;
            foreach (RecommendedItem rec in mostSimilar)
            {
                theNeighborhood[nOffset++] = rec.getItemID();
            }

            if (mostSimilar.Count != 0)
            {
                theNeighborhood[mostSimilar.Count] = itemID;
                for (int i = 0; i < theNeighborhood.Length; i++)
                {
                    PreferenceArray usersNeighborhood = dataModel.getPreferencesForItem(theNeighborhood[i]);
                    int size1 = usersRatedNeighborhood.Count == 0 ? usersNeighborhood.length() : usersRatedNeighborhood.Count;
                    for (int j = 0; j < size1; j++)
                    {
                        if (i == 0)
                        {
                            usersRatedNeighborhood.Add(usersNeighborhood.getUserID(j));
                        }
                        else
                        {
                            if (j >= usersRatedNeighborhood.Count)
                            {
                                break;
                            }
                            long index = usersRatedNeighborhood[j];
                            if (!usersNeighborhood.hasPrefWithUserID(index) || index == theUserID)
                            {
                                usersRatedNeighborhood.Remove(index);
                                j--;
                            }
                        }
                    }
                }
            }

            double[] weights = null;
            if (mostSimilar.Count != 0)
            {
                weights = getInterpolations(itemID, theNeighborhood, usersRatedNeighborhood);
            }

            int n = 0;
            double preference = 0.0;
            double totalSimilarity = 0.0;
            foreach (long jitem in theNeighborhood)
            {
                float? pref = dataModel.getPreferenceValue(theUserID, jitem);

                if (pref != null)
                {
                    double weight = weights[n];
                    preference += pref.Value * weight;
                    totalSimilarity += weight;
                }
                n++;
            }
            return totalSimilarity == 0.0 ? float.NaN : (float)(preference / totalSimilarity);
        }
    }
}