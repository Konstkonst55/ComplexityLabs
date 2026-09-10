using _1.Algorithms;

var random = new Random();
var source = new int[10];

for (var i = 0; i < source.Length; i++)
{
    source[i] = random.Next(1, 101);
}

Console.WriteLine($"Source: {string.Join(", ", source)}");
Console.WriteLine();

var algorithms = new ISortingAlgorithm[]
{
    new BubbleSort(),
    new SelectionSort(),
    new MergeSort()
};

foreach (var algorithm in algorithms)
{
    var result = algorithm.Sort(source);

    Console.WriteLine(algorithm.Name);

    foreach (var step in result.Steps)
    {
        Console.WriteLine(step);
    }

    Console.WriteLine($"Sorted: {string.Join(", ", result.Values)}");
    Console.WriteLine($"m = {result.Movements}, c = {result.Comparisons}");
    Console.WriteLine();
}
