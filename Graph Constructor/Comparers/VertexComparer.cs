using Graph_Constructor.Models;
using System.Collections.Generic;

namespace Graph_Constructor.Comparers
{
    public class VertexComparer : IComparer<Vertex>
    {
        public int Compare(Vertex? x, Vertex? y)
        {
            return x.Id.CompareTo(y.Id);
        }
    }
}
