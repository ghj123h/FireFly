using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.Graph;

internal sealed class GraphCore<TWeight> {
    internal record struct Edge<T>(int To, int Next, T Weight);
    internal List<Edge<TWeight>> edges;
    internal int[] head;

    public GraphCore(int n, int m) {
        head = GC.AllocateUninitializedArray<int>(n);
        Array.Fill(head, -1);
        edges = new(m);
    }

    public void AddEdge(int from, int to, TWeight weight) {
        int tmp = head[from];
        head[from] = edges.Count;
        edges.Add(new(to, tmp, weight));
    }
}