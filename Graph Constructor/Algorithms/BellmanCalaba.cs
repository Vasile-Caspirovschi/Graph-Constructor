﻿using Graph_Constructor.Enums;
using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class BellmanCalaba : Algorithm
    {
        private int vectorId = 0;
        public ObservableCollection<string> VectorsTitle { get; set; }
        public ObservableCollection<ObservableCollection<MatrixCellValue>> Vectors { get; set; }
        public Dictionary<Vertex, List<Vertex>> OutcomingVertices { get; set; }
        public List<List<Vertex>> Paths { get; set; }
        public int PathLength { get; set; }
        public AlgorithmSteps Steps { get; set; }

        public BellmanCalaba(Graph graph, Canvas drawingArea, Vertex from, Vertex target) 
            : base(graph, drawingArea, from, target!)
        {
            Vectors = new ObservableCollection<ObservableCollection<MatrixCellValue>>();
            VectorsTitle = new ObservableCollection<string>();
            OutcomingVertices = new Dictionary<Vertex, List<Vertex>>();
            Paths = new List<List<Vertex>>();
            Steps = new AlgorithmSteps(drawingArea, "BellmanCalaba");
        }

        public override async Task Execute()
        {
            await DetermineVectors();
            await DetermineOutcomingVertices();
            GetPaths();
            await HighlightPathsOnCanvas();
        }

        public async Task DetermineVectors()
        {
            bool stopCondition = true;
            int nOfVertices = 0, min = 0, diff = 0;
            int[,] weightedMatrix = graph.GetWeightedMatrix();
            nOfVertices = weightedMatrix.GetLength(0);

            var v0 = new ObservableCollection<MatrixCellValue>();
            Vectors.Add(v0);
            VectorsTitle.Add($"V{vectorId++}");
            for (int i = 0; i < nOfVertices; i++)
            {
                Vectors[0].Add(new MatrixCellValue(weightedMatrix[i, target!.Id - 1]));
                await Task.Delay(SetExecutionDelay((int)Delay.Short));
            }

            do
            {
                var vn = new ObservableCollection<MatrixCellValue>();
                var lastVector = Vectors.Last();
                Vectors.Add(vn);
                VectorsTitle.Add($"V{vectorId++}");
                for (int i = 0; i < nOfVertices; i++)
                {
                    min = int.MaxValue;
                    for (int j = 0; j < nOfVertices; j++)
                    {
                        if (i == j || weightedMatrix[i, j] == int.MaxValue || lastVector[j].Value == int.MaxValue)
                            continue;
                        diff = lastVector[j].Value + weightedMatrix[i, j];
                        if (diff < min)
                            min = diff;
                    }
                    if (min == int.MaxValue)
                        min = 0;
                    vn.Add(new MatrixCellValue(min));
                    await Task.Delay(SetExecutionDelay((int)Delay.Short));
                }

                lastVector = Vectors.Last();
                var lastButOne = Vectors[^2];
                stopCondition = lastButOne.Select(x => x.Value).SequenceEqual(lastVector.Select(y => y.Value));
            } while (!stopCondition);
        }

        public async Task DetermineOutcomingVertices()
        {
            var weightedMatrix = graph.GetWeightedMatrix();
            foreach (Vertex vertex in graph.GetAllVertices())
                OutcomingVertices.Add(vertex, new List<Vertex>());

            int[] lastVector = Vectors.Last().Select(x => x.Value).ToArray();
            PathLength = lastVector[0];
            int diff = 0;
            for (int i = 0; i < lastVector.Length; i++)
            {
                Vertex start = graph.GetVertexById(i + 1);
                DrawingHelpers.MarkVertex(drawingArea, start, Colors.DoneVertex);
                await Task.Delay(SetExecutionDelay((int)Delay.Tiny));
                for (int j = 0; j < lastVector.Length; j++)
                {
                    Vertex end = graph.GetVertexById(j + 1);
                    Edge edge = graph.GetEdge(start, end);
                    if (edge != null)
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                    DrawingHelpers.MarkVertex(drawingArea, end, Colors.VisitedVertex);
                    //Steps.Add(new AlgorithmStep(DrawingHelpers.ClearCanvasFromAnimationEffects)
                    //    .AddMarkedElement(start, Colors.DoneVertex)
                    //    .AddMarkedElement(edge, Colors.VisitedEdge)
                    //    .AddMarkedElement(end, Colors.VisitedVertex));
                    await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                    if (edge != null)
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.DefaultEdgeColor);
                    DrawingHelpers.MarkVertex(drawingArea, end, Colors.DefaultVertexColor);

                    if (i == j || weightedMatrix[i, j] == int.MaxValue)
                        continue;
                    diff = lastVector[i] - weightedMatrix[i, j];
                    if (diff == lastVector[j])
                        OutcomingVertices[start].Add(end);
                }
                DrawingHelpers.MarkVertex(drawingArea, start, Colors.DefaultVertexColor);
            }
        }

        void GetPaths()
        {
            HashSet<Vertex> visited = new HashSet<Vertex>();
            List<Vertex> path = new List<Vertex>
            {
                start
            };
            GetPathsUtil(start, target!, visited, path);
        }

        void GetPathsUtil(Vertex start, Vertex target, HashSet<Vertex> visited, List<Vertex> localPath)
        {
            if (start.Equals(target!))
            {
                Paths.Add(new List<Vertex>(localPath));
                return;
            }
            visited.Add(start);

            foreach (var vertex in OutcomingVertices[start])
            {
                if (!visited.Contains(vertex))
                {
                    localPath.Add(vertex);
                    GetPathsUtil(vertex, target!, visited, localPath);
                    localPath.Remove(vertex);
                }
            }
            visited.Remove(start);
        }

        async Task HighlightPathsOnCanvas()
        {
            DrawingHelpers.ClearCanvasFromAnimationEffects(drawingArea);
            Steps.Add(new AlgorithmStep(DrawingHelpers.ClearCanvasFromAnimationEffects));
            Vertex prevVertex = start;
            foreach (var path in Paths)
            {
                var step = new AlgorithmStep();
                foreach (var vertex in path)
                {
                    if (vertex != start)
                    {
                        Edge edge = graph.GetEdge(prevVertex, vertex);
                        DrawingHelpers.MarkEdge(drawingArea, edge, Colors.VisitedEdge);
                        await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                        step.AddMarkedElement(edge, Colors.VisitedEdge);
                    }
                    DrawingHelpers.MarkVertex(drawingArea, vertex, Colors.DoneVertex);
                    await Task.Delay(SetExecutionDelay((int)Delay.Medium));
                    step.AddMarkedElement(vertex, Colors.DoneVertex);
                    prevVertex = vertex;
                }
                Steps.Add(step);
            }
        }

        public override AlgoLog GetResults()
        {
            var title = $"The min path length from {start.Id} to {target!.Id} is {PathLength}";
            var details = "All paths are:";
            AlgoLog log = new AlgoLog(title, details);
            Paths.ForEach(path => log.AddMoreDetails(path.Select(vertex => vertex.Id).ToList()));
            return log;
        }

        public override void BindViewProperties(params Control[] viewControls)
        {
            ((ItemsControl)viewControls[0]).ItemsSource = Vectors;
            ((ItemsControl)viewControls[1]).ItemsSource = VectorsTitle;
        }

        public override AlgorithmSteps GetSolvingSteps()
        {
            return Steps;
        }
    }
}
