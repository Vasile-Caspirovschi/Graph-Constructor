using Graph_Constructor.Enums;
using Graph_Constructor.Models;
using Petzold.Media2D;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Graph_Constructor.Helpers
{
    public static class DrawingHelpers
    {
        const int DIAMETER = 30;

        public static void DrawVertexOnCanvas(Canvas canvas, string title, Point location)
        {
            Grid grid = new Grid();
            Ellipse circle = new Ellipse()
            {
                Stroke = Brushes.Black,
                Width = DIAMETER,
                Height = DIAMETER,
                StrokeThickness = 2,
                Fill = Colors.ToBrush(Colors.DefaultVertexColor),
            };

            TextBlock vertexText = new TextBlock()
            {
                Text = title,
                Foreground = Brushes.Black,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Width = Double.NaN,
                Height = Double.NaN,
            };

            grid.Children.Add(circle);
            grid.Children.Add(vertexText);

            Canvas.SetLeft(grid, location.X);
            Canvas.SetTop(grid, location.Y);

            canvas.Children.Add(grid);
        }

        public static void ClearCanvasFromAnimationEffects(Canvas canvas)
        {
            foreach (var vertex in canvas.Children.OfType<Grid>())
                vertex.Children.OfType<Ellipse>().First().Fill = Colors.ToBrush(Colors.DefaultVertexColor);
            foreach (var vertex in canvas.Children.OfType<ArrowLine>())
                vertex.Stroke = Colors.ToBrush(Colors.DefaultEdgeColor);
        }

        public static void UpdateVertexIdAfterRemoving(Canvas canvas, int removedId)
        {
            //update vertex id
            var verticesToUpdate = canvas.Children.OfType<Grid>().Where(grid => int.Parse(grid.Children.OfType<TextBlock>().First().Text) > removedId);
            foreach (var vertex in verticesToUpdate)
            {
                var textBox = vertex.Children.OfType<TextBlock>().First();
                int id = int.Parse(textBox.Text);
                textBox.Text = (--id).ToString();
            }

            //update edge tag name
            foreach (var edge in canvas.Children.OfType<ArrowLine>())
            {
                var extremities = edge.Tag.ToString().Split(' ').Select(extremety => int.Parse(extremety)).ToArray();
                if (extremities[0] > removedId)
                    extremities[0] -= 1;
                if (extremities[1] > removedId)
                    extremities[1] -= 1;
                edge.Tag = string.Join(" ", extremities);
            }
        }

        public static void RemoveIncidentEdgesOfVertex(Canvas canvas, Vertex vertexToRemove)
        {
            for (int i = canvas.Children.Count - 1; i >= 0; i += -1)
            {
                UIElement child = canvas.Children[i];
                if (!(child is ArrowLine))
                    continue;
                ArrowLine edge = child as ArrowLine;
                if (edge.Tag.ToString().Split(' ')[0] == vertexToRemove.Id.ToString()
                || edge.Tag.ToString().Split(' ')[1] == vertexToRemove.Id.ToString())
                    canvas.Children.Remove(edge);
            }
        }

        public static void UpdateTemporarDashEdgeOnCanvas(Line line, Grid startVertex, Point endPos)
        {
            if (line != null)
            {
                Point centerStart = new Point()
                {
                    X = Canvas.GetLeft(startVertex) + 15,
                    Y = Canvas.GetTop(startVertex) + 15
                };

                Point start = GetPointOnCircumference(centerStart, DIAMETER / 2, GetAngleBetweenTwoPoints(centerStart, endPos));
                Point end = GetPointOnCircumference(endPos, 5, GetAngleBetweenTwoPoints(endPos, start));
                line.X1 = start.X;
                line.Y1 = start.Y;
                line.X2 = end.X;
                line.Y2 = end.Y;
                line.Stroke = Colors.ToBrush(Colors.DefaultEdgeColor);
                line.StrokeThickness = 3;
                line.StrokeDashArray = new DoubleCollection() { 5, 2 };
            }
        }

        public static bool CheckForOppositeEdge(Canvas canvas, Grid startVertex, Grid endVertex)
        {
            string name = $"{GetTextFromVertex(endVertex)} {GetTextFromVertex(startVertex)}";
            var edges = canvas.Children.OfType<ArrowLine>().Where(edge => edge.Tag.ToString() == name).ToList();
            if (edges.Count > 0)
            {
                edges[0].ArrowEnds = ArrowEnds.Both;
                return true;
            }
            return false;

        }

        public static bool CheckIfEdgeExist(Canvas canvas, Grid startVertex, Grid endVertex)
        {
            string name = $"{GetTextFromVertex(startVertex)} {GetTextFromVertex(endVertex)}";
            var edges = canvas.Children.OfType<ArrowLine>().Where(edge => edge.Tag.ToString() == name).ToList();
            if (edges.Count > 0)
                return true;
            return false;
        }

        static ArrowLine CreateEdge(Grid startVertex, Grid endVertex, GraphType graphType = GraphType.Directed)
        {
            Point centerStart = new Point()
            {
                X = Canvas.GetLeft(startVertex),
                Y = Canvas.GetTop(startVertex)
            };
            Point centerEnd = new Point()
            {
                X = Canvas.GetLeft(endVertex),
                Y = Canvas.GetTop(endVertex)
            };

            Point start = GetPointOnCircumference(centerStart, DIAMETER / 2, GetAngleBetweenTwoPoints(centerStart, centerEnd));
            Point end = GetPointOnCircumference(centerEnd, DIAMETER / 2, GetAngleBetweenTwoPoints(centerEnd, centerStart));
            ArrowLine edge = new ArrowLine()
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = Colors.ToBrush(Colors.DefaultEdgeColor),
                StrokeThickness = 3,
                IsArrowClosed = true,
                ArrowEnds = graphType == GraphType.Undirected ? ArrowEnds.None : ArrowEnds.End,
                Tag = GetNameForEdge(startVertex, endVertex),
            };
            return edge;
        }

        public static void DrawEdgeOnCanvas(Canvas canvas, Grid startVertex, Grid endVertex, GraphType graphType)
        {
            ArrowLine edge = CreateEdge(startVertex, endVertex, graphType);
            Canvas.SetLeft(edge, 15);
            Canvas.SetTop(edge, 15);
            canvas.Children.Add(edge);
            UnHighlightSelection(startVertex);
            UnHighlightSelection(endVertex);
        }
        public static void DrawEdgeOnCanvas(Canvas canvas, ArrowLine edge)
        {
            Canvas.SetLeft(edge, 15);
            Canvas.SetTop(edge, 15);
            canvas.Children.Add(edge);
        }

        public static void DrawWeightedEdgeOnCanvas(Canvas canvas, Grid startVertex,
            Grid endVertex, string weight)
        {
            ArrowLine edge = CreateEdge(startVertex, endVertex);
            DrawEdgeOnCanvas(canvas, edge);
            TextBlock weightBlock = new TextBlock()
            {
                Text = weight,
                Tag = edge.Tag,
                Foreground = Colors.ToBrush(Colors.EdgeWeightColor),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                Width = Double.NaN,
                Height = Double.NaN,
            };
            Point midlleOfEdge = GetMidPointOfLine(edge);
            Canvas.SetLeft(weightBlock, midlleOfEdge.X);
            Canvas.SetTop(weightBlock, midlleOfEdge.Y);
            canvas.Children.Add(weightBlock);
            UnHighlightSelection(startVertex);
            UnHighlightSelection(endVertex);
        }

        public static void UpdateOutEdgeLocationOnVertexMoving(Point newStartCenter, ArrowLine line)
        {
            Point endOfLine = new Point()
            {
                X = line.X2,
                Y = line.Y2
            };
            newStartCenter.X = newStartCenter.X - DIAMETER / 2;
            newStartCenter.Y = newStartCenter.Y - DIAMETER / 2;
            Point endCenter = GetPointOfCenter(endOfLine, DIAMETER / 2, GetAngleBetweenTwoPoints(endOfLine, new Point(line.X1, line.Y1)));

            Point newEnd = GetPointOnCircumference(endCenter, DIAMETER / 2, GetAngleBetweenTwoPoints(endCenter, newStartCenter));
            Point newStart = GetPointOnCircumference(newStartCenter, DIAMETER / 2, GetAngleBetweenTwoPoints(newStartCenter, endCenter));
            line.X1 = newStart.X;
            line.Y1 = newStart.Y;
            line.X2 = newEnd.X;
            line.Y2 = newEnd.Y;
        }

        public static void UpdateOutWeightEdgeLocationOnVertexMoving(Canvas canvas, Point newStartCenter, ArrowLine edge)
        {
            UpdateOutEdgeLocationOnVertexMoving(newStartCenter, edge);
            UpdateEdgeWeightLocationOnVertexMoving(GetMidPointOfLine(edge), canvas.Children.OfType<TextBlock>().Where(x => x.Tag == edge.Tag).First());
        }

        public static void UpdateInWeightEdgeLocationOnVertexMoving(Canvas canvas, Point newStartCenter, ArrowLine edge)
        {
            UpdateInEdgeLocationOnVertexMoving(newStartCenter, edge);
            UpdateEdgeWeightLocationOnVertexMoving(GetMidPointOfLine(edge), canvas.Children.OfType<TextBlock>().Where(x => x.Tag.ToString() == edge.Tag.ToString()).First());
        }

        static void UpdateEdgeWeightLocationOnVertexMoving(Point newStart, TextBlock weightBlock)
        {
            Canvas.SetLeft(weightBlock, newStart.X);
            Canvas.SetTop(weightBlock, newStart.Y);
        }

        public static void UpdateInEdgeLocationOnVertexMoving(Point newEndCenter, ArrowLine line)
        {
            Point startOfLine = new Point()
            {
                X = line.X1,
                Y = line.Y1
            };
            newEndCenter.X = newEndCenter.X - DIAMETER / 2;
            newEndCenter.Y = newEndCenter.Y - DIAMETER / 2;
            Point startCenter = GetPointOfCenter(startOfLine, DIAMETER / 2, GetAngleBetweenTwoPoints(startOfLine, new Point(line.X2, line.Y2)));

            Point newEnd = GetPointOnCircumference(newEndCenter, DIAMETER / 2, GetAngleBetweenTwoPoints(newEndCenter, startCenter));
            Point newStart = GetPointOnCircumference(startCenter, DIAMETER / 2, GetAngleBetweenTwoPoints(startCenter, newEndCenter));
            line.X1 = newStart.X;
            line.Y1 = newStart.Y;
            line.X2 = newEnd.X;
            line.Y2 = newEnd.Y;
        }

        public static Grid FindVertexOnCanvas(Canvas canvas, string text)
        {
            return canvas.Children.OfType<Grid>().Where(grid => GetTextFromVertex(grid) == text).Single();
        }

        private static string GetNameForEdge(Grid startVertex, Grid endVertex)
        {
            return $"{GetTextFromVertex(startVertex)} {GetTextFromVertex(endVertex)}";
        }

        public static string GetTextFromVertex(Grid vertex)
        {
            return vertex.Children.OfType<TextBlock>().Single().Text;
        }

        public static void UpdateEdgeFlow(Canvas canvas, string identifier, Edge edge, int flow)
        {
            var block = canvas.Children.OfType<TextBlock>().Where(block => block.Tag.ToString() == identifier).First();
            block.Text = $"{edge.Cost} ({flow})";
            if (edge.Cost == flow)
                block.Foreground = Colors.ToBrush(Colors.VisitedEdge);
        }

        public static void ClearEdgesFlow(Canvas canvas, Graph graph)
        {
            string[] name;
            foreach (var block in canvas.Children.OfType<TextBlock>())
            {
                name = block.Tag.ToString().Split(' ');
                Vertex start = graph.GetVertexById(int.Parse(name[0]));
                Vertex end = graph.GetVertexById(int.Parse(name[1]));
                block.Text = graph.GetEdge(start, end).Cost.ToString();
                block.Foreground = Colors.ToBrush(Colors.EdgeWeightColor);
            }
        }

        public static void HighlightSelection(Grid selectedVertex)
        {
            if (selectedVertex != null)
            {
                Ellipse ellipse = selectedVertex.Children.OfType<Ellipse>().FirstOrDefault();

                ellipse.Fill = Colors.ToBrush(Colors.HighligthedVertex);
            }
        }

        public static void UpdateWeightOnCanvas(Canvas canvas, string weightId, string newWeight)
        {
            TextBlock? weightBlock = canvas.Children.OfType<TextBlock>()
                .Where(x => x.Tag.ToString() == weightId || x.Tag.ToString() == weightId.Reverse()).FirstOrDefault();
            if (weightBlock == null && newWeight != "∞")
            {
                string[] vertices = weightId.Split(' ');
                Grid start = FindVertexOnCanvas(canvas, vertices[0]);
                Grid end = FindVertexOnCanvas(canvas, vertices[1]);
                if (!CheckIfEdgeExist(canvas, start, end))
                    DrawWeightedEdgeOnCanvas(canvas, start, end, newWeight);
            }

            if (weightBlock != null)
                weightBlock.Text = newWeight;
        }

        public static void UnHighlightSelection(Grid selectedVertex)
        {
            if (selectedVertex != null)
            {
                Ellipse ellipse = selectedVertex.Children.OfType<Ellipse>().FirstOrDefault();
                ellipse.Fill = Colors.ToBrush(Colors.DefaultVertexColor);
            }
        }

        private static double GetAngleBetweenTwoPoints(Point first, Point second)
        {
            double angle = Math.Atan2(second.Y - first.Y, second.X - first.X);
            return angle;
        }

        static Point GetMidPointOfLine(Point start, Point end)
        {
            return new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);
        }

        static Point GetMidPointOfLine(ArrowLine line)
        {
            return new Point((line.X1 + line.X2) / 2, (line.Y1 + line.Y2) / 2);
        }

        private static Point GetPointOnCircumference(Point center, int radius, double angle)
        {
            Point position = new Point();
            position.X = center.X + radius * Math.Cos(angle);
            position.Y = center.Y + radius * Math.Sin(angle);
            return position;
        }

        private static Point GetPointOfCenter(Point pointOnCircumference, int radius, double angle)
        {
            Point position = new Point();
            position.X = pointOnCircumference.X - radius * Math.Cos(angle);
            position.Y = pointOnCircumference.Y - radius * Math.Sin(angle);
            return position;
        }

        public static bool IsVertexCollision(Point point1, Point point2)
        {
            Point delta = new Point();
            delta.X = point1.X - point2.X - DIAMETER;
            delta.Y = point1.Y - point2.Y - DIAMETER;
            return delta.X * delta.X + delta.Y * delta.Y <= DIAMETER * DIAMETER;
        }

        public static void MarkVertex(Canvas canvas, Vertex vertex, System.Drawing.Color color)
        {
            var vertexOnCanvas = canvas.Children.OfType<Grid>().Where(grid => GetTextFromVertex(grid) == vertex.Id.ToString()).Single();
            vertexOnCanvas.Children.OfType<Ellipse>().Single().Fill = Colors.ToBrush(color);
        }

        public static void MarkEdge(Canvas canvas, Edge edge, System.Drawing.Color color)
        {
            var startVertexOnCanvas = canvas.Children.OfType<Grid>().Where(grid => GetTextFromVertex(grid) == edge.From.Id.ToString()).Single();
            var endVertexOnCanvas = canvas.Children.OfType<Grid>().Where(grid => GetTextFromVertex(grid) == edge.To.Id.ToString()).Single();
            var edgeOnCanvas = canvas.Children.OfType<ArrowLine>().Where(line =>
            line.Tag.ToString() == $"{GetTextFromVertex(startVertexOnCanvas)} {GetTextFromVertex(endVertexOnCanvas)}" ||
            line.Tag.ToString() == $"{GetTextFromVertex(endVertexOnCanvas)} {GetTextFromVertex(startVertexOnCanvas)}").Single();
            edgeOnCanvas.Stroke = Colors.ToBrush(color);
        }
    }
}
