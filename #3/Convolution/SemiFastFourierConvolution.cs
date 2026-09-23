using _2.Fourier;

namespace _3.Convolution;

public sealed class SemiFastFourierConvolution : FourierConvolutionAlgorithm
{
    public SemiFastFourierConvolution()
        : base(new SemiFastDiscreteFourierTransform())
    {
    }

    public override string Name => "Convolution through semi-fast DFT O(n^3/2)";

    protected override int GetTransformLength(int firstLength, int secondLength)
    {
        var requiredLength = firstLength + secondLength;
        var size = (int)Math.Ceiling(Math.Sqrt(requiredLength));
        return size * size;
    }
}
