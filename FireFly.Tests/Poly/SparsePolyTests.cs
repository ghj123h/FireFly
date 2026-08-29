using System;
using AtCoder;
using Xunit;
using Z = AtCoder.StaticModInt<AtCoder.Mod998244353>;

namespace FireFly.Tests;

public class SparsePolyTests {
    private const int K = 4;
    private const int N = 1 << K;

    [Fact]
    public void InvAndLogMatchDense() {
        int[] exp = [0, 2, 5, 9];
        Z[] coef = [1, 3, 4, 2];
        int[] expCopy = (int[])exp.Clone();
        Z[] coefCopy = (Z[])coef.Clone();
        Z[] a = Dense(N, exp, coef);
        Poly<Mod998244353> dense = new(a);

        Poly<Mod998244353> sparse = SparsePoly.Inv<Mod998244353>(K, exp, coef);
        AssertPolyEqual(dense.Inv().AsSpan(), sparse.AsSpan());
        sparse = SparsePoly.Log<Mod998244353>(K, exp, coef);
        AssertPolyEqual(dense.Log().AsSpan(), sparse.AsSpan());
        Assert.Equal(expCopy, exp);
        Assert.Equal(coefCopy, coef);
    }

    [Fact]
    public void ExpMatchesDense() {
        int[] exp = [1, 4, 7];
        Z[] coef = [2, 5, 3];
        Z[] a = Dense(N, exp, coef);

        Poly<Mod998244353> sparse = SparsePoly.Exp<Mod998244353>(K, exp, coef);
        AssertPolyEqual(new Poly<Mod998244353>(a).Exp().AsSpan(), sparse.AsSpan());
    }

    [Fact]
    public void PowMatchesDense() {
        int[] exp = [2, 5, 9];
        Z[] coef = [3, 7, 4];
        Z[] a = Dense(N, exp, coef);

        Poly<Mod998244353> sparse = SparsePoly.Pow<Mod998244353>(K, 3, exp, coef);
        AssertPolyEqual(new Poly<Mod998244353>(a).Pow(3).AsSpan(), sparse.AsSpan());
    }

    [Fact]
    public void SqrtMatchesDenseWithoutMutatingInputs() {
        int[] exp = [2, 5, 9];
        Z[] coef = [1, 3, 8];
        int[] expCopy = (int[])exp.Clone();
        Z[] coefCopy = (Z[])coef.Clone();
        Z[] a = Dense(N, exp, coef);
        Poly<Mod998244353> dense = new(a);

        Assert.True(dense.Sqrt(out Poly<Mod998244353> root));
        Assert.True(SparsePoly.Sqrt<Mod998244353>(K, exp, coef, out Poly<Mod998244353> sparse));
        AssertPolyEqual(root.AsSpan(), sparse.AsSpan());
        Assert.Equal(expCopy, exp);
        Assert.Equal(coefCopy, coef);
    }

    private static Z[] Dense(int n, ReadOnlySpan<int> exp, ReadOnlySpan<Z> coef) {
        Z[] a = new Z[n];
        for (int i = 0; i < exp.Length; ++i) a[exp[i]] = coef[i];
        return a;
    }

    private static void AssertPolyEqual(ReadOnlySpan<Z> expected, ReadOnlySpan<Z> actual) {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; ++i) Assert.Equal(expected[i], actual[i]);
    }
}
