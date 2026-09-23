using _2.Fourier;
using _3.Convolution;

var random = new Random();
var first = new double[8];
var second = new double[8];

for (var i = 0; i < first.Length; i++)
{
    first[i] = random.Next(-10, 11);
    second[i] = random.Next(-10, 11);
}

var algorithms = new IConvolutionAlgorithm[]
{
    new SimpleConvolution(),
    new DirectFourierConvolution(),
    new SemiFastFourierConvolution()
};

Console.WriteLine($"a = {FormatHelper.Format(first)}");
Console.WriteLine($"b = {FormatHelper.Format(second)}");
Console.WriteLine();

foreach (var algorithm in algorithms)
{
    var result = algorithm.Convolve(first, second);

    Console.WriteLine(algorithm.Name);
    Console.WriteLine();

    foreach (var step in result.Steps)
    {
        Console.WriteLine(step);
    }

    Console.WriteLine();
    Console.WriteLine($"Convolution: {FormatHelper.Format(result.Values)}");
    Console.WriteLine($"Multiplications: {result.Multiplications}");
    Console.WriteLine($"Additions: {result.Additions}");
    if (result.TransformLength.HasValue)
    {
        Console.WriteLine($"Transform length: {result.TransformLength.Value}");
    }

    Console.WriteLine($"Total operations: {result.OperationCount}");
    Console.WriteLine();
}
