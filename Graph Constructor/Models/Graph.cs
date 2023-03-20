﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Graph_Constructor.Models
{
    public class Graph
    {
        private IDictionary<Vertex, ICollection<Edge>> _adjacencyList;

        public Graph()
        {
            _adjacencyList = new Dictionary<Vertex, ICollection<Edge>>();
        }

        public IDictionary<Vertex, ICollection<Edge>> AdjacencyList { get { return _adjacencyList; } }
        public void AddVertex(Vertex vertex)
        {
            Validate(() => !_adjacencyList.ContainsKey(vertex));
            _adjacencyList.Add(vertex, new List<Edge>());
        }

        public void RemoveVertex(Vertex vertex)
        {
            Validate(() => _adjacencyList.ContainsKey(vertex));
            List<Edge> edgesToRemove= new List<Edge>();
            foreach (var vert in AdjacencyList)
                foreach (var edge in vert.Value)
                    if (edge.To == vertex)
                        edgesToRemove.Add(edge);

            foreach (var edge in edgesToRemove)
                RemoveEdge(edge.From, edge.To);
            UpdateVertexIdAfterRemoving(vertex.Id);
            _adjacencyList.Remove(vertex);
        }

        private void UpdateVertexIdAfterRemoving(int removedId)
        {
            foreach (var vertex in AdjacencyList.Where(vertex => vertex.Key.Id > removedId))
                vertex.Key.Id -= 1;
            //foreach (var vertex in AdjacencyList)
            //{
            //    foreach (var edge in vertex.Value)
            //    {
            //        if (edge.To.Id > removedId)
            //        {
            //            edge.To.Id
            //        }
            //    }
            //}
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
            //var destEdge = new Edge(to, from, cost);
            _adjacencyList[from].Add(srcEdge);
            //_adjacencyList[to].Add(destEdge);
        }

        public void RemoveEdge(Vertex from, Vertex to)
        {
            Validate(() => _adjacencyList.ContainsKey(from));
            Validate(() => _adjacencyList.ContainsKey(to));

            var srcEdge = _adjacencyList[from].First(edge => Equals(edge.To, to));
            //var destEdge = _adjacencyList[to].First(edge => Equals(edge.From, from));
            _adjacencyList[from].Remove(srcEdge);
            //_adjacencyList[to].Add(destEdge);
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
                    result.Add(
                        new Edge(
                            vertex.Key,
                            adjacent.To));

            return result;
        }

        public Vertex GetVertexById(int id) => _adjacencyList.Keys.Single(k => Equals(id, k.Id));
    }
}
