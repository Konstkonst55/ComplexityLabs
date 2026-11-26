using _8.Models;
using _8.Output;

namespace _8.Algorithms;

/// <summary>
/// Одностороннее ветвление с откатом: DFS, локальный выбор ребёнка с минимальной
/// нижней оценкой и стек перспективных отложенных ветвей.
/// </summary>
public sealed class BranchAndBoundTspAlgorithm
{
    private readonly LowerBoundCalculator _boundCalculator = new();

    public TspResult Execute(WeightedGraph graph, int startVertex, TspOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(graph);
        options ??= new TspOptions();

        if (graph.VertexCount < 2)
        {
            throw new ArgumentException("Для задачи коммивояжера граф должен содержать не менее двух вершин.", nameof(graph));
        }

        if (!graph.ContainsVertex(startVertex))
        {
            throw new ArgumentException($"Стартовая вершина {startVertex} отсутствует в графе.", nameof(startVertex));
        }

        var trace = options.EnableTrace ? new AlgorithmReport() : null;
        var state = new SearchState();

        FindInitialRecord(graph, startVertex, state, trace);

        var rootPath = new List<int> { startVertex };
        var rootBound = CalculateBound(graph, rootPath, 0, state, trace);
        var root = new SearchNode(
            rootPath,
            0,
            rootBound.LowerBound,
            rootBound.AlphaSum,
            rootBound.BetaSum,
            Serial: 0);

        if (root.LowerBound >= LowerBoundCalculator.Infinity)
        {
            trace?.AddText("Исходная оценка H(D) = inf: продолжение полного цикла невозможно.");
            AddFinalStatistics(trace, state);
            return BuildResult(state, trace);
        }

        var pending = new List<SearchNode> { root };
        IReadOnlyList<int>? previousPath = null;
        var serial = 0;
        var step = 0;

        trace?.AddText(
            $"Нижняя оценка исходного множества H={root.LowerBound}. Вызов оценки H(D) #{state.BoundCalls} (корневое множество).\n" +
            $"Начальный рекорд r={FormatValue(state.Best)}. Отсечение: H(D) >= r.\n" +
            "Обход: сначала спуск до полного цикла или тупика, затем возврат к отложенному пути.\n" +
            "Продолжения печатаются по номерам вершин; следующий ребёнок выбирается по минимальной H(D).\n" +
            "Отложенные перспективные ветви хранятся в стеке (LIFO).");

        while (pending.Count > 0)
        {
            var smallestPendingBound = pending.Min(node => node.LowerBound);
            if (smallestPendingBound >= state.Best)
            {
                state.PrunedBranches += pending.Count;
                trace?.AddText(
                    $"СТОП: все отложенные оценки H(D) >= r={FormatValue(state.Best)} " +
                    $"(минимальная H={FormatValue(smallestPendingBound)}). " +
                    $"Отсечено ветвей: {pending.Count}.");
                break;
            }

            var current = pending[^1];
            pending.RemoveAt(pending.Count - 1);

            if (current.LowerBound >= state.Best)
            {
                state.PrunedBranches++;
                trace?.AddText(
                    $"Отложенная ветвь [{FormatPath(current.Path)}] H={FormatValue(current.LowerBound)} " +
                    $">= новый r={FormatValue(state.Best)} - отсечена без раскрытия.");
                continue;
            }

            if (previousPath is not null && !IsPrefix(previousPath, current.Path))
            {
                var commonLength = CommonPrefixLength(previousPath, current.Path);
                state.Returns++;
                var commonAncestor = previousPath[commonLength - 1];
                trace?.AddText(
                    $"\nПОДЪЁМ {state.Returns} ПОСЛЕ ДОСТИЖЕНИЯ ЛИСТА ИЛИ ТУПИКА\n" +
                    $"Завершили ветвь: {FormatPath(previousPath)}.\n" +
                    $"Поднялись к общему предку: {commonAncestor}.\n" +
                    $"Берём отложенную ветвь: {FormatPath(current.Path)}, " +
                    $"H={FormatValue(current.LowerBound)} < r={FormatValue(state.Best)}.");
            }

            previousPath = current.Path;
            step++;

            trace?.AddText(
                $"\nШАГ {step}: [{FormatPath(current.Path)}] " +
                $"H={FormatValue(current.LowerBound)} < r={FormatValue(state.Best)}" +
                (current.Path.Count == graph.VertexCount ? " - достигнуто дно дерева." : " - проверяем продолжения."));

            if (current.Path.Count == graph.VertexCount)
            {
                var oldRecord = state.Best;
                state.Best = current.LowerBound;
                state.BestPath = current.Path.Append(startVertex).ToList();

                trace?.AddText(
                    $"Найден полный цикл {FormatPath(state.BestPath)}. " +
                    $"Рекорд: {FormatValue(oldRecord)} -> {state.Best}.");
                TracePendingAfterLeaf(trace, pending, current.Path, state.Best);
                continue;
            }

            state.ExpandedNodes++;
            var boundCallsBeforeStep = state.BoundCalls;
            var fromVertex = current.Path[^1];
            var visited = current.Path.ToHashSet();
            var candidates = new List<BranchCandidate>();

            foreach (var toVertex in graph.Vertices)
            {
                if (visited.Contains(toVertex))
                {
                    continue;
                }

                var childPath = new List<int>(current.Path) { toVertex };
                var edgeWeight = graph.GetWeight(fromVertex, toVertex);

                if (edgeWeight is null)
                {
                    state.ImpossibleBranches++;
                    candidates.Add(new BranchCandidate(childPath, null, null, CandidateStatus.Forbidden));
                    continue;
                }

                var prefixCost = LowerBoundCalculator.SafeAdd(current.PrefixCost, edgeWeight.Value);
                var childBound = CalculateBound(graph, childPath, prefixCost, state, trace);
                var child = new SearchNode(
                    childPath,
                    prefixCost,
                    childBound.LowerBound,
                    childBound.AlphaSum,
                    childBound.BetaSum,
                    ++serial);

                if (!childBound.IsPossible)
                {
                    state.ImpossibleBranches++;
                    candidates.Add(new BranchCandidate(childPath, edgeWeight, child, CandidateStatus.Impossible));
                }
                else if (child.LowerBound >= state.Best)
                {
                    state.PrunedBranches++;
                    candidates.Add(new BranchCandidate(childPath, edgeWeight, child, CandidateStatus.Pruned));
                }
                else
                {
                    candidates.Add(new BranchCandidate(childPath, edgeWeight, child, CandidateStatus.Pending));
                }
            }

            // Сортировка в обратном порядке нужна, чтобы лучшая ветвь оказалась на вершине стека.
            // Среди равных оценок первой выбирается ветвь с меньшим номером следующей вершины.
            var viable = candidates
                .Where(candidate => candidate.Status == CandidateStatus.Pending)
                .Select(candidate => candidate.Node!)
                .OrderByDescending(node => node.LowerBound)
                .ThenByDescending(node => node.Path[^1])
                .ToList();

            var chosenSerial = viable.Count == 0 ? -1 : viable[^1].Serial;
            TraceCandidates(trace, fromVertex, candidates, chosenSerial, state.Best, graph.VertexCount);

            foreach (var child in viable)
            {
                pending.Add(child);
            }

            trace?.AddText(
                $"Вызовов оценки H(D) на шаге: {state.BoundCalls - boundCallsBeforeStep}; " +
                $"всего с начала поиска: {state.BoundCalls}.");

            if (chosenSerial >= 0)
            {
                var next = pending[^1];
                trace?.AddText(
                    $"Спуск: {FormatPath(next.Path)}, H={FormatValue(next.LowerBound)}. " +
                    "К отложенным ветвям пока не возвращаемся.");
            }
            else
            {
                trace?.AddText(
                    "ДНО / ТУПИК: перспективных продолжений нет. Теперь разрешён возврат к отложенным ветвям.");
                TracePendingAfterLeaf(trace, pending, current.Path, state.Best);
            }
        }

        trace?.AddText(
            $"\nПереключений на отложенные ветви после достижения дна: {state.Returns}.\n" +
            "Один подъём - одно переключение от законченной ветви к другой отложенной, " +
            "а не каждый пройденный уровень дерева.");
        AddFinalStatistics(trace, state);

        return BuildResult(state, trace);
    }

