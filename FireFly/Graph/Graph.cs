using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Graph;

/// <summary>Represents the absence of an edge weight.</summary>
public readonly struct NoWeight { }

/// <summary>Represents an adjacency-list graph whose vertices are numbered from zero.</summary>
public class Graph<TDir, TWeight> where TDir: struct, IGraphDirection<TDir> {
    internal GraphCore<TWeight> _core;

    /// <summary>Represents a value copy of an outgoing edge.</summary>
    /// <param name="To">The destination vertex.</param>
    /// <param name="Weight">The edge weight.</param>
    public record struct Edge<T>(int To, T Weight);

    /// <summary>Creates a graph with the given number of vertices and initial edge capacity.</summary>
    /// <param name="vertexCount">The nonnegative number of vertices.</param>
    /// <param name="edgeCapacity">The nonnegative initial capacity for stored directed edges.</param>
    public Graph(int vertexCount, int edgeCapacity) {
        _core = new(vertexCount, edgeCapacity);
    }

    /// <summary>Gets the number of vertices.</summary>
    public int VertexCount => _core.head.Length;

    /// <summary>Gets the number of stored directed edges.</summary>
    public int EdgeCount => _core.edges.Count;

    /// <summary>Adds an edge according to the graph direction's storage convention.</summary>
    /// <param name="from">The source vertex in the range [0, VertexCount).</param>
    /// <param name="to">The destination vertex in the range [0, VertexCount).</param>
    /// <param name="weight">The edge weight.</param>
    [MethodImpl(256)]
    public void AddEdge(int from, int to, TWeight weight) {
        //_core.AddEdge(from, to, weight);
        //if (typeof(TDir) == typeof(TWeight)) {
        //    _core.AddEdge(to, from, weight);
        //}
        TDir.AddEdge(this, from, to, weight);
    }

    /// <summary>Gets an allocation-free live view of the outgoing edges of a vertex in O(1) time.</summary>
    /// <param name="from">The vertex in the range [0, VertexCount).</param>
    /// <returns>A view that enumerates outgoing edges from newest to oldest in O(out-degree) time; adding edges can change later enumerations.</returns>
    public EdgeCollection GetEdges(int from) => new(_core, from);

    /// <summary>Gets an allocation-free live view of the outgoing neighbors of a vertex in O(1) time.</summary>
    /// <param name="from">The vertex in the range [0, VertexCount).</param>
    /// <returns>A view that enumerates outgoing neighbors from newest to oldest in O(out-degree) time; adding edges can change later enumerations.</returns>
    public NeighborCollection GetNeighbors(int from) => new(_core, from);

    /// <summary>Provides a live, allocation-free view over the outgoing edges of one vertex. Instances must be obtained from GetEdges, and stable traversal requires that no edges be added during enumeration.</summary>
    public readonly struct EdgeCollection {
        private readonly GraphCore<TWeight> _core;
        private readonly int _u;
        internal EdgeCollection(GraphCore<TWeight> core, int u) {
            _core = core;
            _u = u;
        }

        /// <summary>Creates an enumerator for the outgoing edges.</summary>
        /// <returns>An enumerator positioned before the first edge.</returns>
        public readonly Enumerator GetEnumerator() => new(_core, _u);

        /// <summary>Enumerates the outgoing edges of one vertex. Instances must be obtained from EdgeCollection.GetEnumerator.</summary>
        public struct Enumerator {
            private readonly GraphCore<TWeight> _core;
            private int _index;
            private readonly int _u;
            internal Enumerator(GraphCore<TWeight> core, int u) {
                _core = core;
                _u = u;
                _index = -2;
            }

            /// <summary>Advances to the next outgoing edge in O(1) time.</summary>
            /// <returns>Whether an edge is available.</returns>
            public bool MoveNext() {
                _index = _index == -2 ? _core.head[_u] : _core.edges[_index].Next;
                return _index >= 0;
            }

            /// <summary>Gets a value copy of the current edge after the most recent call to MoveNext returned true; modifying the copy does not modify the graph.</summary>
            public readonly Edge<TWeight> Current => new(_core.edges[_index].To, _core.edges[_index].Weight);
        }
    }

    /// <summary>Provides a live, allocation-free view over the outgoing neighbors of one vertex. Instances must be obtained from GetNeighbors, and stable traversal requires that no edges be added during enumeration.</summary>
    public readonly struct NeighborCollection {
        private readonly GraphCore<TWeight> _core;
        private readonly int _u;
        internal NeighborCollection(GraphCore<TWeight> core, int u) {
            _core = core;
            _u = u;
        }

        /// <summary>Creates an enumerator for the outgoing neighbors.</summary>
        /// <returns>An enumerator positioned before the first neighbor.</returns>
        public readonly Enumerator GetEnumerator() => new(_core, _u);

        /// <summary>Enumerates the outgoing neighbors of one vertex. Instances must be obtained from NeighborCollection.GetEnumerator.</summary>
        public struct Enumerator {
            private readonly GraphCore<TWeight> _core;
            private int _index;
            private readonly int _u;
            internal Enumerator(GraphCore<TWeight> core, int u) {
                _core = core;
                _u = u;
                _index = -2;
            }

            /// <summary>Advances to the next outgoing neighbor in O(1) time.</summary>
            /// <returns>Whether a neighbor is available.</returns>
            public bool MoveNext() {
                _index = _index == -2 ? _core.head[_u] : _core.edges[_index].Next;
                return _index >= 0;
            }

            /// <summary>Gets the current neighbor after the most recent call to MoveNext returned true.</summary>
            public readonly int Current => _core.edges[_index].To;
        }
    }
}

/// <summary>Provides unweighted graph extensions.</summary>
public static class NoWeightGraphExtension {
    /// <summary>Adds an unweighted edge according to the graph direction's storage convention.</summary>
    /// <param name="graph">The graph to modify.</param>
    /// <param name="from">The source vertex in the range [0, VertexCount).</param>
    /// <param name="to">The destination vertex in the range [0, VertexCount).</param>
    [MethodImpl(256)]
    public static void AddEdge<TDir>(this Graph<TDir, NoWeight> graph, int from, int to)
        where TDir: struct, IGraphDirection<TDir> {
        graph.AddEdge(from, to, default);
    }
}
