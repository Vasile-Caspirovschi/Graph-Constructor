using Graph_Constructor.Helpers;
using Graph_Constructor.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Graph_Constructor.Algorithms
{
    internal class BellmanCalaba
    {
        readonly Graph _graph;
        readonly Vertex _target;
        readonly Canvas _drawingArea;
        int vectorId = 0;
        public ObservableCollection<string> VectorsTitle { get; set; }
        public ObservableCollection<ObservableCollection<MatrixCellValue>> Vectors { get; set; }
        //etichetele
        //public List<List<Vertex>> Paths { get; set; }

        public BellmanCalaba(Graph graph, Canvas drawingArea, Vertex target)
        {
            _target = target;
            _graph = graph;
            _drawingArea = drawingArea;
            Vectors = new ObservableCollection<ObservableCollection<MatrixCellValue>>();
            VectorsTitle = new ObservableCollection<string>();
        }

        public async Task Init()
        {
            await DetermineVectors();
        }

        public async Task DetermineVectors()
        {
            bool stopCondition = true;
            int nOfVertices = 0, min = 0, diff = 0;
            int[,] weightedMatrix = _graph.GetWeightedMatrix();
            nOfVertices = weightedMatrix.GetLength(0);

            var v0 = new ObservableCollection<MatrixCellValue>();
            Vectors.Add(v0);
            VectorsTitle.Add($"V{vectorId++}");
            for (int i = 0; i < nOfVertices; i++)
            {
                Vectors[0].Add(new MatrixCellValue(weightedMatrix[i, _target.Id - 1]));
                await Task.Delay((int)Delay.VeryTiny / 2);
            }

            do
            {
                stopCondition = true;
                var vn = new ObservableCollection<MatrixCellValue>();
                var lastVector = Vectors.Last();
                Vectors.Add(vn);
                VectorsTitle.Add($"V{vectorId++}");
                for (int i = 0; i < nOfVertices; i++)
                {
                    min = int.MaxValue;
                    for (int j = 0; j < nOfVertices; j++)
                    {
                        if (i == j || weightedMatrix[i,j] == int.MaxValue || lastVector[j].Value == int.MaxValue)
                            continue;
                        diff = lastVector[j].Value + weightedMatrix[i,j];
                        if (diff < min)
                            min = diff;
                    }
                    if (min == int.MaxValue)
                        min = 0;
                    vn.Add(new MatrixCellValue(min));
                    await Task.Delay((int)Delay.VeryTiny / 2);
                }
                lastVector = Vectors.Last();
                var lastButOne = Vectors[Vectors.Count - 2];

                for (int i = 0; i < nOfVertices; i++)
                {
                    if (lastVector[i].Value != lastButOne[i].Value)
                    {
                        stopCondition = false;
                        break;
                    }
                }
            } while (!stopCondition);
        }
    }
}
