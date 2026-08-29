using AtCoder;
using Xunit;
using Z = AtCoder.StaticModInt<AtCoder.Mod998244353>;

namespace FireFly.Tests;

public class NumericsTests {
    [Fact]
    public void InversesCacheAndGrow() {
        ReadOnlySpan<Z> first = Numerics.GetInvs<Mod998244353>(4);
        Assert.Equal(5, first.Length);
        Assert.Equal((Z)0, first[0]);
        for (int i = 1; i < first.Length; ++i) Assert.Equal((Z)1, first[i] * i);

        ReadOnlySpan<Z> grown = Numerics.GetInvs<Mod998244353>(1000);
        for (int i = 1; i < grown.Length; ++i) Assert.Equal((Z)1, grown[i] * i);
        for (int i = 1; i < first.Length; ++i) Assert.Equal((Z)1, first[i] * i);

        ReadOnlySpan<Z> prefix = Numerics.GetInvs<Mod998244353>(7);
        Assert.Equal(8, prefix.Length);

        ReadOnlySpan<StaticModInt<Mod1000000007>> other = Numerics.GetInvs<Mod1000000007>(50);
        for (int i = 1; i < other.Length; ++i) {
            Assert.Equal((StaticModInt<Mod1000000007>)1, other[i] * i);
        }
    }

    [Fact]
    public void FactorialsAndInverseFactorialsCacheAndGrow() {
        ReadOnlySpan<Z> facts = Numerics.GetFacts<Mod998244353>(5);
        Assert.Equal(6, facts.Length);
        Assert.Equal((Z)1, facts[0]);
        for (int i = 1; i < facts.Length; ++i) Assert.Equal(facts[i - 1] * i, facts[i]);

        ReadOnlySpan<Z> invFacts = Numerics.GetInvFacts<Mod998244353>(5);
        for (int i = 0; i < invFacts.Length; ++i) Assert.Equal((Z)1, facts[i] * invFacts[i]);

        ReadOnlySpan<Z> grownFacts = Numerics.GetFacts<Mod998244353>(1000);
        ReadOnlySpan<Z> grownInvFacts = Numerics.GetInvFacts<Mod998244353>(1000);
        for (int i = 0; i < grownFacts.Length; ++i) {
            Assert.Equal((Z)1, grownFacts[i] * grownInvFacts[i]);
        }
        for (int i = 0; i < facts.Length; ++i) Assert.Equal((Z)1, facts[i] * invFacts[i]);
    }

    [Fact]
    public void Binom() {
        Assert.Equal((Z)10, Numerics.Binom<Mod998244353>(5, 2));
        Assert.Equal((Z)1, Numerics.Binom<Mod998244353>(5, 0));
        Assert.Equal((Z)1, Numerics.Binom<Mod998244353>(5, 5));
        Assert.Equal((Z)0, Numerics.Binom<Mod998244353>(5, -1));
        Assert.Equal((Z)0, Numerics.Binom<Mod998244353>(5, 6));
    }

    [Fact]
    public void Perm() {
        Assert.Equal((Z)20, Numerics.Perm<Mod998244353>(5, 2));
        Assert.Equal((Z)1, Numerics.Perm<Mod998244353>(5, 0));
        Assert.Equal((Z)120, Numerics.Perm<Mod998244353>(5, 5));
        Assert.Equal((Z)0, Numerics.Perm<Mod998244353>(5, -1));
        Assert.Equal((Z)0, Numerics.Perm<Mod998244353>(5, 6));
    }
}
