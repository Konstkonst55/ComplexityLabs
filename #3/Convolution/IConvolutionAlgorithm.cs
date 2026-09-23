namespace _3.Convolution;

public interface IConvolutionAlgorithm
{
    string Name { get; }

    ConvolutionResult Convolve(
        IReadOnlyList<double> first,
        IReadOnlyList<double> second);
}
