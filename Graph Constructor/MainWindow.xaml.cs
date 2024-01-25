using Ab2d.Controls;
using Graph_Constructor.Algorithms;
using Graph_Constructor.Enums;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using Microsoft.Win32;
using Petzold.Media2D;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        bool _isAnimation = false;

        private bool _isSpaceKeyDown;
        private bool _isZoomModeChanged;
        private bool _isReadFromFile;
        private ZoomPanel.ZoomModeType _savedZoomMode;

        private int _prevValue;
        private string _prevCell;
        private bool _isWeightedGraph;
        private bool _isUndirectedGraph;

        private Graph _graph = null!;
        private Grid? _previousSelectedVertex;
        private Grid? _currentSelectedVertex;
        private Line _tempLineOnMouseMove = null!;
        private bool _wasAlgoRunned;
        private AlgorithmSteps _algorithmSteps = null!;
        private AdjacencyMatrix _matrix = null!;
        private ObservableCollection<Vertex> _vertices = null!;

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<Vertex> Vertices { get => _vertices; set { _vertices = value; OnPropertyChanged("Vertices"); } }
        public ObservableCollection<ObservableCollection<MatrixCellValue>> AdjList { get; set; } = new();
        public AdjacencyMatrix Matrix { get => _matrix; set { _matrix = value; OnPropertyChanged("Matrix"); } }
        public bool IsWeightedGraph
        {
            get => _isWeightedGraph;
            set { _isWeightedGraph = value; OnPropertyChanged(nameof(IsWeightedGraph)); }
        }

        public bool IsUndirectedGraph
        {
            get => _isUndirectedGraph;
            set { _isUndirectedGraph = value; OnPropertyChanged(nameof(IsUndirectedGraph)); }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            this.PreviewKeyDown += new KeyEventHandler(PainterSample_PreviewKeyDown);
            this.PreviewKeyUp += new KeyEventHandler(PainterSample_PreviewKeyUp);
        }

        #region handle zooming shortcuts
        void PainterSample_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            ProcessKeyEvent(e);
        }

        void PainterSample_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeyEvent(e);
        }

        private void ProcessKeyEvent(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                // First handle space
                _isSpaceKeyDown = e.IsDown;

                if (UpdateZoomMode())
                    e.Handled = true;
            }
            else if (_isSpaceKeyDown)
            {
                // While the space is down, the Control os Shift can be pressed or released - check this
                if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                    e.Key == Key.LeftShift || e.Key == Key.RightShift)
                {
                    if (UpdateZoomMode())
                        e.Handled = true;
                }
            }
            else
            {
                // Handle other keys
                switch (e.Key)
                {
                    case Key.Left:
                        if (e.IsDown || e.IsRepeat) // On key up only set handled to true
                            ZoomingPanel.LineLeft(); // As using the scroll bar

                        e.Handled = true;

                        break;

                    case Key.Right:
                        if (e.IsDown || e.IsRepeat)
                            ZoomingPanel.LineRight(); // As using the scroll bar

                        e.Handled = true;

                        break;

                    case Key.Up:
                        if (e.IsDown || e.IsRepeat)
                            ZoomingPanel.LineUp(); // As using the scroll bar

                        e.Handled = true;

                        break;

                    case Key.Down:
                        if (e.IsDown || e.IsRepeat)
                            ZoomingPanel.LineDown(); // As using the scroll bar

                        e.Handled = true;

                        break;

                    case Key.Home:
                        if (e.IsDown || e.IsRepeat)
                            ZoomingPanel.ResetToLimits();

                        e.Handled = true;

                        break;

                    case Key.Insert: // ZoomOut
                    case Key.Subtract: // ZoomOut
                        if (e.IsDown || e.IsRepeat)
                            ZoomingPanel.ZoomForFactor(1 / ZoomingPanel.MouseWheelZoomFactor); // ZoomingPanel.MouseWheelZoomFactor == Zoom In factor

                        e.Handled = true;

                        break;

                    case Key.Delete: // ZoomIn
                    case Key.Add: // ZoomIn
                        if (e.IsDown || e.IsRepeat)
                            ZoomingPanel.ZoomForFactor(ZoomingPanel.MouseWheelZoomFactor);

                        e.Handled = true;
                        break;
                }
            }
        }

        private bool UpdateZoomMode()
        {
            bool isHandled;

            isHandled = false;

            if (_isSpaceKeyDown)
            {
                Ab2d.Controls.ZoomPanel.ZoomModeType newZoomMode;

                if (Keyboard.Modifiers == ModifierKeys.None)
                {
                    newZoomMode = Ab2d.Controls.ZoomPanel.ZoomModeType.Move;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    newZoomMode = Ab2d.Controls.ZoomPanel.ZoomModeType.ZoomIn;
                }
                else if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    newZoomMode = Ab2d.Controls.ZoomPanel.ZoomModeType.ZoomOut;
                }
                else
                {
                    newZoomMode = Ab2d.Controls.ZoomPanel.ZoomModeType.None;
                }

                if (newZoomMode != Ab2d.Controls.ZoomPanel.ZoomModeType.None)
                {
                    ChanageZoomMode(newZoomMode);
                    isHandled = true;
                }
            }
            else
            {
                ResetZoomMode();
                isHandled = true;
            }

            return isHandled;
        }

        private void ChanageZoomMode(Ab2d.Controls.ZoomPanel.ZoomModeType newZoomMode)
        {
            if (ZoomingPanel.ZoomMode != newZoomMode && newZoomMode != Ab2d.Controls.ZoomPanel.ZoomModeType.None)
            {
                if (!_isZoomModeChanged) // if zoom mode was already changed (only modifier - control or shift - was pressed)
                    _savedZoomMode = ZoomingPanel.ZoomMode;

                ZoomingPanel.ZoomMode = newZoomMode;
                _isZoomModeChanged = true;
            }
        }

        private void ResetZoomMode()
        {
            if (_isZoomModeChanged)
            {
                ZoomingPanel.ZoomMode = _savedZoomMode;
                _isZoomModeChanged = false;
            }
        }
        #endregion

        private void DrawGraphElement_Click(object sender, MouseButtonEventArgs e)
        {
            _isReadFromFile = false;
            _currentSelectedVertex = SelectedVertex(e);
            if (MoveMode.IsChecked == true)
                return;
            if (ZoomingPanel.ZoomMode != ZoomPanel.ZoomModeType.None)
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
                    Vertex start = Vertices.Where(vertex => vertex.Id == int.Parse(DrawingHelpers.GetTextFromVertex(_previousSelectedVertex))).FirstOrDefault();
                    Vertex end = Vertices.Where(vertex => vertex.Id == int.Parse(DrawingHelpers.GetTextFromVertex(_currentSelectedVertex))).FirstOrDefault();
                    DrawEdge(start, end);
                    DrawingArea.Children.Remove(_tempLineOnMouseMove);
                    return;
                }
                _previousSelectedVertex = _currentSelectedVertex;
                return;
            }
            UnselectAll();
            var vertex = new Vertex(_graph.CurrentVertexId, Mouse.GetPosition(DrawingArea));
            DrawVertex(vertex);
        }

        private void MoveVertexOnCanvas(object sender, MouseEventArgs e)
        {
            var mousePos = e.GetPosition(DrawingArea);
            if (ZoomingPanel.ZoomMode != ZoomPanel.ZoomModeType.None)
                return;
            if (Mouse.LeftButton == MouseButtonState.Pressed
                && MoveMode.IsChecked == true && _currentSelectedVertex != null)
            {
                var allVertices = DrawingArea.Children.OfType<Grid>().Where(vertex => vertex.Children.OfType<TextBlock>().Single().Text != DrawingHelpers.GetTextFromVertex(_currentSelectedVertex));
                foreach (var vertex in allVertices)
                {
                    if (!DrawingHelpers.IsVertexCollision(mousePos, new Point(Canvas.GetLeft(vertex) - 15, Canvas.GetTop(vertex) - 15)))
                    {
                        Canvas.SetLeft(_currentSelectedVertex, mousePos.X - 15);
                        Canvas.SetTop(_currentSelectedVertex, mousePos.Y - 15);
                        var adjacentEdgesOut = DrawingArea.Children.OfType<ArrowLine>().Where(edge => edge.Tag.ToString().Split(' ')[0] == DrawingHelpers.GetTextFromVertex(_currentSelectedVertex));
                        var adjacentEdgesIn = DrawingArea.Children.OfType<ArrowLine>().Where(edge => edge.Tag.ToString().Split(' ')[1] == DrawingHelpers.GetTextFromVertex(_currentSelectedVertex));
                        foreach (var edge in adjacentEdgesOut)
                        {
                            if (GraphType.Weighted == _graph.GetGraphType)
                                DrawingHelpers.UpdateOutWeightEdgeLocationOnVertexMoving(DrawingArea, mousePos, edge);
                            else
                                DrawingHelpers.UpdateOutEdgeLocationOnVertexMoving(mousePos, edge);
                        }
                        foreach (var edge in adjacentEdgesIn)
                        {
                            if (GraphType.Weighted == _graph.GetGraphType)
                                DrawingHelpers.UpdateInWeightEdgeLocationOnVertexMoving(DrawingArea, mousePos, edge);
                            else
                                DrawingHelpers.UpdateInEdgeLocationOnVertexMoving(mousePos, edge);
                        }
                    }
                }
            }
            if (MoveMode.IsChecked == false && _currentSelectedVertex != null && DrawingArea.Children.Count > 2)
                DrawingHelpers.UpdateTemporarDashEdgeOnCanvas(_tempLineOnMouseMove, _currentSelectedVertex, mousePos);
        }

        void RemoveEdge(Vertex start, Vertex end)
        {
            Matrix.RemoveEdge(Vertices, start, end);
            _graph.RemoveEdge(start, end);
            AdjList[Vertices.IndexOf(start)].Remove(AdjList[Vertices.IndexOf(start)].Where(vertex => vertex.Value == end.Id).FirstOrDefault());
        }

        void RemoveWeightedEdge(Vertex start, Vertex end, string weightBlockId)
        {
            Matrix.RemoveEdge(Vertices, start, end);
            _graph.RemoveEdge(start, end);
            AdjList[Vertices.IndexOf(start)].Remove(AdjList[Vertices.IndexOf(start)].Where(vertex => vertex.Value == end.Id).First());
            var weightBlock = DrawingArea.Children.OfType<TextBlock>().Where(x => x.Tag.ToString() == weightBlockId).First();
            DrawingArea.Children.Remove(weightBlock);
        }
        private void DeleteGraphElement_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is ArrowLine)
            {
                ArrowLine graphEdge = e.OriginalSource as ArrowLine;
                string[] extremities = graphEdge.Tag.ToString()!.Split(' ');
                Vertex start = Vertices.Where(vertex => vertex.Id == int.Parse(extremities[0])).First();
                Vertex end = Vertices.Where(vertex => vertex.Id == int.Parse(extremities[1])).First();
                DeleteEdge(graphEdge, start, end);
            }
            else if ((_currentSelectedVertex = SelectedVertex(e)) != null)
            {
                Vertex vertexToRemove = _graph.GetVertexById(int.Parse(DrawingHelpers.GetTextFromVertex(_currentSelectedVertex)));
                _graph.RemoveVertex(vertexToRemove);
                Matrix.RemoveVertex(vertexToRemove);
                Vertices = new ObservableCollection<Vertex>(_graph.GetAllVertices());

                AdjList.Clear();
                foreach (var vertex in Vertices)
                {
                    var row = new ObservableCollection<MatrixCellValue>
                    {
                        new MatrixCellValue(0)
                    };
                    if (_graph.AdjacencyList[vertex].Count != 0)
                        foreach (var edge in _graph.AdjacencyList[vertex])
                            row.Insert(0, new MatrixCellValue(edge.To.Id));
                    AdjList.Add(row);
                }

                DrawingArea.Children.Remove(_currentSelectedVertex);
                DrawingHelpers.RemoveIncidentEdgesOfVertex(DrawingArea, vertexToRemove, _graph.GetGraphType);
                DrawingHelpers.UpdateVertexIdAfterRemoving(DrawingArea, vertexToRemove.Id);
            }
        }

        private void DeleteEdge(ArrowLine graphEdge, Vertex start, Vertex end)
        {
            if (GraphType.Weighted == _graph.GetGraphType)
                RemoveWeightedEdge(start, end, graphEdge.Tag.ToString()!);
            else
                RemoveEdge(start, end);
            DrawingArea.Children.Remove(graphEdge);
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

        private void DrawEdge(Vertex start, Vertex end, int cost = 1)
        {
            if (_previousSelectedVertex == null && _currentSelectedVertex == null)
            {
                _previousSelectedVertex = DrawingHelpers.FindVertexOnCanvas(DrawingArea, start.Id.ToString());
                _currentSelectedVertex = DrawingHelpers.FindVertexOnCanvas(DrawingArea, end.Id.ToString());
            }
            if (!DrawingHelpers.CheckIfEdgeExist(DrawingArea, _previousSelectedVertex, _currentSelectedVertex))
            {
                AdjList[Vertices.IndexOf(start)].Insert(0, new MatrixCellValue(end.Id));
                if (!DrawingHelpers.CheckForOppositeEdge(DrawingArea, _previousSelectedVertex, _currentSelectedVertex))
                {
                    _graph.AddEdge(start, end, cost);
                    if (_isReadFromFile)
                        Matrix[start.Id - 1][end.Id - 1].Value = cost;
                    else
                        Matrix.AddEdge(Vertices, start, end);
                    if (GraphType.Weighted == _graph.GetGraphType)
                    {
                        DrawingHelpers.DrawWeightedEdgeOnCanvas(DrawingArea, _previousSelectedVertex, _currentSelectedVertex, cost.ToString());
                    }
                    else
                    {
                        if (GraphType.Undirected == _graph.GetGraphType && _isReadFromFile)
                            Matrix[start.Id - 1][end.Id - 1].Value = cost;
                        else
                            Matrix.AddEdge(Vertices, end, start);
                        DrawingHelpers.DrawEdgeOnCanvas(DrawingArea, _previousSelectedVertex, _currentSelectedVertex, _graph.GetGraphType);
                    }
                }
                _previousSelectedVertex = null;
                _currentSelectedVertex = null;
            }
        }

        private void DrawVertex(Vertex vertex)
        {
            var allVertices = DrawingArea.Children.OfType<Grid>().ToList();
            foreach (var v in allVertices)
                if (DrawingHelpers.IsVertexCollision(vertex.Location, new Point(Canvas.GetLeft(v) - 15, Canvas.GetTop(v) - 15)))
                    return;
            vertex.Id = _graph.GetNextVertexId;
            _graph.AddVertex(vertex);
            Vertices.Add(vertex);
            if (!_isReadFromFile)
                Matrix.AddVertex(Vertices.Count);

            var temp = new ObservableCollection<MatrixCellValue>
            {
                new MatrixCellValue(0)
            };
            AdjList.Add(temp);

            DrawingHelpers.DrawVertexOnCanvas(DrawingArea, vertex.Id.ToString(), vertex);
            _previousSelectedVertex = null;
            _currentSelectedVertex = null;
            return;
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
            if (!(int.TryParse(StartVertex.Text, out start) && start > 0 && start <= _graph.CurrentVertexId))
                return;
            var selectedAlgorithm = AlgorithmsPanel.Children.OfType<RadioButton>()
                 .FirstOrDefault(r => r.IsChecked.HasValue && r.IsChecked.Value);
            if (selectedAlgorithm is null)
                return;

            Algorithm algorithm = selectedAlgorithm.Tag switch
            {
                "DFS" => new DFS(_graph, DrawingArea, _graph.GetVertexById(start)),
                "BFS" => new BFS(_graph, DrawingArea, _graph.GetVertexById(start)),
                "Ford" => new Ford(_graph, DrawingArea, _graph.GetVertexById(1), _graph.GetVertexById(start)),
                "BellmanCalaba" => new BellmanCalaba(_graph, DrawingArea, _graph.GetVertexById(1), _graph.GetVertexById(start)),
                "FordFulkersson" => new FordFulkersson(_graph, DrawingArea, _graph.GetVertexById(1), _graph.GetVertexById(start)),
                "Prim" => new Prim(_graph, DrawingArea, _graph.GetVertexById(start)),
                _ => null!
            };

            BindingOperations.SetBinding(algorithm, Algorithm.ExecutionSpeedProperty, new Binding("Value")
            {
                Source = ExecutionSpeedSlider,
                Mode = BindingMode.OneWay
            });
            ClearAnimation();
            algorithm.BindViewProperties(BellmanAlgoResultsMatrix, BellmanResultsVerticalHeader);
            await algorithm.Execute();
            _algorithmSteps = algorithm.GetSolvingSteps();
            AlgoLogs.Children.Insert(0, algorithm.GetResults().GetLog());
            _wasAlgoRunned = true;
            _isAnimation = true;
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            ClearCanvas();
        }

        private void ClearCanvas()
        {
            if (_wasAlgoRunned)
            {
                ClearAnimation();
                _wasAlgoRunned = false;
            }
            else
            {
                DrawingArea.Children.Clear();
                if (_graph is not null)
                {
                    _graph = null!;
                    Matrix.Clear();
                    Vertices.Clear();
                    AdjList.Clear();
                }
                GraphTypePopup.Visibility = Visibility.Visible;
                GraphTypePopupBlurEffect.Effect = new BlurEffect()
                {
                    KernelType = KernelType.Gaussian,
                    Radius = 5
                };
                DrawingArea.IsEnabled = false;
                AlgoLogs.Children.Clear();
            }
        }

        private void ClearAnimation()
        {
            DrawingHelpers.ClearCanvasFromAnimationEffects(DrawingArea);
            DrawingHelpers.ClearEdgesFlow(DrawingArea, _graph);
            BellmanAlgoResultsMatrix.ItemsSource = null;
            BellmanAlgoResultsMatrix.Items.Clear();
            BellmanResultsVerticalHeader.ItemsSource = null;
            BellmanResultsVerticalHeader.Items.Clear();
            _isAnimation = false;
        }

        private void CreateNewGraph_Click(object sender, RoutedEventArgs e)
        {
            GraphType graphType = GraphType.Undirected;
            var btn = sender as Button;
            if (btn.Name == "WeightedGraph")
                graphType = GraphType.Weighted;
            if (btn.Name == "DirectedGraph")
                graphType = GraphType.Directed;
            InitializeNewGraph(graphType);
        }

        private void InitializeNewGraph(GraphType graphType)
        {
            _graph = new Graph(graphType);

            IsWeightedGraph = GraphType.Weighted == graphType;
            IsUndirectedGraph = GraphType.Undirected == graphType;
            Matrix = new AdjacencyMatrix(graphType);
            Vertices = new ObservableCollection<Vertex>();

            GraphTypePopup.Visibility = Visibility.Collapsed;
            GraphTypePopupBlurEffect.Effect = null;
            DrawingArea.IsEnabled = true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
            string[] vertices = textBox.Tag.ToString()!.Split(" ");
            _previousSelectedVertex = DrawingHelpers.FindVertexOnCanvas(DrawingArea, vertices[0]);
            _currentSelectedVertex = DrawingHelpers.FindVertexOnCanvas(DrawingArea, vertices[1]);
            DrawingHelpers.HighlightSelection(_currentSelectedVertex);
            DrawingHelpers.HighlightSelection(_previousSelectedVertex);
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
                try
                {
                    var bindingExpression = BindingOperations.GetMultiBindingExpression(textBox, TextBox.TextProperty);
                    bindingExpression.UpdateSource();

                    bool isValid = int.TryParse(textBox.Text, out int value) && value != 0;
                    if (_prevValue == value && _prevCell == textBox.Tag.ToString())
                        return;
                    string[] vertices = textBox.Tag.ToString()!.Split(" ");
                    Vertex start = _graph.GetVertexById(int.Parse(vertices[0]));
                    Vertex end = _graph.GetVertexById(int.Parse(vertices[1]));

                    if (isValid)
                    {
                        if (_graph.GetEdge(start, end) is not null)
                            _graph.UpdateEdgeWeight(start, end, int.Parse(textBox.Text));
                        else
                        {
                            AdjList[Vertices.IndexOf(start)].Insert(0, new MatrixCellValue(end.Id));
                            _graph.AddEdge(start, end, value);
                            if (_graph.GetGraphType == GraphType.Undirected)
                                Matrix.AddEdge(Vertices, end, start);
                        }
                        if (_graph.GetGraphType == GraphType.Weighted)
                            DrawingHelpers.UpdateWeightOnCanvas(DrawingArea, textBox.Tag.ToString()!, textBox.Text);
                        else
                            DrawingHelpers.DrawEdgeOnCanvas(DrawingArea, _previousSelectedVertex, _currentSelectedVertex, _graph.GetGraphType);
                    }
                    else
                    {
                        ArrowLine? edge = DrawingArea.Children.OfType<ArrowLine>()
                            .Where(x => x.Tag.ToString() == textBox.Tag.ToString()
                            || x.Tag.ToString() == $"{end.Id} {start.Id}").FirstOrDefault();
                        if (edge is not null)
                        {
                            if (_graph.GetGraphType == GraphType.Weighted)
                                RemoveWeightedEdge(start, end, textBox.Tag.ToString());
                            else
                                RemoveEdge(start, end);
                            if (_graph.GetGraphType == GraphType.Undirected)
                                Matrix.RemoveEdge(Vertices, end, start);
                            DrawingArea.Children.Remove(edge);
                        }
                    }
                    _prevValue = value;
                    _prevCell = textBox.Tag.ToString();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message, "An error occured. Try again!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                Keyboard.ClearFocus();
            }
        }

        private void Cell_LostFocus(object sender, RoutedEventArgs e)
        {
            DrawingHelpers.UnHighlightSelection(_currentSelectedVertex);
            DrawingHelpers.UnHighlightSelection(_previousSelectedVertex);
            _previousSelectedVertex = null;
            _currentSelectedVertex = null;
        }

        private void GetPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (!_wasAlgoRunned)
                return;
            if (_isAnimation)
                ClearAnimation();
            _algorithmSteps.ShowPreviousStep();
        }

        private void GetNext_Click(object sender, RoutedEventArgs e)
        {
            if (!_wasAlgoRunned)
                return;
            if (_isAnimation)
                ClearAnimation();
            _algorithmSteps.ShowNextStep();
        }

        private void ZoomingPanel_ViewboxChanged(object sender, Ab2d.Controls.ViewboxChangedRoutedEventArgs e)
        {
            //DrawingArea.Width /= ZoomingPanel.ZoomFactor;
            //DrawingArea.Height /= ZoomingPanel.ZoomFactor;
        }

        private void ShowKeybordNavigation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button.IsChecked.HasValue && button.IsChecked.Value)
            {
                Navigation.Visibility = Visibility.Visible;
            }
            else
            {
                Navigation.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveGraph_Click(object sender, RoutedEventArgs e)
        {
            if (_graph == null || _graph.GetAllEdges().Count == 0)
            {
                MessageBox.Show("Construct a graph before saving it!", "Invalid opperation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Text file (*.txt)|*.txt";
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (saveFileDialog.ShowDialog() == false)
                return;
            string fileContent = $"{_graph.GetGraphType}";
            fileContent += $" {_graph.GetVerticesCount()}";
            fileContent += Environment.NewLine;
            foreach (var edge in _graph.GetAllEdges())
            {
                fileContent += $"{edge.From.Id} {edge.To.Id}";
                if (_graph.GetGraphType == GraphType.Weighted)
                    fileContent += $" {edge.Cost}";
                fileContent += Environment.NewLine;
            }
            File.WriteAllText(saveFileDialog.FileName, fileContent);
        }

        private void ReadGraph_Click(object sender, RoutedEventArgs e)
        {

            _isReadFromFile = true;
            ClearCanvas();
            OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Text file (*.txt)|*.txt";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == false)
                return;
            try
            {
                var fileContent = File.ReadAllLines(openFileDialog.FileName);
                var config = fileContent[0].Split(' ', ',', ';');
                var graphType = (GraphType)Enum.Parse(typeof(GraphType), config[0]);
                InitializeNewGraph(graphType);
                Matrix.Initialize(int.Parse(config[1]));
                for (int i = 1; i < fileContent.Length; i++)
                {
                    var data = fileContent[i].Split(' ', ',', ';');
                    int startId = int.Parse(data[0]);
                    int endId = int.Parse(data[1]);
                    Vertex? start = _graph.CheckIfVertexExists(startId) ? _graph.GetVertexById(startId)
                        : new Vertex(startId, GenerateRandomPointOnCanvas(),
                        new Point((DrawingArea.Width - ZoomingPanel.ActualWidth) / 2, (DrawingArea.Height - ZoomingPanel.ActualHeight) / 2));
                    Vertex? end = _graph.CheckIfVertexExists(endId) ? _graph.GetVertexById(endId)
                        : new Vertex(endId, GenerateRandomPointOnCanvas(),
                        new Point((DrawingArea.Width - ZoomingPanel.ActualWidth) / 2, (DrawingArea.Height - ZoomingPanel.ActualHeight) / 2)); ;
                    if (!_graph.CheckIfVertexExists(startId))
                        DrawVertex(start);
                    if (!_graph.CheckIfVertexExists(endId))
                        DrawVertex(end);
                    int cost = 1;
                    if (graphType == GraphType.Weighted)
                        cost = int.Parse(data[2]);

                    DrawEdge(start, end, cost);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Could not read the graph!\nMake sure the file follows the format rules!", "Invalid opperation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            _graph.CurrentVertexId = _graph.AdjacencyList.Keys.Count;
        }

        private Point GenerateRandomPointOnCanvas()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            int limx1 = 50, limx2 = (int)ZoomingPanel.ActualWidth - 50,
                limy1 = limx1, limy2 = (int)ZoomingPanel.ActualHeight - 50;
            int x = random.Next(limx1, limx2);
            int y = random.Next(limy1, limy2);
            return new Point(x, y);
        }

        private void ShowFormatInfo_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as ToggleButton;
            if (button.IsChecked.HasValue && button.IsChecked.Value)
            {
                InfoFormat.Visibility = Visibility.Visible;
            }
            else
            {
                InfoFormat.Visibility = Visibility.Collapsed;
            }
        }
    }
}
