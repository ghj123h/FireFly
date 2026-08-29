using System;
using AtCoder;
using Xunit;
using Z = AtCoder.StaticModInt<AtCoder.Mod998244353>;

namespace FireFly.Tests;

public class PolyTests {
    private const int Seed = 20260810;

    [Fact]
    public void ExpOfXHasInverseFactorialCoefficients() {
        Z[] f = new Z[8];
        f[1] = 1;
        Poly<Mod998244353> poly = new(f);
        Poly<Mod998244353> exp = poly.Exp();
        Z fact = 1;
        for (int i = 0; i < exp.Length; ++i) {
            if (i > 0) fact *= i;
            Assert.Equal((Z)1, exp[i] * fact);
        }
    }

    [Fact]
    public void WrapsProvidedStorage() {
        const int k = 4;
        const int n = 1 << k;
        Z[] wrapped = new Z[n];
        Poly<Mod998244353> view = new(wrapped);
        view[3] = 7;
        Assert.Equal(n, view.Length);
        Assert.Equal((Z)7, wrapped[3]);

        Span<Z> stack = stackalloc Z[8];
        Poly<Mod998244353> stackView = new(stack);
        stackView[2] = 11;
        Assert.Equal((Z)11, stack[2]);

        Poly<Mod998244353> zero = new(k);
        AssertPolyEqual(new Z[n], zero.AsSpan());
    }

    [Fact]
    public void AddAndMultiplyWithoutMutatingInputs() {
        const int n = 16;
        Random rnd = new(Seed);
        Z[] a = RandomPoly(rnd, n), b = RandomPoly(rnd, n);
        Z[] aa = (Z[])a.Clone(), bb = (Z[])b.Clone();
        Poly<Mod998244353> pa = new(a), pb = new(b);

        Poly<Mod998244353> sum = pa + pb;
        Z[] expected = new Z[n];
        for (int i = 0; i < n; ++i) expected[i] = a[i] + b[i];
        AssertPolyEqual(expected, sum.AsSpan());

        Poly<Mod998244353> prod = pa * pb;
        AssertPolyEqual(NaiveMul(a, b), prod.AsSpan());
        AssertPolyEqual(aa, a);
        AssertPolyEqual(bb, b);
    }

    [Fact]
    public void Inv() {
        const int n = 16;
        Z[] a = RandomPoly(new Random(Seed), n);
        a[0] = 3;
        Z[] copy = (Z[])a.Clone();
        Poly<Mod998244353> poly = new(a);

        Poly<Mod998244353> inv = poly.Inv();
        Z[] expected = NaiveMul(a, inv.AsSpan());
        Assert.Equal((Z)1, expected[0]);
        for (int i = 1; i < n; ++i) Assert.Equal((Z)0, expected[i]);
        AssertPolyEqual(copy, a);
    }

    [Fact]
    public void LogExpRoundTrip() {
        const int n = 16;
        Z[] a = RandomPoly(new Random(Seed), n);
        a[0] = 0;
        Poly<Mod998244353> poly = new(a);

        Poly<Mod998244353> exp = poly.Exp();
        Poly<Mod998244353> log = exp.Log();
        AssertPolyEqual(a, log.AsSpan());
    }

    [Fact]
    public void Pow() {
        const int n = 16;
        Z[] a = RandomPoly(new Random(Seed), n);
        a[0] = 0;
        a[1] = 5;
        Z[] copy = (Z[])a.Clone();
        Poly<Mod998244353> poly = new(a);

        Poly<Mod998244353> pow = poly.Pow(5);
        Z[] expected = new Z[n];
        expected[0] = 1;
        for (int i = 0; i < 5; ++i) expected = NaiveMul(expected, a);
        AssertPolyEqual(expected, pow.AsSpan());
        AssertPolyEqual(copy, a);
    }

    [Fact]
    public void SqrtAcceptsSquaresAndRejectsOddValuation() {
        const int n = 16;
        Z[] a = RandomPoly(new Random(Seed), n);
        a[0] = 2;
        Poly<Mod998244353> poly = new(a);
        Poly<Mod998244353> square = poly * poly;

        Assert.True(square.Sqrt(out Poly<Mod998244353> root));
        Poly<Mod998244353> prod = root * root;
        AssertPolyEqual(square.AsSpan(), prod.AsSpan());

        a = new Z[n];
        a[1] = 1;
        poly = new Poly<Mod998244353>(a);
        Assert.False(poly.Sqrt(out _));
    }

    [Fact]
    public void CompInv() {
        const int n = 16;
        Z[] a = RandomPoly(new Random(Seed), n);
        a[0] = 0;
        a[1] = 3;
        Poly<Mod998244353> poly = new(a);

        Poly<Mod998244353> inv = poly.CompInv();
        Z[] expected = Compose(a, inv.AsSpan());
        Assert.Equal((Z)0, expected[0]);
        Assert.Equal((Z)1, expected[1]);
        for (int i = 2; i < n; ++i) Assert.Equal((Z)0, expected[i]);
    }

