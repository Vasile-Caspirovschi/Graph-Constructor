using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Graph_Constructor.Models
{
    public static class AdjacencyMatrixHandler
    {
        public static ObservableCollection<ObservableCollection<MatrixCellValue>> CreateAdjacencyMatrix(List<Vertex> vertices, List<Edge> edges)
        {
            ObservableCollection<ObservableCollection<MatrixCellValue>> matrix = new ObservableCollection<ObservableCollection<MatrixCellValue>>();
            return matrix;
        }

        public static ObservableCollection<ObservableCollection<MatrixCellValue>> AddVertex(ObservableCollection<ObservableCollection<MatrixCellValue>> matrix, int numberOfVertices)
        {
            var temp = new ObservableCollection<MatrixCellValue>();
            for (int i = 0; i < matrix.Count; i++)
                for (int j = matrix[i].Count; j < numberOfVertices; j++)
                    matrix[i].Add(new MatrixCellValue(0, new string($"{i + 1} {j + 1}")));

            for (int index = 0; index < numberOfVertices; index++)
                temp.Add(new MatrixCellValue(0, new string($"{numberOfVertices} {index + 1}")));
            matrix.Add(temp);
            ValidateMainDiagonal(matrix);
            return matrix;
        }
        static void ValidateMainDiagonal(ObservableCollection<ObservableCollection<MatrixCellValue>> matrix)
        {
            for (int i = 0; i < matrix.Count; i++)
                matrix[i][i].Value = -1;
        }

        public static ObservableCollection<ObservableCollection<MatrixCellValue>> RemoveVertex(ObservableCollection<ObservableCollection<MatrixCellValue>> matrix, ObservableCollection<Vertex> vertices, Vertex vertexToRemove)
        {
            matrix.RemoveAt(vertices.IndexOf(vertexToRemove));
            foreach (var column in matrix)
                column.RemoveAt(vertices.IndexOf(vertexToRemove));
            return matrix;
        }

        public static ObservableCollection<ObservableCollection<MatrixCellValue>> AddEdge(ObservableCollection<ObservableCollection<MatrixCellValue>> matrix, ObservableCollection<Vertex> vertices, Vertex start, Vertex end)
        {
            matrix[vertices.IndexOf(start)][vertices.IndexOf(end)].Value = 1;
            return matrix;
        }
        public static ObservableCollection<ObservableCollection<MatrixCellValue>> RemoveEdge(ObservableCollection<ObservableCollection<MatrixCellValue>> matrix, ObservableCollection<Vertex> vertices, Vertex start, Vertex end)
        {
            matrix[vertices.IndexOf(start)][vertices.IndexOf(end)].Value = 0;
            return matrix;
        }

    }
}

