﻿using Ab2d.Controls;
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
        static int _vertexId = 0;
        bool _isWeightedGraph = false;
        bool _isDirectedGraph = true;
        bool _isAnimation = false;

        private bool _isSpaceKeyDown;
        private bool _isZoomModeChanged;
        private Ab2d.Controls.ZoomPanel.ZoomModeType _savedZoomMode;


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
        private AlgorithmSteps _algorithmSteps;
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _graph = new Graph(IsWeightedGraph);
            Vertices = new ObservableCollection<Vertex>(_graph.GetAllVertices().ToList());
            Edges = new ObservableCollection<Edge>(_graph.GetAllEdges().ToList());
            AdjList = new ObservableCollection<ObservableCollection<MatrixCellValue>>();

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
            if (MoveMode.IsChecked == true)
                return;
            if (ZoomingPanel.ZoomMode != Ab2d.Controls.ZoomPanel.ZoomModeType.None)
                return;
            _currentSelectedVertex = SelectedVertex(e);
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
                _ => null!
            };

            BindingOperations.SetBinding(algorithm, Algorithm.ExecutionSpeedProperty, new Binding("Value")
            {
                Source = ExecutionSpeedSlider,
                Mode = BindingMode.OneWay
            });

            algorithm.BindViewProperties(BellmanAlgoResultsMatrix, BellmanResultsVerticalHeader);
            await algorithm.Execute();
            _algorithmSteps = algorithm.GetSolvingSteps();
            AlgoLogs.Children.Add(algorithm.GetResults().GetLog());
            _wasAlgoRunned = true;
            _isAnimation = true;
        }

        private void ClearCanvas_Click(object sender, RoutedEventArgs e)
        {
            if (_wasAlgoRunned)
            {
                ClearAnimation();
                _wasAlgoRunned = false;
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
            string[] vertices = textBox.Tag.ToString()!.Split(" ");
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
    }
}
