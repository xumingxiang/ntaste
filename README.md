# .Net 版本的taste

```
            string filePath = @"E:\WorkStudio\ntaste\ntaste.Test\datafile\item.csv";
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
```
