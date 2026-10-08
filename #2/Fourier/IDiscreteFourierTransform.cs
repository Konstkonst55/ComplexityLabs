using System.Numerics;

namespace _2.Fourier;

public interface IDiscreteFourierTransform
{
    string Name { get; }

    TransformResult Transform(IReadOnlyList<double> source, double pi = Math.PI);

    InverseTransformResult InverseTransform(IReadOnlyList<Complex> coefficients, double pi = Math.PI);
}
