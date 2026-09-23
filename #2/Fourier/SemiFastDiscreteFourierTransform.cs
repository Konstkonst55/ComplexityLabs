using System.Numerics;

namespace _2.Fourier;

public sealed class SemiFastDiscreteFourierTransform : IDiscreteFourierTransform
{
    private const int CostPerSummand = 5;

    public string Name => "Semi-fast discrete Fourier transform O(n^3/2)";

    public TransformResult Transform(IReadOnlyList<double> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        var n = source.Count;
        var size = GetSquareRootSize(n);
        var intermediate = new Complex[n];
        var coefficients = new Complex[n];
        var steps = new List<string>();
        long summandCount = 0;
        long operationCount = 0;

        for (var k1 = 0; k1 < size; k1++)
        {
            for (var n2 = 0; n2 < size; n2++)
            {
                var sum = Complex.Zero;

                for (var n1 = 0; n1 < size; n1++)
                {
                    var sourceIndex = n1 * size + n2;
                    var angle = -2.0 * Math.PI * k1 * n1 / size;
                    var factor = Complex.FromPolarCoordinates(1.0, angle);
                    sum += source[sourceIndex] * factor;
                    summandCount++;
                    operationCount += CostPerSummand;
                }

                intermediate[k1 * size + n2] = sum;
            }
        }

        for (var k1 = 0; k1 < size; k1++)
        {
            for (var k2 = 0; k2 < size; k2++)
            {
                var sum = Complex.Zero;

                for (var n2 = 0; n2 < size; n2++)
                {
                    var intermediateIndex = k1 * size + n2;
                    var angle = -2.0 * Math.PI * k1 * n2 / n;
                    var twiddle = Complex.FromPolarCoordinates(1.0, angle);
                    var secondFactorAngle = -2.0 * Math.PI * k2 * n2 / size;
                    var secondFactor = Complex.FromPolarCoordinates(1.0, secondFactorAngle);
                    sum += intermediate[intermediateIndex] * twiddle * secondFactor;
                    summandCount++;
                    operationCount += CostPerSummand;
                }

                var coefficientIndex = k1 + size * k2;
                coefficients[coefficientIndex] = sum / n;
                steps.Add($"A[{coefficientIndex}] = {FormatHelper.Format(coefficients[coefficientIndex])}");
            }
        }

        return new TransformResult(
            coefficients,
            steps,
            summandCount,
            CostPerSummand,
            operationCount);
    }

    public InverseTransformResult InverseTransform(
        IReadOnlyList<Complex> coefficients)
    {
        ArgumentNullException.ThrowIfNull(coefficients);

        var n = coefficients.Count;
        var size = GetSquareRootSize(n);
        var intermediate = new Complex[n];
        var values = new Complex[n];
        var steps = new List<string>();
        long summandCount = 0;
        long operationCount = 0;

        for (var k1 = 0; k1 < size; k1++)
        {
            for (var n2 = 0; n2 < size; n2++)
            {
                var sum = Complex.Zero;

                for (var k2 = 0; k2 < size; k2++)
                {
                    var coefficientIndex = k1 + size * k2;
                    var angle = 2.0 * Math.PI * k1 * n2 / n;
                    var twiddle = Complex.FromPolarCoordinates(1.0, angle);
                    var secondFactorAngle = 2.0 * Math.PI * k2 * n2 / size;
                    var secondFactor = Complex.FromPolarCoordinates(1.0, secondFactorAngle);
                    sum += coefficients[coefficientIndex] * twiddle * secondFactor;
                    summandCount++;
                    operationCount += CostPerSummand;
                }

                intermediate[k1 * size + n2] = sum;
            }
        }

        for (var n1 = 0; n1 < size; n1++)
        {
            for (var n2 = 0; n2 < size; n2++)
            {
                var sum = Complex.Zero;

                for (var k1 = 0; k1 < size; k1++)
                {
                    var intermediateIndex = k1 * size + n2;
                    var angle = 2.0 * Math.PI * k1 * n1 / size;
                    var factor = Complex.FromPolarCoordinates(1.0, angle);
                    sum += intermediate[intermediateIndex] * factor;
                    summandCount++;
                    operationCount += CostPerSummand;
                }

                var valueIndex = n1 * size + n2;
                values[valueIndex] = sum;
                steps.Add($"x[{valueIndex}] = {FormatHelper.Format(values[valueIndex])}");
            }
        }

        return new InverseTransformResult(
            values,
            steps,
            summandCount,
            CostPerSummand,
            operationCount);
    }

    private static int GetSquareRootSize(int n)
    {
        if (n <= 0)
        {
            throw new ArgumentException("Input sequence must contain at least one value.", nameof(n));
        }

        var size = (int)Math.Sqrt(n);

        if (size * size != n)
        {
            throw new ArgumentException(
                "Semi-fast discrete Fourier transform requires a sequence length that is a perfect square.",
                nameof(n));
        }

        return size;
    }
}
