namespace Tables.Tables;

public sealed class TableBuilder
{
    private readonly string title;
    private readonly List<string> headers = new();
    private readonly List<IReadOnlyList<string>> rows = new();

    public TableBuilder(string title)
    {
        this.title = title;
    }

    public TableBuilder AddColumn(string header)
    {
        headers.Add(header);
        return this;
    }

    public TableBuilder AddRow(params object?[] cells)
    {
        if (headers.Count == 0)
        {
            throw new InvalidOperationException("Сначала добавьте столбцы.");
        }

        if (cells.Length != headers.Count)
        {
            throw new ArgumentException("Количество ячеек не совпадает с количеством столбцов.", nameof(cells));
        }

        rows.Add(cells.Select(cell => cell?.ToString() ?? string.Empty).ToArray());
        return this;
    }

    public TableDefinition Build()
    {
        return new TableDefinition(title, headers.ToArray(), rows.ToArray());
    }
}
