using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class FordFulkersson
    {
        readonly Vertex _source;
        readonly Vertex _sink;
        readonly Canvas _drawingArea;
        readonly Graph _graph;

        public List<List<Edge>> Paths { get; set; }
        public Dictionary<Edge, int> Residual { get; set; }
        public int MaxFlow { get; set; }
        public List<int> StepsMinFlow { get; set; }
        public List<Edge> MinCutEdges { get; set; }

        public FordFulkersson(Graph graph, Vertex source, Vertex sink, Canvas drawingArea)
        {
            _source = source;
            _sink = sink;
            _drawingArea = drawingArea;
            _graph = graph;
            Paths = new List<List<Edge>>();
            Residual = new Dictionary<Edge, int>();
            StepsMinFlow = new List<int>();
            MinCutEdges = new List<Edge>();
        }

        #region oldimplementation
        public async Task Init()
        {
            foreach (Edge edge in _graph.GetAllEdges())
            {
                Residual.Add(edge, 0);
                #region animation
                DrawingHelpers.UpdateEdgeFlow(_drawingArea, $"{edge.From.Id} {edge.To.Id}", edge, 0);
                await Task.Delay((int)Delay.VeryTiny);
                #endregion
            }
            await DetermineMaxFlow();
        }

        private async Task DetermineMaxFlow()
        {
            var path = Bfs(_source, _sink);

            while (path != null && path.Count > 0)
            {
                var minCapacity = int.MaxValue;
                foreach (var edge in path)
                {
                    if (edge.Cost < minCapacity)
                        minCapacity = edge.Cost;
                }

                if (minCapacity == int.MaxValue || minCapacity < 0)
                    throw new Exception("minCapacity " + minCapacity);

                AugmentPath(path, minCapacity);
                MaxFlow += minCapacity;
                Paths.Add(path);
                path = Bfs(_source, _sink);
            }
        }

        private void AugmentPath(IEnumerable<Edge> path, int minCapacity)
        {
            foreach (var edge in path)
            {
                edge.Cost -= minCapacity;
                Residual[edge] += minCapacity;
            }
        }

        private List<Edge> Bfs(Vertex root, Vertex target)
        {
            root.TraverseParent = null!;
            target.TraverseParent = null!;

            var queue = new Queue<Vertex>();
            var discovered = new HashSet<Vertex>();
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                discovered.Add(current);

                if (current.Id == target.Id)
                    return GetPath(current);

                foreach (var edge in _graph.AdjacencyList[current])
                {
                    var next = edge.To;
                    if (edge.Cost > 0 && !discovered.Contains(next))
                    {
                        next.TraverseParent = current;
                        queue.Enqueue(next);
                    }
                }
            }
            return null;
        }

        private List<Edge> GetPath(Vertex node)
        {
            var path = new List<Edge>();
            var current = node;
            while (current.TraverseParent != null)
            {
                var edge = _graph.GetEdge(current.TraverseParent, current);
                path.Add(edge);
                current = current.TraverseParent;
            }
            return path;
        }

        async Task ChangeFlowInCurrentPath(List<Edge> path)
        {
            DrawingHelpers.ClearCanvasFromAnimationEffects(_drawingArea);

            int minFlow = int.MaxValue;
            foreach (var edge in path)
            {
                int min = edge.Cost - Residual[edge];
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
                Residual[edge] += minFlow;
                #region animation
                DrawingHelpers.UpdateEdgeFlow(_drawingArea, $"{edge.From.Id} {edge.To.Id}", edge, Residual[edge]);
                await Task.Delay((int)Delay.VeryTiny);
                #endregion
            }
            MaxFlow += minFlow;
        }
        #endregion
    }
}
