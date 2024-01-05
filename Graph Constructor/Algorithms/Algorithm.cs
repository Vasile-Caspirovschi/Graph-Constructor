using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    public abstract class Algorithm
    {
        protected readonly Graph graph = default!;
        protected readonly Canvas drawingArea = default!;
        protected readonly Vertex start = default!;
        protected readonly Vertex? target;

   
        protected Algorithm(Graph graph, Canvas drawingArea, Vertex start, Vertex? target)
        {
            this.graph = graph;
            this.drawingArea = drawingArea;
            this.start = start;
            this.target = target;
        }

        public abstract Task Execute();
        public abstract AlgoLog GetResults();
        public abstract void BindViewProperties(params Control[] viewControls);
    }
}