    private static void FindInitialRecord(
        WeightedGraph graph,
        int startVertex,
        SearchState state,
        AlgorithmReport? trace)
    {
        var statistics = new MutableInitialSearchStatistics();
        var path = new List<int> { startVertex };
        var used = new HashSet<int> { startVertex };

        trace?.AddText(
            "ПРЕДВАРИТЕЛЬНЫЙ ПЕРЕБОР: ищем первый допустимый полный цикл.\n" +
            "Рёбра пробуем от меньшей цены к большей; при тупике возвращаемся назад.");

        if (!FindFirstCycle(graph, startVertex, path, used, statistics, trace))
        {
            trace?.AddText(
                "Ни одного допустимого цикла не найдено: начальный рекорд r=inf. " +
                "Основной поиск проверит, существует ли гамильтонов цикл.");
            state.InitialSearch = statistics.ToSnapshot();
            return;
        }

        var cost = CalculateCycleCost(graph, path);
        if (cost >= LowerBoundCalculator.Infinity)
        {
            // При допустимых весах и корректном полном пути эта ситуация недостижима,
            // но проверка защищает алгоритм от переполнения длинных сумм.
            trace?.AddText("Стоимость найденного цикла превышает допустимый диапазон вычислений.");
            state.InitialSearch = statistics.ToSnapshot();
            return;
        }

        state.Best = cost;
        state.BestPath = path.Append(startVertex).ToList();
        trace?.AddText(
            $"Первый найденный перебором маршрут: {FormatPath(state.BestPath)}.\n" +
            $"Начальный рекорд r={cost}.\n" +
            $"Проверено переходов между различными вершинами: {statistics.TriedEdges}; " +
            $"попыток замыкания цикла: {statistics.CheckedClosures}; " +
            $"возвратов при поиске начального цикла: {statistics.Backtracks}.\n" +
            $"Замыкающее ребро учитывается отдельно, поэтому полный цикл из {graph.VertexCount} вершин содержит " +
            $"{graph.VertexCount} рёбер.\n" +
            "Предварительный перебор остановлен: оптимальность рекорда ещё не доказана. Далее выполняется метод ветвей и границ.");

        state.InitialSearch = statistics.ToSnapshot();
    }

