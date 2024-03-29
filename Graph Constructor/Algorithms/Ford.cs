﻿using Graph_Constructor.Comparers;
using Graph_Constructor.Enums;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class Ford : Algorithm
    {
        //etichetele
        public Dictionary<Vertex, Tag> Tags { get; set; }
        public List<List<Vertex>> Paths { get; set; }
        public AlgorithmSteps Steps { get; set; }

        public class Tag
        {
            public Vertex Vertex { get; set; }
            public int Cost { get; set; }
            public List<Vertex> IncomingVertices { get; set; }
            public Tag(Vertex vertex)
            {
                Vertex = vertex;
                IncomingVertices = new List<Vertex>();
            }
        }

        public Ford(Graph graph, Canvas drawingArea, Vertex start, Vertex target)
            : base(graph, drawingArea, start, target)
        {
            Tags = new Dictionary<Vertex, Tag>();
            Paths = new List<List<Vertex>>();
            Steps = new AlgorithmSteps(drawingArea, "Ford");
        }

        public override async Task Execute()
        {
            InitializeTags();
            await DetermineTags();
            GetPaths();
            RemoveDuplicatePaths();
            await HighlightPathsOnCanvas();
        }

        void InitializeTags()
        {
            foreach (var vertex in graph.GetAllVertices())
            {
                Tag tag = new Tag(vertex);
                if (vertex.Id == start.Id)
                    tag.Cost = 0;
                else tag.Cost = int.MaxValue;
                Tags.Add(vertex, tag);
            }
        }

        async Task DetermineTags()
        {
            Dictionary<Edge, bool> diffInequality = new Dictionary<Edge, bool>();
            int diff = 0;
            var edges = graph.GetAllEdges();
            foreach (var edge in edges)
                diffInequality.Add(edge, true);

            while (diffInequality.Values.Any(greaterThan => greaterThan))
            {
                Edge? previousEdge = null;
                foreach (var edge in edges)
                {
                    Tag hj = Tags[edge.To];
                    Tag hi = Tags[edge.From];
                    diff = hj.Cost - hi.Cost;
                    DrawingHelpers.MarkVertex(drawingArea, edge.From, Colors.VisitedVertex);
                    DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                    DrawingHelpers.MarkVertex(drawingArea, edge.To, Colors.VisitedVertex);
                    AlgorithmStep step = new AlgorithmStep();
                    if (previousEdge is not null)
                        step.AddMarkedElement(new Vertex(previousEdge.From.Id), Colors.DefaultVertexColor)
                            .AddMarkedElement(new Edge(previousEdge.From, previousEdge.To, previousEdge.Cost), Colors.DefaultEdgeColor)
                            .AddMarkedElement(new Vertex(previousEdge.To.Id), Colors.DefaultVertexColor);
                    step.AddMarkedElement(edge.From, Colors.VisitedVertex)
                        .AddMarkedElement(edge, Colors.VisitedEdge)
                        .AddMarkedElement(edge.To, Colors.VisitedVertex);
                    Steps.Add(step);
                    await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                    DrawingHelpers.MarkVertex(drawingArea, edge.To, Colors.DefaultVertexColor);
                    DrawingHelpers.MarkEdge(drawingArea, edge, Colors.DefaultEdgeColor);
                    DrawingHelpers.MarkVertex(drawingArea, edge.From, Colors.DefaultVertexColor);

                    if (diff > edge.Cost)
                        hj.Cost = hi.Cost + edge.Cost;
                    if (diff == edge.Cost)
                    {
                        hj.IncomingVertices.Add(edge.From);
                        diffInequality[edge] = false;
                    }
                    if (diff < edge.Cost) diffInequality[edge] = false;
                    previousEdge = edge;
                }
                if (previousEdge is not null)
                    Steps.Add(new AlgorithmStep()
                        .AddMarkedElement(new Vertex(previousEdge.From.Id), Colors.DefaultVertexColor)
                        .AddMarkedElement(new Edge(previousEdge.From, previousEdge.To, previousEdge.Cost), Colors.DefaultEdgeColor)
                        .AddMarkedElement(new Vertex(previousEdge.To.Id), Colors.DefaultVertexColor));
            }
        }

        void GetPaths()
        {
            HashSet<Vertex> visited = new HashSet<Vertex>();
            List<Vertex> path = new List<Vertex>();
            path.Insert(0, target!);
            GetPathsUtil(target!, start, visited, path);
        }

        void GetPathsUtil(Vertex start, Vertex target, HashSet<Vertex> visited, List<Vertex> localPath)
        {
            if (start.Equals(target))
            {
                Paths.Add(new List<Vertex>(localPath));
                return;
            }
            visited.Add(start);
            foreach (var vertex in Tags[start].IncomingVertices)
            {
                if (!visited.Contains(vertex))
                {
                    localPath.Insert(0, vertex);
                    GetPathsUtil(vertex, target, visited, localPath);
                    localPath.Remove(vertex);
                }
            }
            visited.Remove(start);
        }

        void RemoveDuplicatePaths()
        {
            Paths = new List<List<Vertex>>(Paths.Distinct(new ListEqualityComparer<Vertex>()));
        }

        async Task HighlightPathsOnCanvas()
        {
            DrawingHelpers.ClearCanvasFromAnimationEffects(drawingArea);
            Steps.Add(new AlgorithmStep(DrawingHelpers.ClearCanvasFromAnimationEffects));
            Vertex prevVertex = start;
            foreach (var path in Paths)
            {
                var step = new AlgorithmStep();
                foreach (var vertex in path)
                {
                    if (vertex != start)
                    {
                        Edge edge = graph.GetEdge(prevVertex, vertex);
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                        await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                        step.AddMarkedElement(edge, Colors.VisitedEdge);
                    }
                    DrawingHelpers.MarkVertex(drawingArea, vertex, Colors.DoneVertex);
                    await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                    step.AddMarkedElement(vertex, Colors.DoneVertex);
                    prevVertex = vertex;
                }
                Steps.Add(step);
            }
        }

        public override AlgoLog GetResults()
        {
            var title = $"The min path length from {start.Id} to {target!.Id} is {Tags[target].Cost}";
            var details = "All paths are:";
            AlgoLog log = new AlgoLog(title, details);
            Paths.ForEach(path => log.AddMoreDetails(path.Select(vertex => vertex.Id).ToList()));
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