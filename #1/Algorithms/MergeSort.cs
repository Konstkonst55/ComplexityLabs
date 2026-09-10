namespace _1.Algorithms;

public sealed class MergeSort : ISortingAlgorithm
{
    public string Name => "Merge sort";

    public SortResult Sort(int[] source)
    {
        var values = (int[])source.Clone();
        var buffer = new int[values.Length];
        var steps = new List<string>();
        long movements = 0;
        long comparisons = 0;

        if (values.Length > 1)
        {
            SortRange(
                values,
                buffer,
                0,
                values.Length - 1,
                steps,
                ref movements,
                ref comparisons);
        }

        return new SortResult(values, movements, comparisons, steps);
    }

    private static void SortRange(
        int[] values,
        int[] buffer,
        int left,
        int right,
        List<string> steps,
        ref long movements,
        ref long comparisons)
    {
        if (left >= right)
        {
            return;
        }

        var middle = left + (right - left) / 2;

        SortRange(
            values,
            buffer,
            left,
            middle,
            steps,
            ref movements,
            ref comparisons);

        SortRange(
            values,
            buffer,
            middle + 1,
            right,
            steps,
            ref movements,
            ref comparisons);

        Merge(
            values,
            buffer,
            left,
            middle,
            right,
            steps,
            ref movements,
            ref comparisons);
    }

    private static void Merge(
        int[] values,
        int[] buffer,
        int left,
        int middle,
        int right,
        List<string> steps,
        ref long movements,
        ref long comparisons)
    {
        var leftPointer = left;
        var rightPointer = middle + 1;
        var bufferPointer = left;

        var leftPart = string.Join(", ", values[left..(middle + 1)]);
        var rightPart = string.Join(", ", values[(middle + 1)..(right + 1)]);

        steps.Add($"Merge pair: [{leftPart}] + [{rightPart}]");

        while (leftPointer <= middle && rightPointer <= right)
        {
            comparisons++;

            if (values[leftPointer] <= values[rightPointer])
            {
                buffer[bufferPointer++] = values[leftPointer++];
            }
            else
            {
                buffer[bufferPointer++] = values[rightPointer++];
            }

            movements++;
        }

        while (leftPointer <= middle)
        {
            buffer[bufferPointer++] = values[leftPointer++];
            movements++;
        }

        while (rightPointer <= right)
        {
            buffer[bufferPointer++] = values[rightPointer++];
            movements++;
        }

        for (var i = left; i <= right; i++)
        {
            values[i] = buffer[i];
            movements++;
        }

        steps.Add(
            $"After merge: {string.Join(", ", values[left..(right + 1)])}");
    }
}
