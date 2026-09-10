namespace _2.Fourier;

public interface IDiscreteFourierTransform
{
    string Name { get; }

    TransformResult Transform(
        IReadOnlyList<double> source,
        TransformDirection direction);
}
