using Graph_Constructor.Algorithms;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using Petzold.Media2D;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Graph_Constructor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private static int _vertexId = 0;
        public ObservableCollection<Vertex> Vertices { get; set; }
        public ObservableCollection<Edge> Edges { get; set; }
        public ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> AdjMatrix { get; set; }
        public ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> AdjList { get; set; }

        private Graph _graph;
        private Grid? _previousSelectedVertex;
        private Grid? _currentSelectedVertex;
        private Line _tempLineOnMouseMove;
        private bool _wasAlgoRunned;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _graph = new Graph();
            Vertices = new ObservableCollection<Vertex>(_graph.GetAllVertices().ToList());
            Edges = new ObservableCollection<Edge>(_graph.GetAllEdges().ToList());
            AdjMatrix = AdjacencyMatrixHandler.CreateAdjacencyMatrix(Vertices.ToList(), Edges.ToList());
            AdjList = new ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>>();
        }

        private void DrawGraphElement_Click(object sender, MouseButtonEventArgs e)
        {
            _currentSelectedVertex = SelectedVertex(e);
            if (MoveMode.IsChecked == true)
                return;
            if (_currentSelectedVertex != null)
            {
                if (!DrawingArea.Children.Contains(_tempLineOnMouseMove))
                    DrawingArea.Children.Add(_tempLineOnMouseMove = new Line());

                if (Equals(_currentSelectedVertex, _previousSelectedVertex)) return;
                DrawingHelpers.UnHighlightSelection(_previousSelectedVertex);
                DrawingHelpers.HighlightSelection(_currentSelectedVertex);

                if (_previousSelectedVertex != null)
                {
                    DrawEdge();
                    DrawingArea.Children.Remove(_tempLineOnMouseMove);
                    return;
                }
                _previousSelectedVertex = _currentSelectedVertex;
                return;
            }
            UnselectAll();
            DrawVertex();
        }

        private void MoveVertexOnCanvas(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(DrawingArea);

            if (Mouse.LeftButton == MouseButtonState.Pressed
                && MoveMode.IsChecked == true && _currentSelectedVertex != null)
            {
                var allVertices = DrawingArea.Children.OfType<Grid>().Where(vertex => vertex.Children.OfType<TextBlock>().Single().Text != DrawingHelpers.GetTextFromVertex(_currentSelectedVertex));
                foreach (var vertex in allVertices)
                {
                    if (DrawingHelpers.IsVertexCollision(mousePos, new Point(Canvas.GetLeft(vertex) - 15, Canvas.GetTop(vertex) - 15)))
                    {
                        _currentSelectedVertex = null;
                        return;
                    }
                }
                Canvas.SetLeft(_currentSelectedVertex, mousePos.X - 15);
                Canvas.SetTop(_currentSelectedVertex, mousePos.Y - 15);

                var adjacentEdgesOut = DrawingArea.Children.OfType<ArrowLine>().Where(edge => edge.Tag.ToString().Split(' ')[0] == DrawingHelpers.GetTextFromVertex(_currentSelectedVertex));
                var adjacentEdgesIn = DrawingArea.Children.OfType<ArrowLine>().Where(edge => edge.Tag.ToString().Split(' ')[1] == DrawingHelpers.GetTextFromVertex(_currentSelectedVertex));
                foreach (var edge in adjacentEdgesOut)
                    DrawingHelpers.UpdateOutEdgeLocationOnVertexMoving(mousePos, edge);
                foreach (var edge in adjacentEdgesIn)
                    DrawingHelpers.UpdateInEdgeLocationOnVertexMoving(mousePos, edge);
            }

            if (MoveMode.IsChecked == false && _currentSelectedVertex != null && DrawingArea.Children.Count > 2)
                DrawingHelpers.UpdateTemporarDashEdgeOnCanvas(_tempLineOnMouseMove, _currentSelectedVertex, mousePos);
        }

        private void DeleteGraphElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ArrowLine)
            {
                ArrowLine graphEdge = e.OriginalSource as ArrowLine;
                string[] extremities = graphEdge.Tag.ToString().Split(' ');
                Vertex start = Vertices.Where(vertex => vertex.Id == int.Parse(extremities[0])).First();
                Vertex end = Vertices.Where(vertex => vertex.Id == int.Parse(extremities[1])).First();

                _graph.RemoveEdge(start, end);
                AdjMatrix = AdjacencyMatrixHandler.RemoveEdge(AdjMatrix, Vertices, start, end);
                AdjList[Vertices.IndexOf(start)].Remove(AdjList[Vertices.IndexOf(start)].Where(vertex => vertex.Value == end.Id).First());
                DrawingArea.Children.Remove(graphEdge);
            }
            else if ((_currentSelectedVertex = SelectedVertex(e)) != null)
            {
                Vertex vertexToRemove = Vertices.Where(vertex => vertex.Id.ToString() == DrawingHelpers.GetTextFromVertex(_currentSelectedVertex)).First();
                _graph.RemoveVertex(vertexToRemove);
                AdjMatrix = AdjacencyMatrixHandler.RemoveVertex(AdjMatrix, Vertices, vertexToRemove);
                
                //bad code
                Vertices.Clear();
                List<Vertex> updatedVertices = _graph.GetAllVertices().OrderBy(vertex => vertex.Id).ToList();
                foreach (var item in updatedVertices)
                    Vertices.Add(item);

                AdjList.Clear();
                foreach (var vertex in Vertices)
                {
                    var temp = new ObservableCollection<AdjacencyMatrixCellValue>
                    {
                        new AdjacencyMatrixCellValue(0)
                    };
                    if (_graph.AdjacencyList[vertex].Count != 0)
                        foreach (var edge in _graph.AdjacencyList[vertex])
                            temp.Insert(0, new AdjacencyMatrixCellValue(edge.To.Id));
                    AdjList.Add(temp);
                }

                DrawingArea.Children.Remove(_currentSelectedVertex);
                DrawingHelpers.RemoveIncidentEdgesOfVertex(DrawingArea, vertexToRemove);
                DrawingHelpers.UpdateVertexIdAfterRemoving(DrawingArea, vertexToRemove.Id);
                _vertexId--;
            }
        }

        private Grid SelectedVertex(MouseButtonEventArgs e)
        {
            Ellipse ellipse = new Ellipse();
            TextBlock textBlock = new TextBlock();
            if (e.OriginalSource is Ellipse)
                ellipse = e.OriginalSource as Ellipse;

            if (e.OriginalSource is TextBlock)
                textBlock = e.OriginalSource as TextBlock;

            if (ellipse.Parent is Grid)
                return ellipse.Parent as Grid;

            if (e.Source is TextBlock)
                return textBlock.Parent as Grid;
            return null;
        }

        private void DrawEdge()
        {
            Vertex start = Vertices.Where(vertex => vertex.Id == int.Parse(DrawingHelpers.GetTextFromVertex(_previousSelectedVertex))).First();
            Vertex end = Vertices.Where(vertex => vertex.Id == int.Parse(DrawingHelpers.GetTextFromVertex(_currentSelectedVertex))).First();

            if (!DrawingHelpers.CheckIfEdgeExist(DrawingArea, _previousSelectedVertex, _currentSelectedVertex))
            {
                _graph.AddEdge(start, end);
                AdjMatrix = AdjacencyMatrixHandler.AddEdge(AdjMatrix, Vertices, start, end);
                AdjList[Vertices.IndexOf(start)].Insert(0, new AdjacencyMatrixCellValue(end.Id));
                if (!DrawingHelpers.CheckForOppositeEdge(DrawingArea, _previousSelectedVertex, _currentSelectedVertex))
                    DrawingHelpers.DrawEdgeOnCanvas(DrawingArea, _previousSelectedVertex, _currentSelectedVertex);
                _previousSelectedVertex = null;
                _currentSelectedVertex = null;
            }
        }

        private void DrawVertex()
        {
            Point mouseLocation = Mouse.GetPosition(DrawingArea);
            var allVertices = DrawingArea.Children.OfType<Grid>().ToList();

            foreach (var vertex in allVertices)
                if (DrawingHelpers.IsVertexCollision(mouseLocation, new Point(Canvas.GetLeft(vertex) - 15, Canvas.GetTop(vertex) - 15)))
                    return;

            Vertex newVertex = new Vertex(++_vertexId, mouseLocation);
            _graph.AddVertex(newVertex);
            Vertices.Add(newVertex);
            AdjMatrix = AdjacencyMatrixHandler.AddVertex(AdjMatrix, Vertices.Count);
            var temp = new ObservableCollection<AdjacencyMatrixCellValue>
            {
                new AdjacencyMatrixCellValue(0)
            };
            AdjList.Add(temp);

            DrawingHelpers.DrawVertexOnCanvas(DrawingArea, _vertexId.ToString(), newVertex.Location);
            _previousSelectedVertex = null;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Vertices.Count() > 0)
                Vertices.Remove(Vertices.First());
        }

        private void DrawingArea_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                UnselectAll();
        }

        private void UnselectAll()
        {
            DrawingHelpers.UnHighlightSelection(_previousSelectedVertex);
            DrawingHelpers.UnHighlightSelection(_currentSelectedVertex);
            if (DrawingArea.Children.Contains(_tempLineOnMouseMove))
                DrawingArea.Children.Remove(_tempLineOnMouseMove);
            _currentSelectedVertex = null;
            _previousSelectedVertex = null;
        }

        private void RunAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            int start = 0;
            if (!(int.TryParse(StartVertex.Text, out start) && start > 0))
                return;
            if (RunDFS.IsChecked == true)
            {
                DFS dfs = new DFS(_graph, DrawingArea, _graph.GetVertexById(start));
                AlgoLog log = new AlgoLog($"DFS from {start}", dfs.Path);
                AlgoLogs.Text += log.ToString();
                _wasAlgoRunned = true;
            }
            if (RunBFS.IsChecked == true)
            {
                BFS bfs = new BFS(_graph, DrawingArea, _graph.GetVertexById(start));
                AlgoLog log = new AlgoLog($"BFS from {start}", bfs.Path);
                AlgoLogs.Text += log.ToString();
                _wasAlgoRunned = true;
            }
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_wasAlgoRunned)
            {
                DrawingHelpers.ClearCanvasFromAnimationEffects(DrawingArea);
                _wasAlgoRunned = false;
            }
            else
            {
                DrawingArea.Children.Clear();
                _graph = new Graph();
                AdjMatrix.Clear();
                Vertices.Clear();
                AdjList.Clear();
                _vertexId = 0;
            }

        }


    }
}
