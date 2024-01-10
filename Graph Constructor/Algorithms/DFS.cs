using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    public class DFS : Algorithm
    {
        private HashSet<Vertex> _visited;

        public List<int> Path { get; set; }
        public AlgorithmSteps Steps { get; set; }
        public DFS(Graph graph, Canvas drawingArea, Vertex from) : base(graph, drawingArea, from, null)
        {
            _visited = new HashSet<Vertex>();
            Path = new List<int>();
            Steps = new AlgorithmSteps(drawingArea);
        }

        public override async Task Execute()
        {
            await SolveDFS(start);
        }

        private async Task SolveDFS(Vertex start)
        {
            Stack<Vertex> vertices = new Stack<Vertex>();
            bool hasNext;
            bool foundUnvisitedVertex = true;
            vertices.Push(start);
            _visited.Add(start);
            Path.Add(start.Id);
            while (vertices.Count != 0)
            {
                start = vertices.Peek();
                hasNext = false;
                if (!_visited.Contains(start) || foundUnvisitedVertex)
                {
                    DrawingHelpers.MarkVertex(drawingArea, start, Colors.VisitedVertex);
                    Steps.Add(new AlgorithmStep().AddMarkedElement(start, Colors.VisitedVertex));
                    await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                    foundUnvisitedVertex = false;
                }
                foreach (Edge edge in graph.AdjacencyList[start])
                {
                    if (!_visited.Contains(edge.To))
                    {
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                        Steps.Add(new AlgorithmStep().AddMarkedElement(edge, Colors.VisitedEdge));
                        await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                        vertices.Push(edge.To);
                        _visited.Add(edge.To);
                        Path.Add(edge.To.Id);
                        hasNext = true;
                        foundUnvisitedVertex = true;
                        break;
                    }
                }
                if (!hasNext)
                {
                    vertices.Pop();
                    DrawingHelpers.MarkVertex(drawingArea, start, Colors.DoneVertex);
                    Steps.Add(new AlgorithmStep().AddMarkedElement(start, Colors.DoneVertex));
                }
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

        public override AlgorithmSteps GetSolvingSteps()
        {
            return Steps;
        }
    }
}
