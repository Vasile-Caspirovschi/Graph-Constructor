using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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
        public List<LinkedList<Vertex>> Paths { get; set; }
        public class Tag
        {
            public Vertex Vertex { get; set; }
            public int Cost { get; set; }
            public List<Vertex> VerticesWichCome { get; set; }
            public Tag(Vertex vertex)
            {
                Vertex = vertex;
                VerticesWichCome = new List<Vertex>();
            }
        }

        public Ford(Graph graph, Canvas drawingArea, Vertex from, Vertex target)
        {
            _from = from;
            _target = target;
            _graph = graph;
            _drawingArea = drawingArea;
            InitializeTags();
            DetermineTags();
            GetPaths();
        }

        void InitializeTags()
        {
            Tags = new Dictionary<Vertex, Tag>();
            foreach (var vertex in _graph.GetAllVertices())
            {
                Tag tag = new Tag(vertex);
                if (vertex.Id == _from.Id)
                    tag.Cost = 0;
                else tag.Cost = int.MaxValue;
                Tags.Add(vertex, tag);
            }
        }

        void DetermineTags()
        {
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

                    if (diff > edge.Cost)
                        hj.Cost = hi.Cost + edge.Cost;
                    if (diff == edge.Cost)
                    {
                        hj.VerticesWichCome.Add(edge.From);
                        diffInequality[edge] = false;
                    }
                    if (diff < edge.Cost) diffInequality[edge] = false;
                }
            }
        }

        void GetPaths()
        {
            HashSet<Vertex> visited = new HashSet<Vertex>();
            LinkedList<Vertex> path = new LinkedList<Vertex>();
            Paths = new List<LinkedList<Vertex>>();
            path.AddFirst(_target);
            GetPathsUtil(_target, _from, visited, path);
        }

        void GetPathsUtil(Vertex start, Vertex target, HashSet<Vertex> visited, LinkedList<Vertex> localPath)
        {
            if (start.Equals(target))
            {
                Paths.Add(new LinkedList<Vertex>(localPath));
                return;
            }
            visited.Add(start);

            foreach (var vertex in Tags[start].VerticesWichCome)
            {
                if (!visited.Contains(vertex))
                {
                    localPath.AddFirst(vertex);
                    GetPathsUtil(vertex, target, visited, localPath);
                    localPath.Remove(vertex);
                }
            }
            visited.Remove(start);
        }
    }
}