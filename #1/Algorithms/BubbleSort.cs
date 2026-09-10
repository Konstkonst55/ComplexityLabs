namespace _1.Algorithms;

public sealed class BubbleSort : ISortingAlgorithm
{
    public string Name => "Bubble sort";

    public SortResult Sort(int[] source)
    {
        var values = (int[])source.Clone();
        var steps = new List<string>();
        long movements = 0;
        long comparisons = 0;

        for (var i = 0; i < values.Length - 1; i++)
        {
            var swapped = false;

            for (var j = 0; j < values.Length - i - 1; j++)
            {
                comparisons++;

                if (values[j] <= values[j + 1])
                {
                    continue;
                }

                var left = values[j];
                var right = values[j + 1];

                (values[j], values[j + 1]) = (values[j + 1], values[j]);
                movements += 3;
                swapped = true;

                steps.Add(
                    $"Swap {left} and {right}: {string.Join(", ", values)}");
            }

            steps.Add($"Pass {i + 1}: {string.Join(", ", values)}");

            if (!swapped)
            {
                steps.Add("No swaps on this pass. Sorting stopped.");
                break;
            }
        }

        return new SortResult(values, movements, comparisons, steps);
    }
}
