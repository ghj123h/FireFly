using AtCoder;
using Xunit;
using Z = AtCoder.StaticModInt<AtCoder.Mod998244353>;

namespace FireFly.Tests;

public class CombTests {
    [Fact]
    public void FactorialsAndInverseFactorialsCacheAndGrow() {
        ReadOnlySpan<Z> facts = Comb.GetFacts<Mod998244353>(5);
        Assert.Equal(6, facts.Length);
        Assert.Equal((Z)1, facts[0]);
        for (int i = 1; i < facts.Length; ++i) Assert.Equal(facts[i - 1] * i, facts[i]);

        ReadOnlySpan<Z> invFacts = Comb.GetInvFacts<Mod998244353>(5);
        for (int i = 0; i < invFacts.Length; ++i) Assert.Equal((Z)1, facts[i] * invFacts[i]);

        ReadOnlySpan<Z> grownFacts = Comb.GetFacts<Mod998244353>(1000);
        ReadOnlySpan<Z> grownInvFacts = Comb.GetInvFacts<Mod998244353>(1000);
        for (int i = 0; i < grownFacts.Length; ++i) {
            Assert.Equal((Z)1, grownFacts[i] * grownInvFacts[i]);
        }
        for (int i = 0; i < facts.Length; ++i) Assert.Equal((Z)1, facts[i] * invFacts[i]);
    }

    [Fact]
    public void IncrementalGrowthStaysValidAcrossBoundaries() {
        for (int n = 0; n <= 64; ++n) {
            ReadOnlySpan<StaticModInt<Mod1000000007>> facts = Comb.GetFacts<Mod1000000007>(n);
            ReadOnlySpan<StaticModInt<Mod1000000007>> invFacts = Comb.GetInvFacts<Mod1000000007>(n);
            Assert.Equal(n + 1, facts.Length);
            Assert.Equal(n + 1, invFacts.Length);
            Assert.Equal((StaticModInt<Mod1000000007>)1, facts[0]);
            for (int i = 1; i <= n; ++i) {
                Assert.Equal(facts[i - 1] * i, facts[i]);
            }
            for (int i = 0; i <= n; ++i) {
                Assert.Equal((StaticModInt<Mod1000000007>)1, facts[i] * invFacts[i]);
            }
        }
    }

    [Fact]
    public void Binom() {
        Assert.Equal((Z)10, Comb.Binom<Mod998244353>(5, 2));
        Assert.Equal((Z)1, Comb.Binom<Mod998244353>(5, 0));
        Assert.Equal((Z)1, Comb.Binom<Mod998244353>(5, 5));
        Assert.Equal((Z)0, Comb.Binom<Mod998244353>(5, -1));
        Assert.Equal((Z)0, Comb.Binom<Mod998244353>(5, 6));
    }

    [Fact]
    public void Perm() {
        Assert.Equal((Z)20, Comb.Perm<Mod998244353>(5, 2));
        Assert.Equal((Z)1, Comb.Perm<Mod998244353>(5, 0));
        Assert.Equal((Z)120, Comb.Perm<Mod998244353>(5, 5));
        Assert.Equal((Z)0, Comb.Perm<Mod998244353>(5, -1));
        Assert.Equal((Z)0, Comb.Perm<Mod998244353>(5, 6));
    }
}
