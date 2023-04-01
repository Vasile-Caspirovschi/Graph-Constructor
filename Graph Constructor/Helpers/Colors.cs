using System;
using System.Drawing;

namespace Graph_Constructor.Helpers
{
    public static class Colors
    {
        public static Color
            DefaultVertexColor = ColorTranslator.FromHtml("#eee489"),
            DefaultEdgeColor = ColorTranslator.FromHtml("#b12e25"),
            VisitedEdge = ColorTranslator.FromHtml("#edb922"),
            VisitedVertex = ColorTranslator.FromHtml("#b12e25"),
            DoneVertex = ColorTranslator.FromHtml("#279fdb"),
            HighligthedVertex = ColorTranslator.FromHtml("#249543"),
            EdgeWeightColor = ColorTranslator.FromHtml("#1f54a1");

        private static readonly Random rnd = new Random();
        public static Color GetRandom() => Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
        public static System.Windows.Media.Brush ToBrush(this Color color)
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
