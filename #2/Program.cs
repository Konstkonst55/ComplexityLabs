using _2.Fourier;

var random = new Random();
var source = new double[16];

for (var i = 0; i < source.Length; i++)
{
    source[i] = random.Next(-10, 11);
}

var transforms = new IDiscreteFourierTransform[]
{
    new SemiFastDiscreteFourierTransform()
};

Console.WriteLine($"Source: {FormatHelper.Format(source)}");
Console.WriteLine();

foreach (var transform in transforms)
{
    var forwardResult = transform.Transform(source);

    Console.WriteLine(transform.Name);
    Console.WriteLine($"Input length: {source.Length}");
    Console.WriteLine();
    Console.WriteLine("Direct DFT:");

    foreach (var step in forwardResult.Steps)
    {
        Console.WriteLine(step);
    }

    Console.WriteLine($"Total summands: {forwardResult.SummandCount}");
    Console.WriteLine($"C per summand: {forwardResult.CostPerSummand}");
    Console.WriteLine($"Total operations: {forwardResult.OperationCount}");
    Console.WriteLine();

    var inverseResult = transform.InverseTransform(forwardResult.Coefficients);

    Console.WriteLine("Inverse DFT:");

    foreach (var step in inverseResult.Steps)
    {
        Console.WriteLine(step);
    }

    Console.WriteLine($"Total summands: {inverseResult.SummandCount}");
    Console.WriteLine($"C per summand: {inverseResult.CostPerSummand}");
    Console.WriteLine($"Total operations: {inverseResult.OperationCount}");
    Console.WriteLine();
    Console.WriteLine($"Restored: {FormatHelper.Format(inverseResult.Values)}");
    Console.WriteLine();
}
