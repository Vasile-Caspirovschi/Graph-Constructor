using Graph_Constructor.Enums;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    public class BFS : Algorithm
    {
        private readonly HashSet<Vertex> _visited;
        public List<int> Path { get; set; }
        public AlgorithmSteps Steps { get; set; }
        public BFS(Graph graph, Canvas drawingArea, Vertex from) : base(graph, drawingArea, from, null)
        {
            _visited = new HashSet<Vertex>();
            Path = new List<int>();
            Steps = new AlgorithmSteps(drawingArea, "BFS");
        }

        public override async Task Execute()
        {
            await SolveBFS();
        }

        public async Task SolveBFS()
        {
            Queue<Vertex> vertices = new Queue<Vertex>();
            Vertex from = start;
            vertices.Enqueue(from);
            while (vertices.Count != 0)
            {
                from = vertices.Peek();
                Path.Add(from.Id);
                DrawingHelpers.MarkVertex(drawingArea, from, Colors.VisitedVertex);
                if (!_visited.Contains(from))
                {
                    Steps.Add(new AlgorithmStep().AddMarkedElement(from, Colors.VisitedVertex));
                    _visited.Add(from);
                }
                await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                Edge? previousEdge = null;
                foreach (Edge edge in graph.AdjacencyList[from])
                {
                    if (!_visited.Contains(edge.To))
                    {
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                        DrawingHelpers.MarkVertex(drawingArea, edge.To, Colors.VisitedVertex);
                        var step = new AlgorithmStep()
                            .AddMarkedElement(edge.To, Colors.VisitedVertex)
                            .AddMarkedElement(edge, Colors.VisitedEdge);
                        if (previousEdge is not null) step.AddMarkedElement(previousEdge, Colors.DefaultEdgeColor);
                        Steps.Add(step);
                        await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.DefaultEdgeColor);
                        vertices.Enqueue(edge.To);
                        _visited.Add(edge.To);
                    }
                    previousEdge = edge;
                }
                vertices.Dequeue();
                DrawingHelpers.MarkVertex(drawingArea, from, Colors.DoneVertex);
                var step1 = new AlgorithmStep().AddMarkedElement(from, Colors.DoneVertex);
                if (previousEdge is not null) step1.AddMarkedElement(previousEdge, Colors.DefaultEdgeColor);
                Steps.Add(step1);
            }
        }

        public override AlgoLog GetResults()
        {
            return new AlgoLog($"BFS from {start.Id}\n", Path);
        }

        public override void BindViewProperties(params Control[] controls)
        {
            return;
        }

        public override AlgorithmSteps GetSolvingSteps()
        {
            return Steps;
        }
    }
}
