using _2.Fourier;
using _3.Convolution;

var algorithms = new IConvolutionAlgorithm[]
{
    new SimpleConvolution(),
    new DirectFourierConvolution(),
    new SemiFastFourierConvolution()
};

var sizes = new[] { 100, 400, 1000 };
const double approximatePi = 3.1415;

PrintComplexityTable(algorithms, sizes, Math.PI);
PrintPiErrorTable(algorithms, sizes, Math.PI, approximatePi);

Console.WriteLine("Демонстрация свёртки на возрастающих массивах:");
var demoFirst = CreateAscendingArray(5);
var demoSecond = CreateAscendingArray(5);
Console.WriteLine($"a = {FormatHelper.Format(demoFirst)}");
Console.WriteLine($"b = {FormatHelper.Format(demoSecond)}");
Console.WriteLine();

foreach (var algorithm in algorithms)
{
    var result = algorithm.Convolve(demoFirst, demoSecond, Math.PI);
    Console.WriteLine(algorithm.Name);
    Console.WriteLine($"c = {FormatHelper.Format(result.Values)}");
    Console.WriteLine($"Operations: {result.OperationCount}");
    Console.WriteLine();
}

static void PrintComplexityTable(
    IReadOnlyList<IConvolutionAlgorithm> algorithms,
    IReadOnlyList<int> sizes,
    double pi)
{
    Console.WriteLine("Таблица 1. Сравнение трудоёмкости свёртки");
    Console.WriteLine("pi = Math.PI. Массивы a и b имеют одинаковую длину и заполнены по возрастанию: 1..N.");
    Console.WriteLine();
    Console.WriteLine($"{"Алгоритм",-42} {"N",8} {"Операции",16} {"Длина ДПФ",12}");
    Console.WriteLine(new string('-', 82));

    foreach (var algorithm in algorithms)
    {
        foreach (var size in sizes)
        {
            var first = CreateAscendingArray(size);
            var second = CreateAscendingArray(size);
            var result = algorithm.Convolve(first, second, pi);
            var transformLength = result.TransformLength?.ToString() ?? "-";
            Console.WriteLine($"{algorithm.Name,-42} {size,8} {result.OperationCount,16} {transformLength,12}");
        }

        Console.WriteLine();
    }
}

static void PrintPiErrorTable(
    IReadOnlyList<IConvolutionAlgorithm> algorithms,
    IReadOnlyList<int> sizes,
    double exactPi,
    double approximatePi)
{
    var referenceAlgorithm = new SimpleConvolution();

    Console.WriteLine("Таблица 2. Влияние значения pi на погрешность");
    Console.WriteLine($"Эталон: простая свёртка. Для ДПФ сравниваются pi = Math.PI и pi = {approximatePi}.");
    Console.WriteLine("Абсолютная ошибка = max|C_ДПФ - C_эталон|, относительная = max(|C_ДПФ - C_эталон| / max(|C_эталон|, 1)).");
    Console.WriteLine("Количество операций должно совпадать для обоих значений pi.");
    Console.WriteLine();
    Console.WriteLine($"{"Алгоритм",-42} {"N",6} {"pi",-14} {"Абс. ошибка",18} {"Отн. ошибка",18} {"Операции",14}");
    Console.WriteLine(new string('-', 120));

    foreach (var algorithm in algorithms.Where(item => item is not SimpleConvolution))
    {
        foreach (var size in sizes)
        {
            var first = CreateAscendingArray(size);
            var second = CreateAscendingArray(size);
            var reference = referenceAlgorithm.Convolve(first, second, exactPi).Values;

            var exactResult = algorithm.Convolve(first, second, exactPi);
            var approximateResult = algorithm.Convolve(first, second, approximatePi);

            PrintErrorRow(algorithm.Name, size, "Math.PI", reference, exactResult);
            PrintErrorRow(algorithm.Name, size, approximatePi.ToString("0.0000"), reference, approximateResult);

            var operationDifference = exactResult.OperationCount - approximateResult.OperationCount;
            Console.WriteLine($"  Разница операций: {operationDifference}");
        }

        Console.WriteLine();
    }
}

static void PrintErrorRow(
    string algorithmName,
    int size,
    string piName,
    IReadOnlyList<double> reference,
    ConvolutionResult result)
{
    var absoluteError = GetMaxAbsoluteError(reference, result.Values);
    var relativeError = GetMaxRelativeError(reference, result.Values);
    Console.WriteLine($"{algorithmName,-42} {size,6} {piName,-14} {absoluteError,18:E6} {relativeError,18:E6} {result.OperationCount,14}");
}

static double[] CreateAscendingArray(int size)
{
    var result = new double[size];
    for (var i = 0; i < size; i++)
    {
        result[i] = i + 1;
    }

    return result;
}

static double GetMaxAbsoluteError(IReadOnlyList<double> expected, IReadOnlyList<double> actual)
{
    var maxError = 0.0;

    for (var i = 0; i < expected.Count; i++)
    {
        maxError = Math.Max(maxError, Math.Abs(expected[i] - actual[i]));
    }

    return maxError;
}

static double GetMaxRelativeError(IReadOnlyList<double> expected, IReadOnlyList<double> actual)
{
    var maxError = 0.0;

    for (var i = 0; i < expected.Count; i++)
    {
        var denominator = Math.Max(Math.Abs(expected[i]), 1.0);
        var error = Math.Abs(expected[i] - actual[i]) / denominator;
        maxError = Math.Max(maxError, error);
    }

    return maxError;
}