    [Fact]
    public void ShiftSample() {
        const int n = 16;
        Z[] input = new Z[n];
        new Z[] { 1, 2, 3, 4, 5 }.CopyTo(input, 0);

        Poly<Mod998244353> shifted = new Poly<Mod998244353>(input).Shift(3);
        Z[] expected = new Z[n];
        new Z[] { 547, 668, 309, 64, 5 }.CopyTo(expected, 0);
        AssertPolyEqual(expected, shifted.AsSpan());
    }

    [Fact]
    public void CompSample() {
        const int n = 16;
        Z[] outer = new Z[n], inner = new Z[n];
        new Z[] { 5, 4, 3, 2, 1 }.CopyTo(outer, 0);
        new Z[] { 0, 1, 2, 3, 4 }.CopyTo(inner, 0);

        Poly<Mod998244353> composed = new Poly<Mod998244353>(outer)
            .Comp(new Poly<Mod998244353>(inner));
        AssertPolyEqual(Compose(outer, inner), composed.AsSpan());
    }

    [Fact]
    public void ShiftAndCompMatchNaiveRandomCases() {
        Random rnd = new(Seed);
        for (int e = 0; e <= 6; ++e) {
            int n = 1 << e;
            for (int t = 0; t < 8; ++t) {
                Z[] outer = RandomPoly(rnd, n);
                Z[] inner = RandomPoly(rnd, n);
                inner[0] = 0;
                Z[] outerCopy = (Z[])outer.Clone();
                Z[] innerCopy = (Z[])inner.Clone();
                Z c = rnd.Next(20);
                Poly<Mod998244353> f = new(outer);
                Poly<Mod998244353> g = new(inner);

                Poly<Mod998244353> shifted = f.Shift(c);
                Poly<Mod998244353> composed = f.Comp(g);
                AssertPolyEqual(NaiveShift(outer, c), shifted.AsSpan());
                AssertPolyEqual(Compose(outer, inner), composed.AsSpan());
                AssertPolyEqual(outerCopy, outer);
                AssertPolyEqual(innerCopy, inner);
            }
        }
    }

    [Fact]
    public void EvalAndChirpZMatchNaive() {
        const int n = 16;
        Random rnd = new(Seed);
        Z[] a = RandomPoly(rnd, n);
        Poly<Mod998244353> poly = new(a);

        Z[] points = RandomPoly(rnd, 13);
        Z[] values = poly.Eval(points);
        for (int i = 0; i < points.Length; ++i) Assert.Equal(Eval(a, points[i]), values[i]);

        Z q = 3, r = 5;
        values = poly.ChirpZ(q, r, 19);
        Z x = r;
        for (int i = 0; i < values.Length; ++i, x *= q) Assert.Equal(Eval(a, x), values[i]);
    }

    private static Z[] RandomPoly(Random rnd, int n) {
        Z[] a = new Z[n];
        for (int i = 0; i < n; ++i) a[i] = rnd.Next(20);
        return a;
    }

    private static Z[] NaiveMul(ReadOnlySpan<Z> a, ReadOnlySpan<Z> b) {
        Z[] res = new Z[a.Length];
        for (int i = 0; i < a.Length; ++i) {
            for (int j = 0; i + j < res.Length; ++j) res[i + j] += a[i] * b[j];
        }
        return res;
    }

    private static Z[] Compose(ReadOnlySpan<Z> f, ReadOnlySpan<Z> g) {
        int n = f.Length;
        Z[] res = new Z[n], pow = new Z[n];
        pow[0] = 1;
        for (int i = 0; i < n; ++i) {
            for (int j = 0; j < n; ++j) res[j] += f[i] * pow[j];
            pow = NaiveMul(pow, g);
        }
        return res;
    }

    private static Z[] NaiveShift(ReadOnlySpan<Z> f, Z c) {
        int n = f.Length;
        Z[] res = new Z[n], pow = new Z[n];
        pow[0] = 1;
        for (int i = 0; i < n; ++i) {
            for (int j = 0; j < n; ++j) res[j] += f[i] * pow[j];
            Z[] next = new Z[n];
            for (int j = 0; j < n; ++j) {
                next[j] += pow[j] * c;
                if (j + 1 < n) next[j + 1] += pow[j];
            }
            pow = next;
        }
        return res;
    }

    private static Z Eval(ReadOnlySpan<Z> f, Z x) {
        Z res = 0;
        for (int i = f.Length - 1; i >= 0; --i) res = res * x + f[i];
        return res;
    }

    private static void AssertPolyEqual(ReadOnlySpan<Z> expected, ReadOnlySpan<Z> actual) {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; ++i) Assert.Equal(expected[i], actual[i]);
    }
}
