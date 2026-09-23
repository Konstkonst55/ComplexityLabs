using System.Numerics;
using _2.Fourier;

namespace _3.Convolution;

public abstract class FourierConvolutionAlgorithm : IConvolutionAlgorithm
{
    private readonly IDiscreteFourierTransform transform;

    protected FourierConvolutionAlgorithm(IDiscreteFourierTransform transform)
    {
        this.transform = transform;
    }

    public abstract string Name { get; }

    protected virtual int GetTransformLength(int firstLength, int secondLength)
    {
        return firstLength + secondLength;
    }

    public ConvolutionResult Convolve(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        if (first.Count == 0 || second.Count == 0)
        {
            throw new ArgumentException("Both sequences must contain at least one value.");
        }

        var requiredLength = first.Count + second.Count - 1;
        var transformLength = GetTransformLength(first.Count, second.Count);

        if (transformLength < requiredLength)
        {
            throw new InvalidOperationException(
                "The transform length must be at least the linear convolution length.");
        }

        var paddedFirst = new double[transformLength];
        var paddedSecond = new double[transformLength];
        for (var i = 0; i < first.Count; i++)
        {
            paddedFirst[i] = first[i];
        }

        for (var i = 0; i < second.Count; i++)
        {
            paddedSecond[i] = second[i];
        }

        var firstTransform = transform.Transform(paddedFirst);
        var secondTransform = transform.Transform(paddedSecond);
        var products = new Complex[transformLength];
        var steps = new List<string>();

        for (var i = 0; i < transformLength; i++)
        {
            products[i] = firstTransform.Coefficients[i]
                * secondTransform.Coefficients[i]
                * transformLength;
        }

        steps.Add($"Transform length: {transformLength}");
        steps.Add("F(a):");
        steps.AddRange(firstTransform.Steps);
        steps.Add("F(b):");
        steps.AddRange(secondTransform.Steps);
        steps.Add($"{transformLength} * F(a) * F(b):");

        for (var i = 0; i < products.Length; i++)
        {
            steps.Add($"C[{i}] = {FormatHelper.Format(products[i])}");
        }

        var inverseResult = transform.InverseTransform(products);
        var values = new double[requiredLength];

        for (var i = 0; i < requiredLength; i++)
        {
            values[i] = NormalizeValue(inverseResult.Values[i].Real);
        }

        steps.Add("F^-1(C):");
        for (var i = 0; i < requiredLength; i++)
        {
            steps.Add($"c[{i}] = {FormatValue(values[i])}");
        }

        var transformOperations =
            firstTransform.OperationCount
            + secondTransform.OperationCount
            + inverseResult.OperationCount;

        var pointwiseOperations = transformLength;

        return new ConvolutionResult(
            values,
            pointwiseOperations,
            0,
            transformOperations + pointwiseOperations,
            transformLength,
            steps);
    }

    private static double NormalizeValue(double value)
    {
        return Math.Abs(value) < 1e-9 ? 0 : value;
    }

    private static string FormatValue(double value)
    {
        return value.ToString("0.###");
    }
}