    private static bool FindFirstCycle(
        WeightedGraph graph,
        int startVertex,
        List<int> path,
        HashSet<int> used,
        MutableInitialSearchStatistics statistics,
        AlgorithmReport? trace)
    {
        if (path.Count == graph.VertexCount)
        {
            statistics.CheckedClosures++;
            var closingWeight = graph.GetWeight(path[^1], startVertex);
            trace?.AddText(
                closingWeight is null
                    ? $"Проверяем замыкание {path[^1]} -> {startVertex}: ребра нет. Возвращаемся назад."
                    : $"Проверяем замыкание {path[^1]} -> {startVertex}: вес {closingWeight.Value}. Найден полный цикл.");
            return closingWeight is not null;
        }

        var fromVertex = path[^1];
        var options = graph.Vertices
            .Where(vertex => !used.Contains(vertex))
            .Select(vertex => new { Vertex = vertex, Weight = graph.GetWeight(fromVertex, vertex) })
            .Where(option => option.Weight is not null)
            .OrderBy(option => option.Weight)
            .ThenBy(option => option.Vertex)
            .ToArray();

        foreach (var option in options)
        {
            statistics.TriedEdges++;
            trace?.AddText(
                $"Пробуем переход {fromVertex} -> {option.Vertex} (вес {option.Weight}). " +
                $"Текущий путь: {FormatPath(path.Append(option.Vertex))}.");

            path.Add(option.Vertex);
            used.Add(option.Vertex);

            if (FindFirstCycle(graph, startVertex, path, used, statistics, trace))
            {
                return true;
            }

            var failedPath = FormatPath(path);
            used.Remove(option.Vertex);
            path.RemoveAt(path.Count - 1);
            statistics.Backtracks++;
            trace?.AddText(
                $"Тупик на пути [{failedPath}]. Откат: удаляем вершину {option.Vertex} " +
                $"и пробуем следующее продолжение из {fromVertex}.");
        }

        return false;
    }

