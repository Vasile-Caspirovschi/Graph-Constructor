using Graph_Constructor.Interfaces;
using System.Drawing;

namespace Graph_Constructor.Models
{
    public class AlgorithmStep 
    {
        public IMarkable MarkedElement { get; set; }
        public Color MarkedColor { get; set; }
        public Color CurrentColor { get; set; }
        public AlgorithmStep(IMarkable markedElement, Color markedColor, Color currentColor)
        {
            MarkedElement = markedElement;
            MarkedColor = markedColor;
            CurrentColor = currentColor;
        }

        public bool IsVertex()
        {
            return MarkedElement is Vertex;
        }
    }
}
