namespace _1.Algorithms;

public sealed class SelectionSort : ISortingAlgorithm
{
    public string Name => "Selection sort";

    public SortResult Sort(int[] source)
    {
        var values = (int[])source.Clone();
        var steps = new List<string>();
        long movements = 0;
        long comparisons = 0;

        for (var i = 0; i < values.Length - 1; i++)
        {
            var minIndex = i;

            for (var j = i + 1; j < values.Length; j++)
            {
                comparisons++;

                if (values[j] < values[minIndex])
                {
                    minIndex = j;
                }
            }

            if (minIndex != i)
            {
                var selected = values[minIndex];
                var current = values[i];

                (values[i], values[minIndex]) = (values[minIndex], values[i]);
                movements += 3;

                steps.Add(
                    $"Pass {i + 1}: selected {selected}, swap with {current}: {string.Join(", ", values)}");
            }
            else
            {
                steps.Add(
                    $"Pass {i + 1}: minimum {values[i]} is already in position: {string.Join(", ", values)}");
            }
        }

        return new SortResult(values, movements, comparisons, steps);
    }
}
