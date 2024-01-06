using Graph_Constructor.Algorithms;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using Petzold.Media2D;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace Graph_Constructor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        static int _vertexId = 0;
        bool _isWeightedGraph = false;
        bool _isDirectedGraph = true;
        public ObservableCollection<Vertex> Vertices { get; set; }
        public ObservableCollection<Edge> Edges { get; set; }
        private ObservableCollection<ObservableCollection<MatrixCellValue>> _matrix;
        public ObservableCollection<ObservableCollection<MatrixCellValue>> Matrix { get => _matrix; set { _matrix = value; OnPropertyChanged("Matrix"); } }
        public ObservableCollection<ObservableCollection<MatrixCellValue>> AdjList { get; set; }
        public bool IsWeightedGraph { get => _isWeightedGraph; set { _isWeightedGraph = value; OnPropertyChanged("IsWeightedGraph"); } }

        public bool IsDirectedGraph { get => _isDirectedGraph; set { _isDirectedGraph = value; OnPropertyChanged("IsDirectedGraph"); } }

        private Graph _graph;
        private Grid? _previousSelectedVertex;
        private Grid? _currentSelectedVertex;
        private Line _tempLineOnMouseMove;
        private bool _wasAlgoRunned;
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _graph = new Graph(IsWeightedGraph);
            Vertices = new ObservableCollection<Vertex>(_graph.GetAllVertices().ToList());
            Edges = new ObservableCollection<Edge>(_graph.GetAllEdges().ToList());
            AdjList = new ObservableCollection<ObservableCollection<MatrixCellValue>>();
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
                {
                    if (IsWeightedGraph)
                        DrawingHelpers.UpdateOutWeightEdgeLocationOnVertexMoving(DrawingArea, mousePos, edge);
                    else
                        DrawingHelpers.UpdateOutEdgeLocationOnVertexMoving(mousePos, edge);
                }
                foreach (var edge in adjacentEdgesIn)
                {
                    if (IsWeightedGraph)
                        DrawingHelpers.UpdateInWeightEdgeLocationOnVertexMoving(DrawingArea, mousePos, edge);
                    else
                        DrawingHelpers.UpdateInEdgeLocationOnVertexMoving(mousePos, edge);
                }
            }
            if (MoveMode.IsChecked == false && _currentSelectedVertex != null && DrawingArea.Children.Count > 2)
                DrawingHelpers.UpdateTemporarDashEdgeOnCanvas(_tempLineOnMouseMove, _currentSelectedVertex, mousePos);
        }

        void RemoveEdge(Vertex start, Vertex end)
        {
            Matrix = AdjacencyMatrixHandler.RemoveEdge(Matrix, Vertices, start, end);
            _graph.RemoveEdge(start, end);
            AdjList[Vertices.IndexOf(start)].Remove(AdjList[Vertices.IndexOf(start)].Where(vertex => vertex.Value == end.Id).First());
        }

        void RemoveWeightedEdge(Vertex start, Vertex end, string weightBlockId)
        {
            Matrix = WeightedMatrixHandler.RemoveEdge(Matrix, Vertices, _graph.GetEdge(start, end));
            AdjList[Vertices.IndexOf(start)].Remove(AdjList[Vertices.IndexOf(start)].Where(vertex => vertex.Value == end.Id).First());
            _graph.RemoveEdge(start, end);
            DrawingArea.Children.Remove(DrawingArea.Children.OfType<TextBlock>().Where(x => x.Tag.ToString() == weightBlockId).First());
        }
        private void DeleteGraphElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ArrowLine)
            {
                ArrowLine graphEdge = e.OriginalSource as ArrowLine;
                string[] extremities = graphEdge.Tag.ToString().Split(' ');
                Vertex start = Vertices.Where(vertex => vertex.Id == int.Parse(extremities[0])).First();
                Vertex end = Vertices.Where(vertex => vertex.Id == int.Parse(extremities[1])).First();
                if (_isWeightedGraph)
                    RemoveWeightedEdge(start, end, graphEdge.Tag.ToString());
                else
                    RemoveEdge(start, end);
                DrawingArea.Children.Remove(graphEdge);
            }
            else if ((_currentSelectedVertex = SelectedVertex(e)) != null)
            {
                Vertex vertexToRemove = Vertices.Where(vertex => vertex.Id.ToString() == DrawingHelpers.GetTextFromVertex(_currentSelectedVertex)).First();
                _graph.RemoveVertex(vertexToRemove);
                Matrix = AdjacencyMatrixHandler.RemoveVertex(Matrix, Vertices, vertexToRemove);

                //bad code
                Vertices.Clear();
                List<Vertex> updatedVertices = _graph.GetAllVertices().OrderBy(vertex => vertex.Id).ToList();
                foreach (var item in updatedVertices)
                    Vertices.Add(item);

                AdjList.Clear();
                foreach (var vertex in Vertices)
                {
                    var temp = new ObservableCollection<MatrixCellValue>
                    {
                        new MatrixCellValue(0)
                    };
                    if (_graph.AdjacencyList[vertex].Count != 0)
                        foreach (var edge in _graph.AdjacencyList[vertex])
                            temp.Insert(0, new MatrixCellValue(edge.To.Id));
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
                AdjList[Vertices.IndexOf(start)].Insert(0, new MatrixCellValue(end.Id));
                if (!DrawingHelpers.CheckForOppositeEdge(DrawingArea, _previousSelectedVertex, _currentSelectedVertex))
                {
                    if (IsWeightedGraph)
                    {
                        _graph.AddEdge(start, end);
                        Matrix = WeightedMatrixHandler.AddEdge(Matrix, Vertices, _graph.GetEdge(start, end));
                        DrawingHelpers.DrawWeightedEdgeOnCanvas(DrawingArea, _previousSelectedVertex, _currentSelectedVertex, Matrix[Vertices.IndexOf(start)][Vertices.IndexOf(end)].Value.ToString());
                    }
                    else
                    {
                        _graph.AddEdge(start, end);
                        Matrix = AdjacencyMatrixHandler.AddEdge(Matrix, Vertices, start, end);
                        DrawingHelpers.DrawEdgeOnCanvas(DrawingArea, _previousSelectedVertex, _currentSelectedVertex);
                    }
                }
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
            if (IsWeightedGraph)
                Matrix = WeightedMatrixHandler.AddVertex(Matrix, Vertices.Count);
            else
                Matrix = AdjacencyMatrixHandler.AddVertex(Matrix, Vertices.Count);
            var temp = new ObservableCollection<MatrixCellValue>
            {
                new MatrixCellValue(0)
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
            //Keyboard.ClearFocus();
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

        private async void RunAlgorithm_Click(object sender, RoutedEventArgs e)
        {
            int start = 0;
            if (!(int.TryParse(StartVertex.Text, out start) && start > 0))
                return;
            var selectedAlgorithm = AlgorithmsPanel.Children.OfType<RadioButton>()
                 .First(r => r.IsChecked.HasValue && r.IsChecked.Value);
            Algorithm algorithm = selectedAlgorithm.Tag switch
            {
                "DFS" => new DFS(_graph, DrawingArea, _graph.GetVertexById(start)),
                "BFS" => new BFS(_graph, DrawingArea, _graph.GetVertexById(start)),
                "Ford" => new Ford(_graph, DrawingArea, _graph.GetVertexById(1), _graph.GetVertexById(start)),
                "BellmanCalaba" => new BellmanCalaba(_graph, DrawingArea, _graph.GetVertexById(1), _graph.GetVertexById(start)),
                "FordFulkersson" => new FordFulkersson(_graph, DrawingArea, _graph.GetVertexById(1), _graph.GetVertexById(start)),
                _ => null!
            };

            BindingOperations.SetBinding(algorithm, Algorithm.ExecutionSpeedProperty, new Binding("Value")
            {
                Source = ExecutionSpeedSlider,
                Mode = BindingMode.OneWay
            });

            algorithm.BindViewProperties(BellmanAlgoResultsMatrix, BellmanResultsVerticalHeader);
            await algorithm.Execute();
            AlgoLogs.Children.Add(algorithm.GetResults().GetLog());
            _wasAlgoRunned = true;
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_wasAlgoRunned)
            {
                DrawingHelpers.ClearCanvasFromAnimationEffects(DrawingArea);
                DrawingHelpers.ClearEdgesFlow(DrawingArea, _graph);
                _wasAlgoRunned = false;
                BellmanAlgoResultsMatrix.ItemsSource = null;
                BellmanAlgoResultsMatrix.Items.Clear();
                BellmanResultsVerticalHeader.ItemsSource = null;
                BellmanResultsVerticalHeader.Items.Clear();
            }
            else
            {
                DrawingArea.Children.Clear();
                _graph = new Graph(IsWeightedGraph);
                Matrix.Clear();
                Vertices.Clear();
                AdjList.Clear();
                GraphTypePopup.Visibility = Visibility.Visible;
                GraphTypePopupBlurEffect.Effect = new BlurEffect()
                {
                    KernelType = KernelType.Gaussian,
                    Radius = 5
                };
                DrawingArea.IsEnabled = false;
                _vertexId = 0;
            }

        }

        private void SetCanvasType_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Name == "WeightedGraph")
            {
                IsDirectedGraph = false;
                IsWeightedGraph = true;
            }
            else
                IsWeightedGraph = false;
            if (IsWeightedGraph)
                Matrix = WeightedMatrixHandler.CreateWeightedMatrix(Vertices.ToList(), Edges.ToList());
            else
                Matrix = AdjacencyMatrixHandler.CreateAdjacencyMatrix(Vertices.ToList(), Edges.ToList());
            GraphTypePopup.Visibility = Visibility.Collapsed;
            GraphTypePopupBlurEffect.Effect = null;
            DrawingArea.IsEnabled = true;
        }
        void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        readonly Regex regex = new Regex("[^0-9]+$");
        private void Cell_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Cell_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.CaptureMouse();
            string[] vertices = textBox.Tag.ToString().Split(" ");
            _previousSelectedVertex = DrawingHelpers.FindVertexOnCanvas(DrawingArea, vertices[0]);
            _currentSelectedVertex = DrawingHelpers.FindVertexOnCanvas(DrawingArea, vertices[1]);
            DrawingHelpers.HighlightSelection(_currentSelectedVertex);
            DrawingHelpers.HighlightSelection(_previousSelectedVertex);
            if (IsDirectedGraph)
                textBox.IsReadOnly = true;
        }

        private void Cell_GotMouseCapture(object sender, MouseEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }

        private void Cell_IsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.SelectAll();
        }

        private void Cell_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.IsFocused)
            {
                BindingExpression bindingExpression = textBox.GetBindingExpression(TextBox.TextProperty);
                bindingExpression.UpdateSource();
                TextBlock weightBlock = DrawingArea.Children.OfType<TextBlock>().Where(x => x.Tag.ToString() == textBox.Tag.ToString()).FirstOrDefault();
                Vertex start = _graph.GetVertexById(int.Parse(DrawingHelpers.GetTextFromVertex(_previousSelectedVertex)));
                Vertex end = _graph.GetVertexById(int.Parse(DrawingHelpers.GetTextFromVertex(_currentSelectedVertex)));
                if (textBox.Text == "∞" && weightBlock != null)
                {
                    ArrowLine edge = DrawingArea.Children.OfType<ArrowLine>().Where(x => x.Tag.ToString() == new string($"{start.Id} {end.Id}")).First();
                    RemoveWeightedEdge(start, end, textBox.Tag.ToString());
                    DrawingArea.Children.Remove(edge);
                }
                if (textBox.Text != "-" && textBox.Text != "∞")
                {
                    if (_graph.GetEdge(start, end) != null)
                        _graph.UpdateEdgeWeight(start, end, int.Parse(textBox.Text));
                    else
                    {
                        AdjList[Vertices.IndexOf(start)].Insert(0, new MatrixCellValue(end.Id));
                        _graph.AddEdge(start, end, int.Parse(textBox.Text));
                    }
                    DrawingHelpers.UpdateWeightOnCanvas(DrawingArea, textBox.Tag.ToString(), textBox.Text);
                }
            }
        }

        private void Cell_LostFocus(object sender, RoutedEventArgs e)
        {
            DrawingHelpers.UnHighlightSelection(_currentSelectedVertex);
            DrawingHelpers.UnHighlightSelection(_previousSelectedVertex);
        }
    }
}
