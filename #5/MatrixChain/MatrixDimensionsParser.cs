using System.Globalization;

namespace _5.MatrixChain;

public static class MatrixDimensionsParser
{
    public static int[] Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Необходимо ввести размеры хотя бы одной матрицы.", nameof(input));
        }

        var tokens = input.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 0)
        {
            throw new ArgumentException("Не удалось распознать размеры матриц.", nameof(input));
        }

        var dimensions = new List<int>();
        for (var index = 0; index < tokens.Length; index++)
        {
            var token = tokens[index].Replace('х', 'x').Replace('Х', 'x').Replace('X', 'x');
            var pair = token.Split('x', StringSplitOptions.TrimEntries);
            if (pair.Length != 2 ||
                !int.TryParse(pair[0], NumberStyles.None, CultureInfo.InvariantCulture, out var rows) ||
                !int.TryParse(pair[1], NumberStyles.None, CultureInfo.InvariantCulture, out var columns) ||
                rows <= 0 || columns <= 0)
            {
                throw new ArgumentException(
                    $"Размер матрицы #{index + 1} задан неверно: '{tokens[index]}'. Используйте формат 3x7.",
                    nameof(input));
            }

            if (index == 0)
            {
                dimensions.Add(rows);
                dimensions.Add(columns);
            }
            else
            {
                if (dimensions[^1] != rows)
                {
                    throw new ArgumentException(
                        $"Матрицы нельзя перемножить: количество столбцов M{index} ({dimensions[^1]}) не совпадает с количеством строк M{index + 1} ({rows}).",
                        nameof(input));
                }

                dimensions.Add(columns);
            }
        }

        return dimensions.ToArray();
    }
}
