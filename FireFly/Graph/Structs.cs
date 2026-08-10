using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Graph;

/// <summary>Defines how a graph stores an added edge.</summary>
public interface IGraphDirection<TSelf> where TSelf : struct, IGraphDirection<TSelf> {
    /// <summary>Adds an edge using the direction's storage convention.</summary>
    /// <param name="graph">The graph to modify.</param>
    /// <param name="from">The source vertex in the range [0, VertexCount).</param>
    /// <param name="to">The destination vertex in the range [0, VertexCount).</param>
    /// <param name="weight">The edge weight.</param>
    static abstract void AddEdge<TWeight>(Graph<TSelf, TWeight> graph, int from, int to, TWeight weight);
}

/// <summary>Represents the directed graph storage convention.</summary>
public readonly struct Directed : IGraphDirection<Directed> {
    /// <summary>Stores an edge from the source to the destination in amortized O(1) time.</summary>
    /// <param name="graph">The directed graph to modify.</param>
    /// <param name="from">The source vertex in the range [0, VertexCount).</param>
    /// <param name="to">The destination vertex in the range [0, VertexCount).</param>
    /// <param name="weight">The edge weight.</param>
    [MethodImpl(256)]
    public static void AddEdge<TWeight>(Graph<Directed, TWeight> graph, int from, int to, TWeight weight) {
        graph._core.AddEdge(from, to, weight);
    }
}

/// <summary>Represents the undirected graph storage convention.</summary>
public readonly struct Undirected : IGraphDirection<Undirected> {
    /// <summary>Stores an edge in both directions in amortized O(1) time.</summary>
    /// <param name="graph">The undirected graph to modify.</param>
    /// <param name="from">One endpoint in the range [0, VertexCount).</param>
    /// <param name="to">The other endpoint in the range [0, VertexCount).</param>
    /// <param name="weight">The edge weight.</param>
    [MethodImpl(256)]
    public static void AddEdge<TWeight>(Graph<Undirected, TWeight> graph, int from, int to, TWeight weight) {
        graph._core.AddEdge(from, to, weight);
        graph._core.AddEdge(to, from, weight);
    }
}
