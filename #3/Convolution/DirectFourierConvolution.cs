using _2.Fourier;

namespace _3.Convolution;

public sealed class DirectFourierConvolution : FourierConvolutionAlgorithm
{
    public DirectFourierConvolution()
        : base(new DirectDiscreteFourierTransform())
    {
    }

    public override string Name => "Convolution through direct DFT O(n^2)";
}
