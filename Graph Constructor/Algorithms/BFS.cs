using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    public class BFS
    {
        private readonly Graph _graph;
        private readonly Vertex _from;
        private readonly HashSet<Vertex> _visited;
        private readonly Canvas _drawingArea;
        public List<int> Path { get; set; }
        public BFS(Graph graph, Canvas drawingArea, Vertex from)
        {
            _from = from;
            _graph = graph;
            _visited = new HashSet<Vertex>();
            Path = new List<int>();
            _drawingArea = drawingArea;
            SolvePath(_from);
            _visited.Clear();
            SolveAnimation(_from);
        }


        public void SolvePath(Vertex start)
        {
            Queue<Vertex> vertices = new Queue<Vertex>();
            vertices.Enqueue(start);
            _visited.Add(start);
            while (vertices.Count != 0)
            {
                start = vertices.Peek();
                Path.Add(start.Id);

                foreach (Edge edge in _graph.AdjacencyList[start])
                {
                    if (!_visited.Contains(edge.To))
                    {
                        vertices.Enqueue(edge.To);
                        _visited.Add(edge.To);
                    }
                }
                vertices.Dequeue();
            }
        }

        public async void SolveAnimation(Vertex start)
        {
            Queue<Vertex> vertices = new Queue<Vertex>();
            vertices.Enqueue(start);
            _visited.Add(start);
            while (vertices.Count != 0)
            {
                start = vertices.Peek();
                DrawingHelpers.MarkVertex(_drawingArea, start, Colors.VisitedVertex);
                await Task.Delay((int)Delay.Tiny);
                foreach (Edge edge in _graph.AdjacencyList[start])
                {
                    if (!_visited.Contains(edge.To))
                    {
                        DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.VisitedEdge);
                        DrawingHelpers.MarkVertex(_drawingArea, edge.To, Colors.VisitedVertex);
                        await Task.Delay((int)Delay.VeryShort);
                        DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.DefaultEdgeColor);
                        vertices.Enqueue(edge.To);
                        _visited.Add(edge.To);
                    }
                }
                vertices.Dequeue();
                DrawingHelpers.MarkVertex(_drawingArea, start, Colors.DoneVertex);
            }
        }
    }
}
