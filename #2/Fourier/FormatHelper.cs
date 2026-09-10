using System.Numerics;

namespace _2.Fourier;

public static class FormatHelper
{
    public static string Format(IReadOnlyList<double> values)
    {
        return string.Join(", ", values.Select(value => value.ToString("0.###")));
    }

    public static string Format(Complex value)
    {
        var real = Math.Abs(value.Real) < 1e-10 ? 0 : value.Real;
        var imaginary = Math.Abs(value.Imaginary) < 1e-10 ? 0 : value.Imaginary;

        if (imaginary == 0)
        {
            return real.ToString("0.###");
        }

        if (real == 0)
        {
            return $"{imaginary:0.###}i";
        }

        var sign = imaginary >= 0 ? "+" : "-";

        return $"{real:0.###} {sign} {Math.Abs(imaginary):0.###}i";
    }
}
