namespace Graph_Constructor.Models
{

    public class Edge
    {
        Vertex _from;
        Vertex _to;

        public Edge(Vertex from, Vertex to)
        {
            _from = from;
            _to = to;
        }

        public Vertex From { get { return _from; } }
        public Vertex To { get { return _to; } }

        public override string ToString()
        {
            return $"{From} -> {To}";
        }
    }
}
