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

        public BFS(Graph graph, Canvas drawingArea, Vertex from) : base(graph, drawingArea, from, null)
        {
            _visited = new HashSet<Vertex>();
            Path = new List<int>();
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
            _visited.Add(from);
            while (vertices.Count != 0)
            {
                from = vertices.Peek();
                Path.Add(from.Id);
                DrawingHelpers.MarkVertex(drawingArea, from, Colors.VisitedVertex);
                await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                foreach (Edge edge in graph.AdjacencyList[from])
                {
                    if (!_visited.Contains(edge.To))
                    {
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                        DrawingHelpers.MarkVertex(drawingArea, edge.To, Colors.VisitedVertex);
                        await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.DefaultEdgeColor);
                        vertices.Enqueue(edge.To);
                        _visited.Add(edge.To);
                    }
                }
                vertices.Dequeue();
                DrawingHelpers.MarkVertex(drawingArea, from, Colors.DoneVertex);
            }
        }

        public override AlgoLog GetResults()
        {
            return new AlgoLog($"DFS from {start.Id}\n", Path);
        }

        public override void BindViewProperties(params Control[] controls)
        {
            return;
        }
    }
}
