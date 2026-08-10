using AtCoder;
using System;

namespace FireFly;

public ref partial struct Poly<TMod> where TMod : struct, IStaticMod {
    /// <summary>Evaluates the polynomial at multiple points in O((N + m) log^2(N + m)) time and O((N + m) log(N + m)) auxiliary space.</summary>
    /// <param name="x">The evaluation points.</param>
    /// <returns>The values at the given points.</returns>
    public StaticModInt<TMod>[] Eval(ReadOnlySpan<StaticModInt<TMod>> x) {
        int m = x.Length;
        if (m == 0) return Array.Empty<StaticModInt<TMod>>();
        int n = Math.Max(coef.Length, m);
        StaticModInt<TMod>[] f = new StaticModInt<TMod>[n + 1];
        StaticModInt<TMod>[] a = new StaticModInt<TMod>[n];
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        StaticModInt<TMod>[][] seg = new StaticModInt<TMod>[n << 2][];
        coef.CopyTo(f);
        x.CopyTo(a);
        EvalInit(seg, a, 1, 0, n - 1);
        EvalWork(seg, res, 1, 0, n - 1, MulT(f, Inverse(seg[1]), n));
        Array.Resize(ref res, m);
        return res;
    }

    /// <summary>Evaluates the polynomial on a geometric progression in O((N + m) log(N + m)) time and O(N + m) auxiliary space.</summary>
    /// <param name="q">The common ratio.</param>
    /// <param name="r">The first point.</param>
    /// <param name="m">The nonnegative number of points.</param>
    /// <returns>The values at r*q^i for 0 &lt;= i &lt; m.</returns>
    public StaticModInt<TMod>[] ChirpZ(
        StaticModInt<TMod> q,
        StaticModInt<TMod> r,
        int m) {
        int n = coef.Length;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[m];
        if (m == 0) return res;
        if (q == 0) {
            res[0] = coef[^1];
            for (int i = n - 2; i >= 0; --i) {
                res[0] = res[0] * r + coef[i];
            }
            for (int i = 1; i < m; ++i) {
                res[i] = coef[0];
            }
            return res;
        }

        StaticModInt<TMod>[] a = new StaticModInt<TMod>[n];
        StaticModInt<TMod>[] b = new StaticModInt<TMod>[n + m - 1];
        StaticModInt<TMod> iq = q.Inv();
        for (int i = 0; i < n; ++i) {
            long c = (long)i * (i - 1) / 2;
            a[i] = coef[i] * iq.Pow(c) * r.Pow(i);
        }
        Array.Reverse(a);
        for (int i = 0; i < b.Length; ++i) {
            long c = (long)i * (i - 1) / 2;
            b[i] = q.Pow(c);
        }
        StaticModInt<TMod>[] conv = MathLib.Convolution(a, b);
        for (int i = 0; i < m; ++i) {
            long c = (long)i * (i - 1) / 2;
            res[i] = iq.Pow(c) * conv[n + i - 1];
        }
        return res;
    }

    private static StaticModInt<TMod>[] MulT(
        StaticModInt<TMod>[] f,
        StaticModInt<TMod>[] g,
        int n) {
        Array.Reverse(g);
        StaticModInt<TMod>[] p = MathLib.Convolution(f, g);
        Array.Reverse(g);
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        Array.Copy(p, g.Length - 1, res, 0, Math.Min(f.Length, n));
        return res;
    }

    private static void EvalInit(
        StaticModInt<TMod>[][] seg,
        StaticModInt<TMod>[] x,
        int i,
        int l,
        int r) {
        if (l == r) {
            seg[i] = new StaticModInt<TMod>[] { 1, -x[l] };
            return;
        }
        int mid = l + (r - l) / 2;
        EvalInit(seg, x, i << 1, l, mid);
        EvalInit(seg, x, i << 1 | 1, mid + 1, r);
        seg[i] = MathLib.Convolution(seg[i << 1], seg[i << 1 | 1]);
    }

    private static void EvalWork(
        StaticModInt<TMod>[][] seg,
        StaticModInt<TMod>[] res,
        int i,
        int l,
        int r,
        StaticModInt<TMod>[] f) {
        if (l == r) {
            res[l] = f[0];
            return;
        }
        int mid = l + (r - l) / 2;
        EvalWork(seg, res, i << 1, l, mid, MulT(f, seg[i << 1 | 1], mid - l + 1));
        EvalWork(seg, res, i << 1 | 1, mid + 1, r, MulT(f, seg[i << 1], r - mid));
    }
}
