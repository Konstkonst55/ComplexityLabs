using System.Numerics;

namespace _2.Fourier;

public sealed class DirectDiscreteFourierTransform : IDiscreteFourierTransform
{
    private const int CostPerSummand = 5;

    public string Name => "Direct discrete Fourier transform O(n^2)";

    public TransformResult Transform(IReadOnlyList<double> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var n = source.Count;
        var coefficients = new Complex[n];
        var steps = new List<string>();

        for (var k = 0; k < n; k++)
        {
            var sum = Complex.Zero;

            for (var j = 0; j < n; j++)
            {
                var angle = -2.0 * Math.PI * k * j / n;
                var factor = Complex.FromPolarCoordinates(1.0, angle);
                sum += source[j] * factor;
            }

            coefficients[k] = sum / n;
            steps.Add($"A[{k}] = {FormatHelper.Format(coefficients[k])}");
        }

        var summandCount = (long)n * n;

        return new TransformResult(
            coefficients,
            steps,
            summandCount,
            CostPerSummand);
    }

    public InverseTransformResult InverseTransform(
        IReadOnlyList<Complex> coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);

        var n = coefficients.Count;
        var values = new Complex[n];
        var steps = new List<string>();

        for (var j = 0; j < n; j++)
        {
            var sum = Complex.Zero;

            for (var k = 0; k < n; k++)
            {
                var angle = 2.0 * Math.PI * k * j / n;
                var factor = Complex.FromPolarCoordinates(1.0, angle);
                sum += coefficients[k] * factor;
            }

            values[j] = sum;
            steps.Add($"x[{j}] = {FormatHelper.Format(values[j])}");
        }

        var summandCount = (long)n * n;

        return new InverseTransformResult(
            values,
            steps,
            summandCount,
            CostPerSummand);
    }
}
