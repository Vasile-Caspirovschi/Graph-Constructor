using Graph_Constructor.Enums;
using System.Collections.ObjectModel;

namespace Graph_Constructor.Models
{
    public class AdjacencyMatrix : ObservableCollection<ObservableCollection<MatrixCellValue>>
    {
        private GraphType _graphType;

        public GraphType GraphType { get => _graphType; set => _graphType = value; }

        public AdjacencyMatrix(GraphType graphType)
        {
            _graphType = graphType;
        }

        public void AddVertex(int numberOfVertices)
        {
            var temp = new ObservableCollection<MatrixCellValue>();
            for (int i = 0; i < Count; i++)
                for (int j = this[i].Count; j < numberOfVertices; j++)
                    this[i].Add(new MatrixCellValue(GraphType == GraphType.Weighted ? int.MaxValue : 0, new string($"{i + 1} {j + 1}"), _graphType));

            for (int index = 0; index < numberOfVertices; index++)
                temp.Add(new MatrixCellValue(GraphType == GraphType.Weighted ? int.MaxValue : 0, new string($"{numberOfVertices} {index + 1}"), _graphType));
            Add(temp);
            ValidateMainDiagonal();
        }
        void ValidateMainDiagonal()
        {
            for (int i = 0; i < Count; i++)
                this[i][i].Value = -1;
        }

        public void RemoveVertex(Vertex vertexToRemove)
        {
            this.RemoveAt(vertexToRemove.Id - 1);
            foreach (var column in this)
                if (column.Count > vertexToRemove.Id - 1)
                    column.RemoveAt(vertexToRemove.Id - 1);
        }

        public void AddEdge(ObservableCollection<Vertex> vertices, Vertex start, Vertex end, int edgeCost = 1)
        {
            this[vertices.IndexOf(start)][vertices.IndexOf(end)].Value = edgeCost;
        }
        public void RemoveEdge(ObservableCollection<Vertex> vertices, Vertex start, Vertex end)
        {
            this[vertices.IndexOf(start)][vertices.IndexOf(end)].Value = GraphType == GraphType.Weighted ? int.MaxValue : 0;
        }
    }
}

