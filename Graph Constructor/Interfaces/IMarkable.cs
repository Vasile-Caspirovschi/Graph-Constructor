using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph_Constructor.Interfaces
{
    public interface IMarkable
    {
        bool IsVertex();
        bool IsWeightedEdge();
    }
}
