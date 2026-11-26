using Tables.Tables;

namespace _8.Output;

public abstract record ReportBlock;

public sealed record TextBlock(string Text) : ReportBlock;

public sealed record TableBlock(TableDefinition Table) : ReportBlock;

public sealed class AlgorithmReport
{
    private readonly List<ReportBlock> _blocks = [];

    public IReadOnlyList<ReportBlock> Blocks => _blocks;

    public void AddText(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        _blocks.Add(new TextBlock(text));
    }

    public void AddTable(TableDefinition table)
    {
        ArgumentNullException.ThrowIfNull(table);
        _blocks.Add(new TableBlock(table));
    }
}
