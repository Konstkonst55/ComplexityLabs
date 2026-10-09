namespace _4.Algorithms;

public sealed class ColumnMultiplication : ILongMultiplicationAlgorithm
{
    public string Name => "Умножение столбиком O(N^2)";

    public MultiplicationResult Multiply(string first, string second)
    {
        Validate(first);
        Validate(second);

        var firstDigits = first.Reverse().Select(c => c - '0').ToArray();
        var secondDigits = second.Reverse().Select(c => c - '0').ToArray();
        var accumulation = new long[firstDigits.Length + secondDigits.Length + 1];

        long elementaryMultiplications = 0;

        for (var j = 0; j < secondDigits.Length; j++)
        {
            for (var i = 0; i < firstDigits.Length; i++)
            {
                accumulation[i + j] += firstDigits[i] * secondDigits[j];
                elementaryMultiplications++;
            }
        }

        for (var i = 0; i < accumulation.Length - 1; i++)
        {
            var carry = accumulation[i] / 10;
            accumulation[i] %= 10;

            if (carry != 0)
            {
                accumulation[i + 1] += carry;
            }
        }

        var additionsWithShifts = elementaryMultiplications * 2 + 1;

        var highest = accumulation.Length - 1;
        while (highest > 0 && accumulation[highest] == 0)
        {
            highest--;
        }

        var result = new char[highest + 1];
        for (var i = 0; i <= highest; i++)
        {
            result[highest - i] = (char)('0' + accumulation[i]);
        }

        return new MultiplicationResult(
            new string(result),
            elementaryMultiplications,
            additionsWithShifts);
    }

    private static void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Число не должно быть пустым.");
        }

        foreach (var character in value)
        {
            if (character is < '0' or > '9')
            {
                throw new ArgumentException("Допускаются только неотрицательные целые числа.");
            }
        }
    }
}
