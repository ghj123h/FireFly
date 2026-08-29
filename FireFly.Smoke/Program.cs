using System;
using AtCoder;
using FireFly;
using FireFly.Graph;
using FireFly.IO;

SourceExpander.Expander.Expand();

if (Numerics.Binom<Mod998244353>(5, 2) != 10) {
    throw new InvalidOperationException("The AtCoder-backed FireFly smoke check failed.");
}

BufferedReader br = new(Console.OpenStandardInput(), 1 << 16);
BufferedWriter bw = new();
int n = br.ReadInt32(), m = br.ReadInt32();
Graph<Directed, NoWeight> g = new(n, m);
while (m-- > 0) {
    int u = br.ReadInt32() - 1, v = br.ReadInt32() - 1;
    g.AddEdge(v, u);
}
int[]? ord = g.TopoSort();
bw.AppendJoin(ord!);
bw.Output();
