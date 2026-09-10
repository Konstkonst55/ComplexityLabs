using System.Numerics;

namespace _2.Fourier;

public sealed class DirectDiscreteFourierTransform : IDiscreteFourierTransform
{
    private const int CostPerSummand = 5;

    public string Name => "Direct discrete Fourier transform O(n^2)";

    public TransformResult Transform(
        IReadOnlyList<double> source,
        TransformDirection direction)
    {
        ArgumentNullException.ThrowIfNull(source);

        var n = source.Count;
        var coefficients = new Complex[n];
        var steps = new List<string>();
        var sign = direction == TransformDirection.Forward ? -1.0 : 1.0;
        var normalization = direction == TransformDirection.Forward ? 1.0 / n : 1.0;

        for (var k = 0; k < n; k++)
        {
            var sum = Complex.Zero;

            for (var j = 0; j < n; j++)
            {
                var angle = sign * 2.0 * Math.PI * k * j / n;
                var factor = Complex.FromPolarCoordinates(1.0, angle);
                sum += source[j] * factor;
            }

            coefficients[k] = normalization * sum;
            steps.Add(
                $"A[{k}] = {FormatHelper.Format(coefficients[k])}");
        }

        var summandCount = (long)n * n;

        return new TransformResult(
            coefficients,
            steps,
            summandCount,
            CostPerSummand);
    }
}
