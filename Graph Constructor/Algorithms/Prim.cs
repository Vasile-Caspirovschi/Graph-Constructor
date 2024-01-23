using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System;
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
            Steps = new(drawingArea, nameof(Prim));
        }

        public void SolvePrim()
        {
            var verticesCount = graph.GetVerticesCount();
            foreach (var vertex in graph.GetAllVertices())
            {
                int minWeight = int.MaxValue;
                Edge? minEdge = null;
                foreach (var edge in graph.GetAllEdges().Where(ed => ed.From.Id == vertex.Id))
                {
                    if (!MST.Contains(edge))
                    {
                        if (edge.Cost < minWeight)
                        {
                            minWeight = edge.Cost;
                            minEdge = edge;
                        }
                    }
                }
                if (minEdge is not null)
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
            var log = new AlgoLog("Minimum spanning tree using Prim's algorithm","");
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
