using AtCoder;
using AtCoder.Internal;
using System;

namespace FireFly;

public ref partial struct Poly<TMod> where TMod : struct, IStaticMod {
    /// <summary>Computes the multiplicative inverse in O(N log N) time when the constant term is invertible.</summary>
    /// <returns>The inverse modulo the truncation length.</returns>
    public Poly<TMod> Inv() {
        int n = coef.Length;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod>[] tmp = new StaticModInt<TMod>[n << 1];
        res[0] = coef[0].Inv();
        for (int t = 2; t <= n; t <<= 1) {
            int t2 = t << 1;
            coef[..t].CopyTo(tmp);
            tmp.AsSpan(t, t).Clear();
            Butterfly<TMod>.Calculate(res.AsSpan(0, t2));
            Butterfly<TMod>.Calculate(tmp.AsSpan(0, t2));
            for (int i = 0; i < t2; ++i) {
                res[i] *= 2 - res[i] * tmp[i];
            }
            Butterfly<TMod>.CalculateInv(res.AsSpan(0, t2));
            res.AsSpan(t, t).Clear();
            StaticModInt<TMod> it2 = new StaticModInt<TMod>(t2).Inv();
            for (int i = 0; i < t; ++i) {
                res[i] *= it2;
            }
        }
        return new Poly<TMod>(res.AsSpan(0, n));
    }

    /// <summary>Computes the formal logarithm in O(N log N) time when the constant term is one and N is less than the prime modulus.</summary>
    /// <returns>The logarithm modulo the truncation length.</returns>
    public Poly<TMod> Log() {
        int n = coef.Length, n2 = n << 1;
        Poly<TMod> inv = Inv();
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n2];
        StaticModInt<TMod>[] d = new StaticModInt<TMod>[n2];
        inv.AsSpan().CopyTo(res);
        for (int i = 1; i < n; ++i) {
            d[i - 1] = coef[i] * i;
        }
        Butterfly<TMod>.Calculate(res);
        Butterfly<TMod>.Calculate(d);
        for (int i = 0; i < n2; ++i) {
            res[i] *= d[i];
        }
        Butterfly<TMod>.CalculateInv(res);
        StaticModInt<TMod> in2 = new StaticModInt<TMod>(n2).Inv();
        ReadOnlySpan<StaticModInt<TMod>> invs = Numerics.GetInvs<TMod>(n);
        for (int i = 0; i < n; ++i) {
            res[i] *= in2;
        }
        for (int i = n - 1; i > 0; --i) {
            res[i] = res[i - 1] * invs[i];
        }
        res[0] = 0;
        return new Poly<TMod>(res.AsSpan(0, n));
    }

    /// <summary>Computes the formal exponential in O(N log N) time when the constant term is zero and N is less than the prime modulus.</summary>
    /// <returns>The exponential modulo the truncation length.</returns>
    public Poly<TMod> Exp() {
        int n = coef.Length;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod>[] tmp = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod>[] tmp2 = new StaticModInt<TMod>[n << 1];
        Numerics.GetInvs<TMod>(n);
        res[0] = 1;
        for (int t = 2; t <= n; t <<= 1) {
            int t2 = t << 1;
            coef[..t].CopyTo(tmp);
            tmp.AsSpan(t, t).Clear();
            Poly<TMod> log = new Poly<TMod>(res.AsSpan(0, t)).Log();
            log.AsSpan().CopyTo(tmp2);
            tmp2.AsSpan(t, t).Clear();
            Butterfly<TMod>.Calculate(res.AsSpan(0, t2));
            Butterfly<TMod>.Calculate(tmp.AsSpan(0, t2));
            Butterfly<TMod>.Calculate(tmp2.AsSpan(0, t2));
            for (int i = 0; i < t2; ++i) {
                res[i] *= 1 - tmp2[i] + tmp[i];
            }
            Butterfly<TMod>.CalculateInv(res.AsSpan(0, t2));
            res.AsSpan(t, t).Clear();
            StaticModInt<TMod> it2 = new StaticModInt<TMod>(t2).Inv();
            for (int i = 0; i < t; ++i) {
                res[i] *= it2;
            }
        }
        return new Poly<TMod>(res.AsSpan(0, n));
    }

    /// <summary>Raises the polynomial to a nonnegative power in O(N log N) time when N is less than the prime modulus.</summary>
    /// <param name="k">The nonnegative power.</param>
    /// <returns>The power modulo the truncation length.</returns>
    public Poly<TMod> Pow(long k) {
        int n = coef.Length;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        if (k == 0) {
            res[0] = 1;
            return new Poly<TMod>(res);
        }
        int z = 0;
        while (z < n && coef[z] == 0) ++z;
        if (z == n || (Int128)z * k >= n) {
            return new Poly<TMod>(res);
        }

        StaticModInt<TMod> c = coef[z];
        StaticModInt<TMod> ic = c.Inv();
        StaticModInt<TMod>[] f = new StaticModInt<TMod>[n];
        for (int i = z; i < n; ++i) {
            f[i - z] = coef[i] * ic;
        }
        Poly<TMod> log = new Poly<TMod>(f).Log();
        for (int i = 0; i < n; ++i) {
            log.coef[i] *= k;
        }
        Poly<TMod> exp = log.Exp();
        int shift = (int)((Int128)z * k);
        c = c.Pow(k);
        for (int i = 0; i + shift < n; ++i) {
            res[i + shift] = exp.coef[i] * c;
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Computes a formal square root in expected O(N log N + log p) time under an odd prime modulus p with N less than p.</summary>
    /// <param name="res">A square root when one exists, or the default polynomial otherwise.</param>
    /// <returns>Whether a square root exists.</returns>
    public bool Sqrt(out Poly<TMod> res) {
        int n = coef.Length, z = 0;
        StaticModInt<TMod>[] ans = new StaticModInt<TMod>[n];
        while (z < n && coef[z] == 0) ++z;
        if (z == n) {
            res = new Poly<TMod>(ans);
            return true;
        }
        if ((z & 1) != 0 || !coef[z].Sqrt(out StaticModInt<TMod> sq)) {
            res = default;
            return false;
        }

        StaticModInt<TMod> ic = coef[z].Inv();
        StaticModInt<TMod>[] f = new StaticModInt<TMod>[n];
        for (int i = z; i < n; ++i) {
            f[i - z] = coef[i] * ic;
        }
        Poly<TMod> root = SqrtInternal(new Poly<TMod>(f));
        int shift = z >> 1;
        for (int i = 0; i + shift < n; ++i) {
            ans[i + shift] = root.coef[i] * sq;
        }
        res = new Poly<TMod>(ans);
        return true;
    }

    /// <summary>Computes the compositional inverse in O(N log^2 N) time when N is greater than one and less than the prime modulus, the constant term is zero, and the linear term is invertible.</summary>
    /// <returns>The compositional inverse modulo the truncation length.</returns>
    public Poly<TMod> CompInv() {
        int n = coef.Length;
        StaticModInt<TMod> inv = coef[1].Inv();
        ReadOnlySpan<StaticModInt<TMod>> invs = Numerics.GetInvs<TMod>(n);
        StaticModInt<TMod>[][] p = new StaticModInt<TMod>[n][];
        StaticModInt<TMod>[][] q = new StaticModInt<TMod>[n][];
        for (int i = 0; i < n; ++i) {
            p[i] = new StaticModInt<TMod>[] { i == 0 ? 1 : 0, 0 };
            q[i] = new StaticModInt<TMod>[] { i == 0 ? 1 : 0, -coef[i] * inv };
        }
        StaticModInt<TMod>[] g = BostanMori(n - 1, p, q);
        StaticModInt<TMod>[] h = new StaticModInt<TMod>[n];
        Array.Resize(ref g, n);
        for (int i = 0; i < n; ++i) {
            h[n - i - 1] = g[i] * (n - 1) * invs[i];
        }
        StaticModInt<TMod> w = 1;
        for (int i = 0; i < n; ++i, w *= inv) {
            h[i] *= w;
        }
        long k = (long)StaticModInt<TMod>.Mod - invs[n - 1].Value;
        Poly<TMod> res = new Poly<TMod>(h).Pow(k);
        res.coef[..^1].CopyTo(res.coef[1..]);
        res.coef[0] = 0;
        for (int i = 0; i < n; ++i) {
            res.coef[i] *= inv;
        }
        return res;
    }

    private static Poly<TMod> SqrtInternal(Poly<TMod> f) {
        int n = f.Length;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod>[] inv = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod>[] tmp = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod> inv2 = new StaticModInt<TMod>(2).Inv();
        inv[0] = res[0] = 1;
        for (int t = 2; t <= n; t <<= 1) {
            int t2 = t << 1;
            f.coef[..t].CopyTo(tmp);
            tmp.AsSpan(t, t).Clear();
            Butterfly<TMod>.Calculate(res.AsSpan(0, t2));
            Butterfly<TMod>.Calculate(inv.AsSpan(0, t2));
            Butterfly<TMod>.Calculate(tmp.AsSpan(0, t2));
            for (int i = 0; i < t2; ++i) {
                res[i] += (tmp[i] - res[i] * res[i]) * inv2 * inv[i];
            }
            Butterfly<TMod>.CalculateInv(res.AsSpan(0, t2));
            res.AsSpan(t, t).Clear();
            StaticModInt<TMod> it2 = new StaticModInt<TMod>(t2).Inv();
            for (int i = 0; i < t; ++i) {
                res[i] *= it2;
            }
            if (t == n) break;
            res.AsSpan(0, t2).CopyTo(tmp);
            Butterfly<TMod>.Calculate(tmp.AsSpan(0, t2));
            for (int i = 0; i < t2; ++i) {
                inv[i] *= 2 - inv[i] * tmp[i];
            }
            Butterfly<TMod>.CalculateInv(inv.AsSpan(0, t2));
            inv.AsSpan(t, t).Clear();
            for (int i = 0; i < t; ++i) {
                inv[i] *= it2;
            }
        }
        return new Poly<TMod>(res.AsSpan(0, n));
    }

    private static StaticModInt<TMod>[][] Convolution2D(
        StaticModInt<TMod>[][] a,
        StaticModInt<TMod>[][] b) {
        int n = a.Length, m = b.Length;
        int p = a[0].Length, q = b[0].Length, s = p + q - 1;
        StaticModInt<TMod>[] x = new StaticModInt<TMod>[n * s];
        StaticModInt<TMod>[] y = new StaticModInt<TMod>[m * s];
        for (int i = 0; i < n; ++i) {
            a[i].CopyTo(x, i * s);
        }
        for (int i = 0; i < m; ++i) {
            b[i].CopyTo(y, i * s);
        }
        x = MathLib.Convolution(x, y);
        StaticModInt<TMod>[][] res = new StaticModInt<TMod>[n + m - 1][];
        for (int i = 0; i < res.Length; ++i) {
            res[i] = new StaticModInt<TMod>[s];
            Array.Copy(x, i * s, res[i], 0, s);
        }
        return res;
    }

    private static StaticModInt<TMod>[] BostanMori(
        int n,
        StaticModInt<TMod>[][] a,
        StaticModInt<TMod>[][] b) {
        if (n == 0) {
            return MathLib.Convolution(a[0], Inverse(b[0]));
        }
        if (n + 1 < a.Length) Array.Resize(ref a, n + 1);
        if (n + 1 < b.Length) Array.Resize(ref b, n + 1);
        StaticModInt<TMod>[][] c = new StaticModInt<TMod>[b.Length][];
        for (int i = 0; i < b.Length; ++i) {
            c[i] = new StaticModInt<TMod>[b[i].Length];
            for (int j = 0; j < b[i].Length; ++j) {
                c[i][j] = (i & 1) == 0 ? b[i][j] : -b[i][j];
            }
        }
        a = Convolution2D(a, c);
        b = Convolution2D(b, c);
        StaticModInt<TMod>[][] x = new StaticModInt<TMod>[(a.Length - (n & 1) + 1) >> 1][];
        StaticModInt<TMod>[][] y = new StaticModInt<TMod>[(b.Length + 1) >> 1][];
        for (int i = n & 1; i < a.Length; i += 2) {
            x[i >> 1] = a[i];
        }
        for (int i = 0; i < b.Length; i += 2) {
            y[i >> 1] = b[i];
        }
        return BostanMori(n >> 1, x, y);
    }

    private static StaticModInt<TMod>[] Inverse(ReadOnlySpan<StaticModInt<TMod>> f) {
        int n = 1 << InternalBit.CeilPow2(f.Length);
        StaticModInt<TMod>[] a = new StaticModInt<TMod>[n];
        f.CopyTo(a);
        Poly<TMod> inv = new Poly<TMod>(a).Inv();
        return inv.coef[..f.Length].ToArray();
    }
}
