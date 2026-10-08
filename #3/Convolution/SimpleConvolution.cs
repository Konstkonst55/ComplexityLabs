namespace _3.Convolution;

public sealed class SimpleConvolution : IConvolutionAlgorithm
{
    public string Name => "Simple convolution O(n*m)";

    public ConvolutionResult Convolve(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second,
        double pi = Math.PI)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        ValidatePi(pi);

        if (first.Count == 0 || second.Count == 0)
        {
            throw new ArgumentException("Both sequences must contain at least one value.");
        }

        var result = new double[first.Count + second.Count - 1];
        var steps = new List<string>();
        long multiplications = 0;
        long additions = 0;

        for (var k = 0; k < result.Length; k++)
        {
            var minIndex = Math.Max(0, k - (second.Count - 1));
            var maxIndex = Math.Min(first.Count - 1, k);
            var sum = 0.0;
            var terms = new List<string>();

            for (var i = minIndex; i <= maxIndex; i++)
            {
                var j = k - i;
                var product = first[i] * second[j];
                sum += product;

                multiplications++;
                if (i > minIndex)
                {
                    additions++;
                }

                terms.Add($"{FormatValue(first[i])}*{FormatValue(second[j])}");
            }

            result[k] = sum;
            if (result.Length <= 32)
            {
                steps.Add($"c[{k}] = {string.Join(" + ", terms)} = {FormatValue(sum)}");
            }
        }

        return new ConvolutionResult(
            result,
            multiplications,
            additions,
            multiplications + additions,
            null,
            steps);
    }

    private static void ValidatePi(double pi)
    {
        if (!double.IsFinite(pi) || pi == 0)
        {
            throw new ArgumentException("Pi must be a finite non-zero value.", nameof(pi));
        }
    }

    private static string FormatValue(double value)
    {
        return value.ToString("0.###");
    }
}
