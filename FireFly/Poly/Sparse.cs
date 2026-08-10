using AtCoder;
using System;

namespace FireFly;

/// <summary>Provides sparse-polynomial operations modulo x^(2^k) from sorted exponent-coefficient pairs.</summary>
public static class SparsePoly {
    /// <summary>Computes the inverse of a sparse polynomial in O(2^k * exp.Length) time.</summary>
    /// <param name="k">The exponent of the truncation length, where 0 &lt;= k &lt; 31.</param>
    /// <param name="exp">The nonempty strictly increasing exponents in [0, 2^k), starting with zero and having the same length as coef.</param>
    /// <param name="coef">The coefficients paired with exp, with an invertible first coefficient.</param>
    /// <returns>The inverse modulo x^(2^k).</returns>
    public static Poly<TMod> Inv<TMod>(
        int k,
        ReadOnlySpan<int> exp,
        ReadOnlySpan<StaticModInt<TMod>> coef)
        where TMod : struct, IStaticMod {
        int n = 1 << k;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        res[0] = coef[0].Inv();
        for (int i = 1; i < n; ++i) {
            for (int j = 1; j < exp.Length && exp[j] <= i; ++j) {
                res[i] += res[i - exp[j]] * coef[j];
            }
            res[i] *= -res[0];
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Computes the formal exponential of a sparse polynomial in O(2^k * exp.Length) time.</summary>
    /// <param name="k">The exponent of the truncation length, where 0 &lt;= k &lt; 31 and 2^k is less than the prime modulus.</param>
    /// <param name="exp">The strictly increasing positive exponents below 2^k, having the same length as coef.</param>
    /// <param name="coef">The coefficients paired with exp; the omitted constant term is zero.</param>
    /// <returns>The exponential modulo x^(2^k).</returns>
    public static Poly<TMod> Exp<TMod>(
        int k,
        ReadOnlySpan<int> exp,
        ReadOnlySpan<StaticModInt<TMod>> coef)
        where TMod : struct, IStaticMod {
        int n = 1 << k;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        ReadOnlySpan<StaticModInt<TMod>> invs = Numerics.GetInvs<TMod>(n);
        res[0] = 1;
        for (int i = 1; i < n; ++i) {
            for (int j = 0; j < exp.Length && exp[j] <= i; ++j) {
                res[i] += res[i - exp[j]] * coef[j] * exp[j];
            }
            res[i] *= invs[i];
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Computes the formal logarithm of a sparse polynomial in O(2^k * exp.Length) time.</summary>
    /// <param name="k">The exponent of the truncation length, where 0 &lt;= k &lt; 31 and 2^k is less than the prime modulus.</param>
    /// <param name="exp">The nonempty strictly increasing exponents in [0, 2^k), starting with zero and having the same length as coef.</param>
    /// <param name="coef">The coefficients paired with exp, with the first coefficient equal to one.</param>
    /// <returns>The logarithm modulo x^(2^k).</returns>
    public static Poly<TMod> Log<TMod>(
        int k,
        ReadOnlySpan<int> exp,
        ReadOnlySpan<StaticModInt<TMod>> coef)
        where TMod : struct, IStaticMod {
        int n = 1 << k;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        ReadOnlySpan<StaticModInt<TMod>> invs = Numerics.GetInvs<TMod>(n);
        for (int i = 1; i < n; ++i) {
            for (int j = 1; j < exp.Length && exp[j] <= i; ++j) {
                res[i] -= res[i - exp[j]] * coef[j] * (i - exp[j]);
                if (i == exp[j]) res[i] += i * coef[j];
            }
            res[i] *= invs[i];
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Raises a sparse polynomial to a nonnegative power in O(2^k * exp.Length) time.</summary>
    /// <param name="k">The exponent of the truncation length, where 0 &lt;= k &lt; 31 and 2^k is less than the prime modulus.</param>
    /// <param name="power">The nonnegative power.</param>
    /// <param name="exp">The strictly increasing exponents of the nonzero terms in [0, 2^k), having the same length as coef.</param>
    /// <param name="coef">The nonzero coefficients paired with exp.</param>
    /// <returns>The power modulo x^(2^k).</returns>
    public static Poly<TMod> Pow<TMod>(
        int k,
        long power,
        ReadOnlySpan<int> exp,
        ReadOnlySpan<StaticModInt<TMod>> coef)
        where TMod : struct, IStaticMod {
        int n = 1 << k;
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        if (power == 0) {
            res[0] = 1;
            return new Poly<TMod>(res);
        }
        if (exp.Length == 0 || (Int128)exp[0] * power >= n) {
            return new Poly<TMod>(res);
        }

        int z = exp[0], shift = (int)((Int128)z * power);
        StaticModInt<TMod> ic = coef[0].Inv();
        StaticModInt<TMod>[] tmp = new StaticModInt<TMod>[n - shift];
        tmp[0] = 1;
        PowInternal(power, exp, coef, z, ic, tmp);
        StaticModInt<TMod> c = coef[0].Pow(power);
        for (int i = 0; i < tmp.Length; ++i) {
            res[i + shift] = tmp[i] * c;
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Computes a square root of a sparse polynomial in expected O(2^k * exp.Length + log p) time under an odd prime modulus p.</summary>
    /// <param name="k">The exponent of the truncation length, where 0 &lt;= k &lt; 31 and 2^k is less than the modulus.</param>
    /// <param name="exp">The strictly increasing exponents of the nonzero terms in [0, 2^k), having the same length as coef.</param>
    /// <param name="coef">The nonzero coefficients paired with exp.</param>
    /// <param name="res">A square root when one exists, or the default polynomial otherwise.</param>
    /// <returns>Whether a square root exists.</returns>
    public static bool Sqrt<TMod>(
        int k,
        ReadOnlySpan<int> exp,
        ReadOnlySpan<StaticModInt<TMod>> coef,
        out Poly<TMod> res)
        where TMod : struct, IStaticMod {
        int n = 1 << k;
        StaticModInt<TMod>[] ans = new StaticModInt<TMod>[n];
        if (exp.Length == 0) {
            res = new Poly<TMod>(ans);
            return true;
        }
        if ((exp[0] & 1) != 0 || !coef[0].Sqrt(out StaticModInt<TMod> sq)) {
            res = default;
            return false;
        }

        int z = exp[0], shift = z >> 1;
        StaticModInt<TMod>[] tmp = new StaticModInt<TMod>[n - shift];
        tmp[0] = 1;
        StaticModInt<TMod> inv2 = new StaticModInt<TMod>(2).Inv();
        PowInternal(inv2, exp, coef, z, coef[0].Inv(), tmp);
        for (int i = 0; i < tmp.Length; ++i) {
            ans[i + shift] = tmp[i] * sq;
        }
        res = new Poly<TMod>(ans);
        return true;
    }

    private static void PowInternal<TMod>(
        StaticModInt<TMod> power,
        ReadOnlySpan<int> exp,
        ReadOnlySpan<StaticModInt<TMod>> coef,
        int z,
        StaticModInt<TMod> ic,
        Span<StaticModInt<TMod>> res)
        where TMod : struct, IStaticMod {
        ReadOnlySpan<StaticModInt<TMod>> invs = Numerics.GetInvs<TMod>(res.Length);
        for (int i = 1; i < res.Length; ++i) {
            for (int j = 1; j < exp.Length; ++j) {
                int e = exp[j] - z;
                if (e > i) break;
                res[i] += (power * e - i + e) * coef[j] * ic * res[i - e];
            }
            res[i] *= invs[i];
        }
    }
}
