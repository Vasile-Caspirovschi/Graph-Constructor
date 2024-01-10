using Graph_Constructor.Interfaces;

namespace Graph_Constructor.Models
{

    public class Edge : IMarkable
    {
        Vertex _from;
        Vertex _to;
        int _cost = 1;

        public Edge(Vertex from, Vertex to)
        {
            _from = from;
            _to = to;
        }

        public Edge(Vertex from, Vertex to, int cost)
        {
            _from = from;
            _to = to;
            _cost = cost;
        }

        public Vertex From { get { return _from; } }
        public Vertex To { get { return _to; } }
        public int Cost { get => _cost; set => _cost = value; }

        public bool IsVertex()
        {
            return false;
        }

        public override string ToString()
        {
            return $"{From} -> {To}";
        }
    }
}
