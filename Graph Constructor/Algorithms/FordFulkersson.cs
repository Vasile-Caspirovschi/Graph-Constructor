using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class FordFulkersson
    {
        readonly Vertex _source;
        readonly Vertex _target;
        readonly Canvas _drawingArea;
        readonly Graph _graph;

        public List<List<Edge>> AllPathsFromSourceToTarget { get; set; }
        public Dictionary<Edge, int> EdgeFlows { get; set; }
        public int MaxFlow { get; set; }
        public List<int> StepsMinFlow { get; set; }

        public FordFulkersson(Graph graph, Vertex source, Vertex target, Canvas drawingArea)
        {
            _source = source;
            _target = target;
            _drawingArea = drawingArea;
            _graph = graph;
            AllPathsFromSourceToTarget = new List<List<Edge>>();
            EdgeFlows = new Dictionary<Edge, int>();
            StepsMinFlow = new List<int>();
        }

        public async Task Init()
        {
            foreach (Edge edge in _graph.GetAllEdges())
            {
                EdgeFlows.Add(edge, 0);
                #region animation
                DrawingHelpers.UpdateEdgeFlow(_drawingArea, $"{edge.From.Id} {edge.To.Id}", edge, 0);
                await Task.Delay((int)Delay.VeryTiny - 50);
                #endregion
            }
            await DetermineMaxFlow();
        }

        async Task DetermineMaxFlow()
        {
            HashSet<Edge> visited = new HashSet<Edge>();
            List<Edge> path;
            foreach (var adjacentEdge in _graph.AdjacencyList[_source])
            {
                path = new List<Edge>
                {
                    adjacentEdge
                };
                await CheckAllPaths(adjacentEdge, _target, visited, path);
            }
        }

        async Task CheckAllPaths(Edge startEdge, Vertex target, HashSet<Edge> visited, List<Edge> localPath)
        {
            if (startEdge.To.Equals(target))
            {
                AllPathsFromSourceToTarget.Add(new List<Edge>(localPath));
                await ChangeFlowInCurrentPath(localPath);
                return;
            }
            foreach (var edge in _graph.AdjacencyList[startEdge.To])
            {
                if (!visited.Contains(edge) && EdgeFlows[edge] != edge.Cost)
                {
                    visited.Add(startEdge);
                    localPath.Add(edge);
                    await CheckAllPaths(edge, target, visited, localPath);
                    localPath.Remove(edge);
                }
            }
            visited.Remove(startEdge);
        }

        async Task ChangeFlowInCurrentPath(List<Edge> path)
        {
            DrawingHelpers.ClearCanvasFromAnimationEffects(_drawingArea);

            int minFlow = int.MaxValue;
            foreach (var edge in path)
            {
                int min = edge.Cost - EdgeFlows[edge];
                if (min < minFlow)
                    minFlow = min;
                #region animation
                DrawingHelpers.MarkVertex(_drawingArea, edge.From, Colors.VisitedVertex);
                await Task.Delay((int)Delay.VeryTiny);
                DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.VisitedEdge);
                await Task.Delay((int)Delay.VeryTiny);
                DrawingHelpers.MarkVertex(_drawingArea, edge.From, Colors.VisitedVertex);
                await Task.Delay((int)Delay.VeryTiny);
                #endregion
            }
            StepsMinFlow.Add(minFlow);
            foreach (var edge in path)
            {
                EdgeFlows[edge] += minFlow;
                #region animation
                DrawingHelpers.UpdateEdgeFlow(_drawingArea, $"{edge.From.Id} {edge.To.Id}", edge, EdgeFlows[edge]);
                await Task.Delay((int)Delay.VeryTiny);
                #endregion
            }
            MaxFlow += minFlow;
        }
    }
}
