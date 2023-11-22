﻿using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class Ford
    {
        private readonly Graph _graph;
        private readonly Vertex _from;
        private readonly Vertex _target;
        private readonly Canvas _drawingArea;

        //etichetele
        public Dictionary<Vertex, Tag> Tags { get; set; }
        public List<List<Vertex>> Paths { get; set; }
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

        public Ford(Graph graph, Canvas drawingArea, Vertex from, Vertex target)
        {
            _from = from;
            _target = target;
            _graph = graph;
            _drawingArea = drawingArea;
            Tags = new Dictionary<Vertex, Tag>();
            Paths = new List<List<Vertex>>();
        }

        public async Task Init()
        {
            InitializeTags();
            Tags = await DetermineTags();
            GetPaths();
            RemoveDuplicatePaths();
            await HighlightPathsOnCanvas();
        }

        void InitializeTags()
        {
            foreach (var vertex in _graph.GetAllVertices())
            {
                Tag tag = new Tag(vertex);
                if (vertex.Id == _from.Id)
                    tag.Cost = 0;
                else tag.Cost = int.MinValue/2;
                Tags.Add(vertex, tag);
            }
        }

        async Task<Dictionary<Vertex, Tag>> DetermineTags()
        {
            Dictionary<Vertex, Tag> tags = Tags;
            Dictionary<Edge, bool> diffInequality = new Dictionary<Edge, bool>();
            int diff = 0;
            var edges = _graph.GetAllEdges();
            foreach (var edge in edges)
                diffInequality.Add(edge, true);

            while (diffInequality.Values.Any(greaterThan => greaterThan))
            {
                foreach (var edge in edges)
                {
                    Tag hj = Tags[edge.To];
                    Tag hi = Tags[edge.From];
                    diff = hj.Cost - hi.Cost;
                    DrawingHelpers.MarkVertex(_drawingArea, edge.From, Colors.DoneVertex);
                    DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.VisitedEdge);
                    DrawingHelpers.MarkVertex(_drawingArea, edge.To, Colors.VisitedVertex);
                    await Task.Delay((int)Delay.VeryTiny);
                    DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.DefaultEdgeColor);
                    DrawingHelpers.MarkVertex(_drawingArea, edge.To, Colors.DefaultVertexColor);
                    DrawingHelpers.MarkVertex(_drawingArea, edge.From, Colors.DefaultVertexColor);
                    if (diff < edge.Cost)
                        hj.Cost = hi.Cost + edge.Cost;
                    if (diff == edge.Cost)
                    {
                        hj.IncomingVertices.Add(edge.From);
                        diffInequality[edge] = false;
                    }
                    if (diff > edge.Cost) diffInequality[edge] = false;
                }
            }
            return tags;
        }

        void GetPaths()
        {
            HashSet<Vertex> visited = new HashSet<Vertex>();
            List<Vertex> path = new List<Vertex>();
            path.Insert(0, _target);
            GetPathsUtil(_target, _from, visited, path);
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
            DrawingHelpers.ClearCanvasFromAnimationEffects(_drawingArea);
            Vertex prevVertex = _from;
            foreach (var path in Paths)
            {
                foreach (var vertex in path)
                {
                    if (vertex != _from)
                    {
                        Edge edge = _graph.GetEdge(prevVertex, vertex);
                        DrawingHelpers.MarkEdge(_drawingArea, edge, Colors.VisitedEdge);
                        await Task.Delay((int)Delay.VeryTiny);
                    }
                    DrawingHelpers.MarkVertex(_drawingArea, vertex, Colors.DoneVertex);
                    await Task.Delay((int)Delay.VeryTiny);
                    prevVertex = vertex;
                }
            }
        }
    }
}