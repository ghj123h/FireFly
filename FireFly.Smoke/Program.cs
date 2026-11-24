using System;
using AtCoder;
using FireFly;
using FireFly.Graph;
using FireFly.IO;

SourceExpander.Expander.Expand();

if (Comb.Binom<Mod998244353>(5, 2) != 10) {
    throw new InvalidOperationException("The AtCoder-backed FireFly smoke check failed.");
}

if (Numerics.Gcd(84, 30) != 6 || Numerics.Lcm(84, 30) != 420) {
    throw new InvalidOperationException("The FireFly integer arithmetic smoke check failed.");
}

if (ModMath.GetInvs<Mod998244353>(2)[2] * 2 != 1 ||
    !ModMath.Sqrt((StaticModInt<Mod998244353>)4, out var root) || root != 2) {
    throw new InvalidOperationException("The FireFly modular arithmetic smoke check failed.");
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
