namespace _8.Models;

/// <summary>
/// Взвешенный граф. Отсутствующее ребро представляется отсутствием записи в матрице весов.
/// Для неориентированного графа каждое ребро автоматически доступно в обоих направлениях.
/// </summary>
public sealed class WeightedGraph
{
    private readonly int[] _vertices;
    private readonly IReadOnlyList<WeightedEdge> _edges;
    private readonly Dictionary<(int From, int To), long> _weights;
    private readonly HashSet<int> _vertexSet;

    public WeightedGraph(IEnumerable<int> vertices, IEnumerable<WeightedEdge> edges, bool isDirected)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(edges);

        _vertices = vertices.Distinct().OrderBy(vertex => vertex).ToArray();
        if (_vertices.Length == 0)
        {
            throw new ArgumentException("Граф должен содержать хотя бы одну вершину.", nameof(vertices));
        }

        _vertexSet = _vertices.ToHashSet();
        IsDirected = isDirected;
        _weights = new Dictionary<(int From, int To), long>();

        foreach (var edge in edges)
        {
            if (!_vertexSet.Contains(edge.From) || !_vertexSet.Contains(edge.To))
            {
                throw new ArgumentException($"Ребро {edge} содержит вершину, отсутствующую в графе.", nameof(edges));
            }

            if (edge.From == edge.To)
            {
                continue;
            }

            if (edge.Weight < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(edges), "Веса рёбер должны быть неотрицательными.");
            }

            SetMinimumWeight(edge.From, edge.To, edge.Weight);
            if (!IsDirected)
            {
                SetMinimumWeight(edge.To, edge.From, edge.Weight);
            }
        }

        _edges = BuildEdgeList();
    }

    public IReadOnlyList<int> Vertices => _vertices;

    public IReadOnlyList<WeightedEdge> Edges => _edges;

    public bool IsDirected { get; }

    public int VertexCount => _vertices.Length;

    public long? GetWeight(int from, int to)
    {
        return _weights.TryGetValue((from, to), out var weight) ? weight : null;
    }

    public bool ContainsVertex(int vertex) => _vertexSet.Contains(vertex);

    public long?[,] ToAdjacencyMatrix()
    {
        var matrix = new long?[VertexCount, VertexCount];

        for (var i = 0; i < VertexCount; i++)
        {
            for (var j = 0; j < VertexCount; j++)
            {
                matrix[i, j] = GetWeight(_vertices[i], _vertices[j]);
            }
        }

        return matrix;
    }

    public static WeightedGraph FromAdjacencyMatrix(
        long?[,] matrix,
        int firstVertex = 0,
        bool isDirected = false)
    {
        ArgumentNullException.ThrowIfNull(matrix);

        var rowCount = matrix.GetLength(0);
        var columnCount = matrix.GetLength(1);
        if (rowCount == 0 || rowCount != columnCount)
        {
            throw new ArgumentException("Матрица смежности должна быть непустой квадратной матрицей.", nameof(matrix));
        }

        if (firstVertex > int.MaxValue - (rowCount - 1))
        {
            throw new ArgumentOutOfRangeException(nameof(firstVertex));
        }

        if (!isDirected)
        {
            for (var i = 0; i < rowCount; i++)
            {
                for (var j = i + 1; j < columnCount; j++)
                {
                    if (matrix[i, j] != matrix[j, i])
                    {
                        throw new ArgumentException(
                            $"Матрица неориентированного графа должна быть симметричной: " +
                            $"элементы [{i},{j}] и [{j},{i}] различаются.",
                            nameof(matrix));
                    }
                }
            }
        }

        var vertices = Enumerable.Range(firstVertex, rowCount).ToArray();
        var edges = new List<WeightedEdge>();

        for (var i = 0; i < rowCount; i++)
        {
            for (var j = 0; j < columnCount; j++)
            {
                if (i == j || matrix[i, j] is not long weight)
                {
                    continue;
                }

                if (weight < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(matrix), "Веса рёбер должны быть неотрицательными.");
                }

                if (isDirected || i < j)
                {
                    edges.Add(new WeightedEdge(vertices[i], vertices[j], weight));
                }
            }
        }

        return new WeightedGraph(vertices, edges, isDirected);
    }

    private void SetMinimumWeight(int from, int to, long weight)
    {
        var key = (from, to);
        if (!_weights.TryGetValue(key, out var current) || weight < current)
        {
            _weights[key] = weight;
        }
    }

    private IReadOnlyList<WeightedEdge> BuildEdgeList()
    {
        var edges = new List<WeightedEdge>();

        foreach (var (key, weight) in _weights)
        {
            if (IsDirected || key.From < key.To)
            {
                edges.Add(new WeightedEdge(key.From, key.To, weight));
            }
        }

        return edges
            .OrderBy(edge => edge.From)
            .ThenBy(edge => edge.To)
            .ToArray();
    }
}
