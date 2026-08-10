using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Graph;

/// <summary>Provides single-source shortest-path algorithms.</summary>
public static class ShortestPath {
    /// <summary>Computes shortest distances in O((V + E) log(V + E)) time, assuming all edge weights and intermediate distances are ordered finite values, all weights are nonnegative, addition does not overflow, and every finite shortest distance is below the maximum weight value.</summary>
    /// <param name="graph">The graph.</param>
    /// <param name="from">The source vertex in the range [0, VertexCount).</param>
    /// <returns>The distance to each vertex, with the maximum weight value for unreachable vertices.</returns>
    public static TWeight[] Dijkstra<TDir, TWeight>(this Graph<TDir, TWeight> graph, int from)
        where TDir: struct, IGraphDirection<TDir>
        where TWeight: INumber<TWeight>, IMinMaxValue<TWeight> {
        int n = graph.VertexCount;
        TWeight[] dist = GC.AllocateUninitializedArray<TWeight>(n);
        bool[] vis = new bool[n];
        var inf = TWeight.MaxValue;
        Array.Fill(dist, inf);
        PriorityQueue<int, TWeight> q = new();
        q.Enqueue(from, dist[from] = TWeight.Zero);
        while (q.Count > 0) {
            int u = q.Dequeue();
            if (vis[u]) continue;
            vis[u] = true;
            foreach (var (v, w) in graph.GetEdges(u)) {
                if (dist[v] > dist[u] + w) {
                    dist[v] = dist[u] + w;
                    q.Enqueue(v, dist[v]);
                }
            }
        }
        return dist;
    }

    /// <summary>Computes shortest distances in O(VE) worst-case time, assuming all edge weights and intermediate distances are ordered finite values, addition does not overflow, and every finite shortest distance is below the maximum weight value.</summary>
    /// <param name="graph">The graph.</param>
    /// <param name="from">The source vertex in the range [0, VertexCount).</param>
    /// <returns>The distance to each vertex with the maximum weight value for unreachable vertices, or null if a reachable negative cycle exists.</returns>
    public static TWeight[]? SPFA<TDir, TWeight>(this Graph<TDir, TWeight> graph, int from)
        where TDir : struct, IGraphDirection<TDir>
        where TWeight : INumber<TWeight>, IMinMaxValue<TWeight> {
        int n = graph.VertexCount;
        TWeight[] dist = GC.AllocateUninitializedArray<TWeight>(n);
        bool[] vis = new bool[n];
        int[] cnt = new int[n];
        var inf = TWeight.MaxValue;
        Array.Fill(dist, inf);
        Queue<int> q = new();
        dist[from] = TWeight.Zero;
        vis[from] = true;
        q.Enqueue(from);
        while (q.Count > 0) {
            int u = q.Dequeue();
            vis[u] = false;
            foreach (var (v, w) in graph.GetEdges(u)) {
                if (dist[v] > dist[u] + w) {
                    dist[v] = dist[u] + w;
                    cnt[v] = cnt[u] + 1;
                    if (cnt[v] >= n) {
                        return null;
                    }
                    if (!vis[v]) {
                        q.Enqueue(v);
                        vis[v] = true;
                    }
                }
            }
        }
        return dist;
    }
}
