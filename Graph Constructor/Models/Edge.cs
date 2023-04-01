namespace Graph_Constructor.Models
{

    public class Edge
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
            Cost = cost;
        }

        public Vertex From { get { return _from; } }
        public Vertex To { get { return _to; } }
        public int Cost { get => _cost; set => _cost = value; }

        public override string ToString()
        {
            return $"{From} -> {To}";
        }
    }
}
