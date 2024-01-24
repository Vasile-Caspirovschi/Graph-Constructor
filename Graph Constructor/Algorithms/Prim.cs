using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    public class Prim : Algorithm
    {
        public List<Edge> MST { get; set; }
        public AlgorithmSteps Steps { get; set; }
        public Prim(Graph graph, Canvas drawingArea, Vertex from) : base(graph, drawingArea, from, null)
        {
            MST = new();
            Steps = new(drawingArea,nameof(Prim));
            base.graph = new Graph(graph.AdjacencyList, Enums.GraphType.Undirected);
        }

        public void SolvePrim()
        {
            var verticesCount = graph.GetVerticesCount();
            HashSet<int> connected = new();
            connected.Add(start.Id);

            PriorityQueue<Edge, int> minEdges = new PriorityQueue<Edge, int>();

            while (connected.Count < verticesCount)
            {
                foreach (var edge in graph.GetAllEdges()
                        .Where(ed => (ed.From.Id == start.Id && !connected.Contains(ed.To.Id))
                            || (ed.To.Id == start.Id && !connected.Contains(ed.From.Id)))
                        .Where(ed => !connected.Contains(ed.To.Id)))
                    if (!connected.Contains(edge.From.Id) || !connected.Contains(edge.To.Id))
                        minEdges.Enqueue(edge, edge.Cost);

                if (minEdges.Count == 0)
                    break;

                var minEdge = minEdges.Dequeue();

                connected.Add(minEdge.To.Id);
                start = minEdge.To;
                MST.Add(minEdge);
            }
        }

        public override void BindViewProperties(params Control[] viewControls)
        {
            return;
        }

        public override async Task Execute()
        {
            SolvePrim();
            await Task.CompletedTask;
        }

        public override AlgoLog GetResults()
        {
            var log = new AlgoLog("Minimum spanning tree using Prim's algorithm", "");
            var cost = 0;
            foreach (var edge in MST)
            {
                log.AddMoreDetails(edge);
                cost += edge.Cost;
            }
            log.AddMoreDetails($"Minimum cost = {cost}");
            return log;
        }

        public override AlgorithmSteps GetSolvingSteps()
        {
            return Steps;
        }
    }
}
