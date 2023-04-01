using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Linq;
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
        public List<Tag> Tags { get; set; }

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
        }

        void InitializeTags()
        {
            Tags = new List<Tag>();
            foreach (var vertex in _graph.GetAllVertices())
            {
                Tag tag = new Tag(vertex);
                if (vertex.Id == _from.Id)
                    tag.Cost = 0;
                else tag.Cost = int.MaxValue;
                Tags.Add(tag);
            }
        }

        void DetermineTags()
        {
            //>
            //bool greaterThanInequality = true;
            Dictionary<Edge, bool> diffInequality = new Dictionary<Edge, bool>();
            int diff = 0;
            var edges = _graph.GetAllEdges();

            while (diffInequality.Values.Any(greaterThan => greaterThan))
            {
                foreach (var edge in edges)
                {
                    if (!diffInequality.ContainsKey(edge))
                        diffInequality.Add(edge, true);
                    //greaterThanInequality = true;
                    Tag hj = Tags.Where(tag => tag.Vertex == edge.To).First();
                    Tag hi = Tags.Where(tag => tag.Vertex == edge.From).First();
                    diff = hj.Cost - hi.Cost;

                    if (diff > edge.Cost)
                    {
                        hj.Cost = hi.Cost + edge.Cost;
                        //greaterThanInequality = false;
                    }
                    if (diff == edge.Cost)
                    {
                        hj.VerticesWichCome.Add(edge.From);
                        diffInequality[edge] = false;
                    }
                    if (diff < edge.Cost) diffInequality[edge] = false; 
                }
            }
        }
    }
}
