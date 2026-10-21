namespace Tables.Tables;

public sealed class TableDefinition
{
    public TableDefinition(string title, IReadOnlyList<string> headers, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        if (headers.Count == 0)
        {
            throw new ArgumentException("В таблице должен быть хотя бы один столбец.", nameof(headers));
        }

        if (rows.Any(row => row.Count != headers.Count))
        {
            throw new ArgumentException("Количество ячеек в каждой строке должно совпадать с количеством столбцов.", nameof(rows));
        }

        Title = title;
        Headers = headers;
        Rows = rows;
    }

    public string Title { get; }
    public IReadOnlyList<string> Headers { get; }
    public IReadOnlyList<IReadOnlyList<string>> Rows { get; }
}