    private BoundCalculation CalculateBound(
        WeightedGraph graph,
        IReadOnlyList<int> path,
        long prefixCost,
        SearchState state,
        AlgorithmReport? trace)
    {
        state.BoundCalls++;
        return _boundCalculator.Calculate(graph, path, prefixCost, state.BoundCalls, trace);
    }

    private static long CalculateCycleCost(WeightedGraph graph, IReadOnlyList<int> path)
    {
        long cost = 0;
        for (var i = 0; i < path.Count; i++)
        {
            var from = path[i];
            var to = path[(i + 1) % path.Count];
            var weight = graph.GetWeight(from, to);
            if (weight is null)
            {
                return LowerBoundCalculator.Infinity;
            }

            cost = LowerBoundCalculator.SafeAdd(cost, weight.Value);
        }

        return cost;
    }

    private static void TraceCandidates(
        AlgorithmReport? trace,
        int fromVertex,
        IReadOnlyList<BranchCandidate> candidates,
        int chosenSerial,
        long record,
        int vertexCount)
    {
        if (trace is null)
        {
            return;
        }

        trace.AddText("Рассмотрение всех возможных продолжений текущего пути:");

        foreach (var candidate in candidates)
        {
            var path = FormatPath(candidate.Path);
            var toVertex = candidate.Path[^1];
            var line = $"  [{path}] (ребро {fromVertex} -> {toVertex}, вес {FormatNullableWeight(candidate.EdgeWeight)}): ";

            line += candidate.Status switch
            {
                CandidateStatus.Forbidden => "ОТСЕЧЕНО - ребро отсутствует.",
                CandidateStatus.Impossible => "ОТСЕЧЕНО - H(D) = inf, полный цикл невозможен.",
                CandidateStatus.Pruned =>
                    $"H(D)={FormatBoundExpression(candidate.Node!, vertexCount)} >= r={FormatValue(record)} - отсечено.",
                CandidateStatus.Pending when candidate.Node!.Serial == chosenSerial =>
                    $"H(D)={FormatBoundExpression(candidate.Node!, vertexCount)} < r={FormatValue(record)} - " +
                    "СЛЕДУЮЩАЯ К ВЫБОРУ, выполняем спуск.",
                CandidateStatus.Pending =>
                    $"H(D)={FormatBoundExpression(candidate.Node!, vertexCount)} < r={FormatValue(record)} - " +
                    "ПЕРСПЕКТИВНАЯ ВЕТВЬ, ОТКЛАДЫВАЕМ.",
                _ => throw new InvalidOperationException("Неизвестный статус ветви.")
            };

            trace.AddText(line);
        }
    }

    private static string FormatBoundExpression(SearchNode node, int vertexCount)
    {
        if (node.Path.Count == vertexCount)
        {
            var closingWeight = node.LowerBound - node.PrefixCost;
            return $"{node.PrefixCost} + {closingWeight} = {node.LowerBound}";
        }

        return $"{node.PrefixCost} + {node.AlphaSum} + {node.BetaSum} = {node.LowerBound}";
    }

    private static void TracePendingAfterLeaf(
        AlgorithmReport? trace,
        IReadOnlyList<SearchNode> pending,
        IReadOnlyList<int> finishedPath,
        long record)
    {
        if (trace is null)
        {
            return;
        }

        trace.AddText("ОТЛОЖЕННЫЕ ВЕТВИ (в порядке возврата, сверху стека):");
        if (pending.Count == 0)
        {
            trace.AddText("  Нет отложенных ветвей: дальнейших подъёмов не будет.");
            return;
        }

        SearchNode? next = null;
        var number = 0;
        for (var i = pending.Count - 1; i >= 0; i--)
        {
            var node = pending[i];
            number++;
            if (node.LowerBound >= record)
            {
                trace.AddText(
                    $"  {number}) {FormatPath(node.Path)}, H={FormatValue(node.LowerBound)} >= " +
                    $"r={FormatValue(record)} - будет отсечена без раскрытия.");
            }
            else
            {
                trace.AddText(
                    $"  {number}) {FormatPath(node.Path)}, H={FormatValue(node.LowerBound)} < " +
                    $"r={FormatValue(record)} - ожидает.");
                next ??= node;
            }
        }

        if (next is null)
        {
            trace.AddText("ВОЗВРАТ НЕ НУЖЕН: все оставшиеся ветви отсекаются.");
            return;
        }

        var commonLength = CommonPrefixLength(finishedPath, next.Path);
        var commonAncestor = finishedPath[commonLength - 1];
        trace.AddText(
            $"План подъёма: от {FormatPath(finishedPath)} к общему предку {commonAncestor}, " +
            $"затем продолжим {FormatPath(next.Path)} (H={FormatValue(next.LowerBound)}).");
    }

