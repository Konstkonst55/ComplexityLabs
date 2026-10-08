using System.Numerics;

namespace _2.Fourier;

public sealed class DirectDiscreteFourierTransform : IDiscreteFourierTransform
{
    private const int CostPerSummand = 5;

    public string Name => "Direct discrete Fourier transform O(n^2)";

    public TransformResult Transform(IReadOnlyList<double> source, double pi = Math.PI)
    {
        ArgumentNullException.ThrowIfNull(source);
        ValidatePi(pi);

        var n = source.Count;
        if (n == 0)
        {
            throw new ArgumentException("Input sequence must contain at least one value.", nameof(source));
        }

        var coefficients = new Complex[n];
        var steps = new List<string>();
        long summandCount = 0;
        long operationCount = 0;

        for (var k = 0; k < n; k++)
        {
            var sum = Complex.Zero;

            for (var j = 0; j < n; j++)
            {
                var angle = -2.0 * pi * k * j / n;
                var factor = Complex.FromPolarCoordinates(1.0, angle);
                sum += source[j] * factor;
                summandCount++;
                operationCount += CostPerSummand;
            }

            coefficients[k] = sum / n;
            steps.Add($"A[{k}] = {FormatHelper.Format(coefficients[k])}");
        }

        return new TransformResult(
            coefficients,
            steps,
            summandCount,
            CostPerSummand,
            operationCount);
    }

    public InverseTransformResult InverseTransform(
        IReadOnlyList<Complex> coefficients,
        double pi = Math.PI)
    {
        ArgumentNullException.ThrowIfNull(coefficients);
        ValidatePi(pi);

        var n = coefficients.Count;
        if (n == 0)
        {
            throw new ArgumentException("Input sequence must contain at least one value.", nameof(coefficients));
        }

        var values = new Complex[n];
        var steps = new List<string>();
        long summandCount = 0;
        long operationCount = 0;

        for (var j = 0; j < n; j++)
        {
            var sum = Complex.Zero;

            for (var k = 0; k < n; k++)
            {
                var angle = 2.0 * pi * k * j / n;
                var factor = Complex.FromPolarCoordinates(1.0, angle);
                sum += coefficients[k] * factor;
                summandCount++;
                operationCount += CostPerSummand;
            }

            values[j] = sum;
            steps.Add($"x[{j}] = {FormatHelper.Format(values[j])}");
        }

        return new InverseTransformResult(
            values,
            steps,
            summandCount,
            CostPerSummand,
            operationCount);
    }

    private static void ValidatePi(double pi)
    {
        if (!double.IsFinite(pi) || pi == 0)
        {
            throw new ArgumentException("Pi must be a finite non-zero value.", nameof(pi));
        }
    }
}
