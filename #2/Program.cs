using _2.Fourier;

var random = new Random();
var source = new double[8];

for (var i = 0; i < source.Length; i++)
{
    source[i] = random.Next(-10, 11);
}

var transforms = new IDiscreteFourierTransform[]
{
    new DirectDiscreteFourierTransform()
};

Console.WriteLine($"Source: {FormatHelper.Format(source)}");
Console.WriteLine();

foreach (var transform in transforms)
{
    var result = transform.Transform(source, TransformDirection.Forward);

    Console.WriteLine(transform.Name);
    Console.WriteLine($"Input length: {source.Length}");
    Console.WriteLine($"Complex coefficients:");

    foreach (var step in result.Steps)
    {
        Console.WriteLine(step);
    }

    Console.WriteLine($"Total summands: {result.SummandCount}");
    Console.WriteLine($"C per summand: {result.CostPerSummand}");
    Console.WriteLine($"Total operations: {result.OperationCount}");
    Console.WriteLine();
}
