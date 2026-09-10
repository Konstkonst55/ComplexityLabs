namespace _1.Algorithms;

public interface ISortingAlgorithm
{
    string Name { get; }

    SortResult Sort(int[] source);
}
