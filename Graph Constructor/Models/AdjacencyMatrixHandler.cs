using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace Graph_Constructor.Models
{
    public static class AdjacencyMatrixHandler
    {
        public static ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> CreateAdjacencyMatrix(List<Vertex> vertices, List<Edge> edges)
        {
            ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> matrix = new ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>>();
            for (int indexVertex = 0; indexVertex < vertices.Count; indexVertex++)
            {
                var temp = new ObservableCollection<AdjacencyMatrixCellValue>();
                for (int indexEdge = 0; indexEdge < vertices.Count; indexEdge++)
                    temp.Add(new AdjacencyMatrixCellValue(0));
                matrix.Add(temp);
            }
            foreach (var element in edges)
            {
                matrix[vertices.IndexOf(element.From)][vertices.IndexOf(element.To)].Value = 1;
                //_matrix[_vertices.IndexOf(element.To), _vertices.IndexOf(element.From)] = 1;
            }
            return matrix;
        }

        public static ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> AddVertex(ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> matrix, int numberOfVertices)
        {
            var temp = new ObservableCollection<AdjacencyMatrixCellValue>();
            for (int i = 0; i < matrix.Count; i++)
                for (int j = matrix[i].Count; j < numberOfVertices; j++)
                    matrix[i].Add(new AdjacencyMatrixCellValue(0));

            for (int index = 0; index < numberOfVertices; index++)
                temp.Add(new AdjacencyMatrixCellValue(0));
            matrix.Add(temp);
            return matrix;
        }

        public static ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> RemoveVertex(ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> matrix, ObservableCollection<Vertex> vertices, Vertex vertexToRemove)
        {
            matrix.RemoveAt(vertices.IndexOf(vertexToRemove));
            foreach (var column in matrix)
                column.RemoveAt(vertices.IndexOf(vertexToRemove));
            return matrix;
        }

        public static ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> AddEdge(ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> matrix, ObservableCollection<Vertex> vertices, Vertex start, Vertex end)
        {
            matrix[vertices.IndexOf(start)][vertices.IndexOf(end)].Value = 1;
            return matrix;
        }
        public static ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> RemoveEdge(ObservableCollection<ObservableCollection<AdjacencyMatrixCellValue>> matrix, ObservableCollection<Vertex> vertices, Vertex start, Vertex end)
        {
            matrix[vertices.IndexOf(start)][vertices.IndexOf(end)].Value = 0;
            return matrix;
        }
    }
}
