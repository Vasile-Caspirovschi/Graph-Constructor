using Graph_Constructor.Comparers;
using Graph_Constructor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph_Constructor.Models
{
    public class Graph
    {
        private int _nextVertexId;

        Dictionary<Vertex, List<Edge>> _adjacencyList;
        private GraphType _type;
        public Graph(GraphType type)
        {
            _adjacencyList = new Dictionary<Vertex, List<Edge>>();
            _type = type;
        }

        public Graph(Dictionary<Vertex, List<Edge>> adjacencyList, GraphType type)
        {
            _adjacencyList = adjacencyList;
            _type = type;
        }

        public Dictionary<Vertex, List<Edge>> AdjacencyList { get { return _adjacencyList; } }

        public GraphType GetGraphType { get => _type; }
        public int GetNextVertexId { get => ++_nextVertexId; }
        public int CurrentVertexId { get => _nextVertexId; set => _nextVertexId = value; }

        public void AddVertex(Vertex vertex)
        {
            Validate(() => !_adjacencyList.ContainsKey(vertex));
            _adjacencyList.Add(vertex, new List<Edge>());
        }

        public void RemoveVertex(Vertex vertex)
        {
            Validate(() => _adjacencyList.ContainsKey(vertex));
            List<Edge> edgesToRemove = new List<Edge>();
            foreach (var vert in AdjacencyList)
                foreach (var edge in vert.Value)
                    if (edge.To.Id == vertex.Id)
                        edgesToRemove.Add(edge);

            foreach (var edge in edgesToRemove)
                RemoveEdge(edge.From, edge.To);
            UpdateVertexIdAfterRemoving(vertex);
        }

        private void UpdateVertexIdAfterRemoving(Vertex vertex)
        {
            _adjacencyList.Remove(_adjacencyList.Keys.Single(v => v.Id == vertex.Id));
            foreach (var key in _adjacencyList.Keys)
            {
                if (key.Id > vertex.Id)
                    key.Id -= 1;
            }

            CurrentVertexId -= 1;
        }

        private void Validate(Func<bool> condition)
        {
            if (!condition())
                throw new InvalidOperationException();
        }

        public void AddEdge(Vertex from, Vertex to)
        {
            Validate(() => _adjacencyList.ContainsKey(from));
            Validate(() => _adjacencyList.ContainsKey(to));

            var srcEdge = new Edge(from, to);
            _adjacencyList[from].Add(srcEdge);
        }

        public void AddEdge(Vertex from, Vertex to, int cost)
        {
            Validate(() => _adjacencyList.ContainsKey(from));
            Validate(() => _adjacencyList.ContainsKey(to));

            var srcEdge = new Edge(from, to, cost);
            _adjacencyList[from].Add(srcEdge);
        }
        public void UpdateEdgeWeight(Vertex from, Vertex to, int cost)
        {
            Validate(() => _adjacencyList.ContainsKey(from));
            Validate(() => _adjacencyList.ContainsKey(to));

            Edge edge = GetEdge(from, to);
            edge.Cost = cost;
        }

        public void RemoveEdge(Vertex from, Vertex to)
        {
            Validate(() => _adjacencyList.ContainsKey(from));
            Validate(() => _adjacencyList.ContainsKey(to));

            var srcEdge = _adjacencyList[from].FirstOrDefault(edge => edge.To.Id == to.Id);
            if (srcEdge is not null)
                _adjacencyList[from].Remove(srcEdge);
        }

        public Edge GetEdge(Vertex from, Vertex to)
        {
            Validate(() => _adjacencyList.ContainsKey(from));
            return _adjacencyList[from].FirstOrDefault(v => v.From.Equals(from) && v.To.Equals(to));
        }

        public int GetVerticesCount() => _adjacencyList.Keys.Count;

        public List<Vertex> GetAllVertices() => _adjacencyList?.Keys?.ToList() ?? new List<Vertex>();

        public List<Edge> GetAllEdges()
        {
            var result = new List<Edge>();

            foreach (var vertex in _adjacencyList)
                foreach (var adjacent in vertex.Value)
                {
                    result.Add(adjacent);
                    if (_type == GraphType.Undirected)
                    {
                        result.Add(new Edge(adjacent.To, adjacent.From, adjacent.Cost));
                    }
                }
            return result;
        }

        public int[,] GetWeightedMatrix()
        {
            int nVertices = _adjacencyList.Count;
            var weightedMatrix = new int[nVertices, nVertices];
            for (int i = 0; i < nVertices; i++)
                for (int j = 0; j < nVertices; j++)
                    if (i != j)
                        weightedMatrix[i, j] = int.MaxValue;
            foreach (var vertex in _adjacencyList)
                foreach (var adjacent in vertex.Value)
                    weightedMatrix[vertex.Key.Id - 1, adjacent.To.Id - 1] = adjacent.Cost;
            return weightedMatrix;
        }

        public Vertex GetVertexById(int id) => _adjacencyList.Keys.Single(k => Equals(id, k.Id));
        public bool CheckIfVertexExists(int id)
        {
            return _adjacencyList.Keys.Any(v => v.Id == id);
        }
    }
}
