﻿using Graph_Constructor.Enums;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class FordFulkersson : Algorithm
    {
        public List<List<Edge>> Paths { get; set; }
        public Dictionary<Edge, int> Residual { get; set; }
        public int MaxFlow { get; set; }
        public List<int> StepsMinFlow { get; set; }
        public AlgorithmSteps Steps { get; set; }
        public FordFulkersson(Graph graph, Canvas drawingArea, Vertex start, Vertex? target) : base(graph, drawingArea, start, target)
        {
            Paths = new List<List<Edge>>();
            Residual = new Dictionary<Edge, int>();
            StepsMinFlow = new List<int>();
            Steps = new AlgorithmSteps(drawingArea, "FordFulkersson");
        }

        public override async Task Execute()
        {
            var step = new AlgorithmStep();
            foreach (Edge edge in graph.GetAllEdges())
            {
                Residual.Add(edge, 0);
                DrawingHelpers.UpdateEdgeFlow(drawingArea, $"{edge.From.Id} {edge.To.Id}", edge, 0);
                await Task.Delay(SetExecutionDelay((int)Delay.Tiny));
                step.AddMarkedElement(edge, Colors.DefaultEdgeColor);
            }
            await DetermineMaxFlow();
            DrawingHelpers.ClearCanvasFromAnimationEffects(drawingArea);
        }

        private async Task DetermineMaxFlow()
        {
            var path = Bfs(start, target!);

            while (path != null && path.Count > 0)
            {
                await ChangeFlowInCurrentPath(path);
                Paths.Add(path);
                path = Bfs(start, target!);
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

                foreach (var edge in graph.AdjacencyList[current])
                {
                    var next = edge.To;
                    if (edge.Cost - Residual[edge] > 0 && !discovered.Contains(next))
                    {
                        next.TraverseParent = current;
                        queue.Enqueue(next);
                    }
                }
            }
            return null!;
        }

        private List<Edge> GetPath(Vertex node)
        {
            var path = new List<Edge>();
            var current = node;
            while (current.TraverseParent != null)
            {
                var edge = graph.GetEdge(current.TraverseParent, current);
                path.Insert(0, edge);
                current = current.TraverseParent;
            }
            return path;
        }

        async Task ChangeFlowInCurrentPath(List<Edge> path)
        {
            Steps.Add(new AlgorithmStep(DrawingHelpers.ClearCanvasFromAnimationEffects));
            DrawingHelpers.ClearCanvasFromAnimationEffects(drawingArea);
            int minFlow = int.MaxValue;
            foreach (var edge in path)
            {
                int min = edge.Cost - Residual[edge];
                if (min < minFlow)
                    minFlow = min;
                DrawingHelpers.MarkVertex(drawingArea, edge.From, Colors.VisitedVertex);
                Steps.Add(new AlgorithmStep()
                    .AddMarkedElement(edge.From, Colors.VisitedVertex));
                await Task.Delay(SetExecutionDelay((int)Delay.Short));
                DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                Steps.Add(new AlgorithmStep()
                    .AddMarkedElement(edge, Colors.VisitedEdge));
                await Task.Delay(SetExecutionDelay((int)Delay.Short));
                DrawingHelpers.MarkVertex(drawingArea, edge.To, Colors.VisitedVertex);
                Steps.Add(new AlgorithmStep()
                    .AddMarkedElement(edge.To, Colors.VisitedVertex));
                await Task.Delay(SetExecutionDelay((int)Delay.Short));
            }
            StepsMinFlow.Add(minFlow);
            foreach (var edge in path)
            {
                Residual[edge] += minFlow;
                DrawingHelpers.UpdateEdgeFlow(drawingArea, $"{edge.From.Id} {edge.To.Id}", edge, Residual[edge]);
                Steps.Add(new AlgorithmStep()
                    .AddMarkedElement(edge, Colors.VisitedEdge));
                await Task.Delay(SetExecutionDelay((int)Delay.Short));
            }
            MaxFlow += minFlow;
        }

        public override AlgoLog GetResults()
        {
            var title = $"The max flow {start.Id} to {target!.Id} is {MaxFlow}";
            var details = $"All steps are below:";
            AlgoLog log = new AlgoLog(title, details);

            int index = 0;
            foreach (var path in Paths)
            {
                var vertices = path.Select(edge => edge.To.Id).ToList();
                vertices.Insert(0, 1);
                log.AddMoreDetails(vertices, $"min = {StepsMinFlow[index++]}");
            }
            return log;
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
