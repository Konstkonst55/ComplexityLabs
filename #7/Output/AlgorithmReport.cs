using Tables.Tables;

namespace _7.Output;

public abstract record ReportBlock;
public sealed record TextBlock(string Text) : ReportBlock;
public sealed record TableBlock(TableDefinition Table) : ReportBlock;

public sealed class AlgorithmReport
{
    private readonly List<ReportBlock> blocks = new();
    public IReadOnlyList<ReportBlock> Blocks => blocks;

    public void AddText(string text) => blocks.Add(new TextBlock(text));
    public void AddTable(TableDefinition table) => blocks.Add(new TableBlock(table));
}
