using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Graph_Constructor.Algorithms
{
    public class DFS
    {
        private readonly Graph _graph;
        private readonly Vertex _from;
        private readonly HashSet<Vertex> _visited;
        private readonly Canvas _drawingArea;
        public List<int> Path { get; set; } 

        public DFS(Graph graph, Canvas drawingArea, Vertex from)
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


        public void SolvePath(Vertex at)
        {
            _visited.Add(at);
            Path.Add(at.Id);
            foreach (Edge edge in _graph.AdjacencyList[at])
                if (!_visited.Contains(edge.To))
                    SolvePath(edge.To);
        }

        public async void SolveAnimation(Vertex start)
        {
            List<int> path = new List<int>();
            Stack<Vertex> vertices = new Stack<Vertex>();
            bool hasNext;
            vertices.Push(start);
            _visited.Add(start);
            while (vertices.Count != 0)
            {
                start = vertices.Peek();
                hasNext = false;
                DrawingHelpers.MarkVertex(_drawingArea, start, Colors.VisitedVertex);
                await Task.Delay((int)Delay.VeryShort);
                foreach (Edge edge in _graph.AdjacencyList[start])
                {
                    if (!_visited.Contains(edge.To))
                    {
                        DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.VisitedEdge);
                        await Task.Delay((int)Delay.VeryShort);
                        vertices.Push(edge.To);
                        _visited.Add(edge.To);
                        hasNext = true;
                        break;
                    }
                }
                if (!hasNext)
                {
                    vertices.Pop();
                    DrawingHelpers.MarkVertex(_drawingArea, start, Colors.DoneVertex);
                }
            }
        }
    }
}
