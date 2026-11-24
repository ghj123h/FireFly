using AtCoder;
using Xunit;
using Z = AtCoder.StaticModInt<AtCoder.Mod998244353>;

namespace FireFly.Tests;

public class ModMathTests {
    [Fact]
    public void InversesCacheAndGrow() {
        ReadOnlySpan<Z> first = ModMath.GetInvs<Mod998244353>(4);
        Assert.Equal(5, first.Length);
        Assert.Equal((Z)0, first[0]);
        for (int i = 1; i < first.Length; ++i) Assert.Equal((Z)1, first[i] * i);

        ReadOnlySpan<Z> grown = ModMath.GetInvs<Mod998244353>(1000);
        for (int i = 1; i < grown.Length; ++i) Assert.Equal((Z)1, grown[i] * i);
        for (int i = 1; i < first.Length; ++i) Assert.Equal((Z)1, first[i] * i);

        ReadOnlySpan<Z> prefix = ModMath.GetInvs<Mod998244353>(7);
        Assert.Equal(8, prefix.Length);

        ReadOnlySpan<StaticModInt<Mod1000000007>> other = ModMath.GetInvs<Mod1000000007>(50);
        for (int i = 1; i < other.Length; ++i) {
            Assert.Equal((StaticModInt<Mod1000000007>)1, other[i] * i);
        }
    }

    [Fact]
    public void InversesGrowIncrementallyToModulusCap() {
        for (int n = 0; n < 7; ++n) {
            ReadOnlySpan<StaticModInt<Mod7>> invs = ModMath.GetInvs<Mod7>(n);
            Assert.Equal(n + 1, invs.Length);
            Assert.Equal((StaticModInt<Mod7>)0, invs[0]);
            for (int i = 1; i <= n; ++i) {
                Assert.Equal((StaticModInt<Mod7>)1, invs[i] * i);
            }
        }

        Assert.Equal(1, ModMath.GetInvs<Mod2>(0).Length);
        ReadOnlySpan<StaticModInt<Mod2>> mod2 = ModMath.GetInvs<Mod2>(1);
        Assert.Equal(2, mod2.Length);
        Assert.Equal((StaticModInt<Mod2>)0, mod2[0]);
        Assert.Equal((StaticModInt<Mod2>)1, mod2[1]);
    }

    [Fact]
    public void SquareRootsMatchAllSmallPrimeResidues() {
        for (int v = 0; v < 7; ++v) {
            int expected = -1;
            for (int r = 0; r <= 3; ++r) {
                if (r * r % 7 == v) {
                    expected = r;
                    break;
                }
            }
            StaticModInt<Mod7> value = new(v);
            Assert.Equal(expected >= 0, value.Sqrt(out var root));
            Assert.Equal(expected >= 0 ? expected : 0, root.Value);
        }
    }

    [Fact]
    public void SquareRootsModuloTwo() {
        Assert.True(ModMath.Sqrt((StaticModInt<Mod2>)0, out var zero));
        Assert.Equal(0, zero.Value);
        Assert.True(ModMath.Sqrt((StaticModInt<Mod2>)1, out var one));
        Assert.Equal(1, one.Value);
    }

    private readonly struct Mod7 : IStaticMod {
        public uint Mod => 7;
        public bool IsPrime => true;
    }

    private readonly struct Mod2 : IStaticMod {
        public uint Mod => 2;
        public bool IsPrime => true;
    }
}
