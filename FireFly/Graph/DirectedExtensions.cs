using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FireFly.Graph;

/// <summary>Provides algorithms specific to directed graphs.</summary>
public static class DirectedExtensions {
    /// <summary>Mutates the graph in O(V + E) time and O(V + E) extra space, where E is the initial edge count, by adding a reverse for every encountered edge. Edges inserted into vertices not yet visited are encountered too, so some original directions are duplicated.</summary>
    /// <param name="G">The directed graph to mutate.</param>
    /// <returns>The same graph after the reversed edges have been added.</returns>
    public static Graph<Directed, TWeight> Inverse<TWeight>(this Graph<Directed, TWeight> G) {
        int n = G.VertexCount, m = G.EdgeCount;
        Graph<Directed, TWeight> I = new(n, m);
        for (int u = 0; u < n; ++u) {
            foreach (var (v, w) in G.GetEdges(u)) {
                G.AddEdge(v, u, w);
            }
        }
        return G;
    }

    /// <summary>Computes a topological ordering in O(V + E) time.</summary>
    /// <param name="G">The directed graph.</param>
    /// <returns>A topological ordering, or null if the graph contains a directed cycle.</returns>
    public static int[]? TopoSort<TWeight>(this Graph<Directed, TWeight> G) {
        int n = G.VertexCount, p = 0;
        int[] deg = new int[n], res = new int[n];
        Queue<int> q = new();
        for (int u = 0; u < n; ++u) {
            foreach (var v in G.GetNeighbors(u)) {
                ++deg[v];
            }
        }
        for (int u = 0; u < n; ++u) {
            if (deg[u] == 0) {
                q.Enqueue(u);
            }
        }
        while (q.Count > 0) {
            int u = q.Dequeue();
            res[p++] = u;
            foreach (var v in G.GetNeighbors(u)) {
                if (--deg[v] == 0) {
                    q.Enqueue(v);
                }
            }
        }
        return p == n ? res : null;
    }
}
