namespace org.apache.mahout.cf.taste.similarity.precompute
{
    public interface SimilarItemsWriter
    {
        void add(SimilarItems similarItems);

        void open();
    }
}