    private static void AddFinalStatistics(AlgorithmReport? trace, SearchState state)
    {
        trace?.AddText(
            "Итоговая статистика поиска:\n" +
            $"Раскрытых ветвей: {state.ExpandedNodes}; вызовов оценки H(D): {state.BoundCalls}; " +
            $"отсечений по рекорду: {state.PrunedBranches}; " +
            $"запрещённых / невозможных продолжений: {state.ImpossibleBranches}; " +
            $"возвратов к отложенным ветвям: {state.Returns}.");
    }

    private static TspResult BuildResult(SearchState state, AlgorithmReport? trace)
    {
        return new TspResult
        {
            BestPath = state.Best < LowerBoundCalculator.Infinity
                ? state.BestPath.ToArray()
                : Array.Empty<int>(),
            BestCost = state.Best < LowerBoundCalculator.Infinity ? state.Best : null,
            ExpandedNodes = state.ExpandedNodes,
            BoundCalls = state.BoundCalls,
            PrunedBranches = state.PrunedBranches,
            ImpossibleBranches = state.ImpossibleBranches,
            Returns = state.Returns,
            InitialSearch = state.InitialSearch,
            Trace = trace
        };
    }

    private static bool IsPrefix(IReadOnlyList<int> prefix, IReadOnlyList<int> path)
    {
        if (prefix.Count > path.Count)
        {
            return false;
        }

        for (var i = 0; i < prefix.Count; i++)
        {
            if (prefix[i] != path[i])
            {
                return false;
            }
        }

        return true;
    }

    private static int CommonPrefixLength(IReadOnlyList<int> left, IReadOnlyList<int> right)
    {
        var length = 0;
        var limit = Math.Min(left.Count, right.Count);
        while (length < limit && left[length] == right[length])
        {
            length++;
        }

        if (length == 0)
        {
            throw new InvalidOperationException("Пути поиска должны иметь общую стартовую вершину.");
        }

        return length;
    }

    private static string FormatPath(IEnumerable<int> path) => string.Join(" -> ", path);

    private static string FormatValue(long value) => LowerBoundCalculator.FormatValue(value);

    private static string FormatNullableWeight(long? weight) => weight?.ToString() ?? "inf";

    private sealed class SearchState
    {
        public long Best { get; set; } = LowerBoundCalculator.Infinity;

        public List<int> BestPath { get; set; } = [];

        public int ExpandedNodes { get; set; }

        public long BoundCalls { get; set; }

        public int PrunedBranches { get; set; }

        public int ImpossibleBranches { get; set; }

        public int Returns { get; set; }

        public InitialSearchStatistics InitialSearch { get; set; } = new();
    }

    private sealed record SearchNode(
        List<int> Path,
        long PrefixCost,
        long LowerBound,
        long AlphaSum,
        long BetaSum,
        int Serial);

    private sealed record BranchCandidate(
        List<int> Path,
        long? EdgeWeight,
        SearchNode? Node,
        CandidateStatus Status);

    private enum CandidateStatus
    {
        Forbidden,
        Impossible,
        Pruned,
        Pending
    }

    private sealed class MutableInitialSearchStatistics
    {
        public long TriedEdges { get; set; }

        public long Backtracks { get; set; }

        public long CheckedClosures { get; set; }

        public InitialSearchStatistics ToSnapshot()
        {
            return new InitialSearchStatistics
            {
                TriedEdges = TriedEdges,
                Backtracks = Backtracks,
                CheckedClosures = CheckedClosures
            };
        }
    }
}
