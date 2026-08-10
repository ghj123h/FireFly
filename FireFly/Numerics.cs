using AtCoder;
using AtCoder.Internal;
using System;

namespace FireFly;

/// <summary>Provides cached modular combinatorics and modular square-root operations. Returned spans alias shared per-modulus caches, and the members are not thread-safe.</summary>
public static class Numerics {
    static Random rnd = new();

    private interface ITableOp<TMod> where TMod : struct, IStaticMod {
        static abstract StaticModInt<TMod>[] Init();
        static abstract void Extend(StaticModInt<TMod>[] values, int from, int to);
    }

    private static class Table<TMod, TOp>
        where TMod : struct, IStaticMod
        where TOp : struct, ITableOp<TMod> {
        private static StaticModInt<TMod>[] values = TOp.Init();
        private static int count = values.Length;

        public static ReadOnlySpan<StaticModInt<TMod>> Get(int n) {
            if (count <= n) {
                if (values.Length <= n) {
                    int len = Math.Min(StaticModInt<TMod>.Mod, Math.Max(values.Length << 1, n + 1));
                    Array.Resize(ref values, len);
                }
                TOp.Extend(values, count, n + 1);
                count = n + 1;
            }
            return values.AsSpan(0, n + 1);
        }
    }

    private readonly struct InvOp<TMod> : ITableOp<TMod> where TMod : struct, IStaticMod {
        public static StaticModInt<TMod>[] Init() => new StaticModInt<TMod>[] { 0, 1 };

        public static void Extend(StaticModInt<TMod>[] values, int from, int to) {
            int mod = StaticModInt<TMod>.Mod;
            for (int i = from; i < to; ++i) {
                values[i] = StaticModInt<TMod>.Raw(mod - mod / i) * values[mod % i];
            }
        }
    }

    private readonly struct FactOp<TMod> : ITableOp<TMod> where TMod : struct, IStaticMod {
        public static StaticModInt<TMod>[] Init() => new StaticModInt<TMod>[] { 1 };

        public static void Extend(StaticModInt<TMod>[] values, int from, int to) {
            for (int i = from; i < to; ++i) {
                values[i] = values[i - 1] * i;
            }
        }
    }

    private readonly struct InvFactOp<TMod> : ITableOp<TMod> where TMod : struct, IStaticMod {
        public static StaticModInt<TMod>[] Init() => new StaticModInt<TMod>[] { 1 };

        public static void Extend(StaticModInt<TMod>[] values, int from, int to) {
            ReadOnlySpan<StaticModInt<TMod>> facts = GetFacts<TMod>(to - 1);
            values[to - 1] = facts[to - 1].Inv();
            for (int i = to - 1; i > from; --i) {
                values[i - 1] = values[i] * i;
            }
        }
    }

    /// <summary>Gets modular inverses through n; a cache hit is O(1), and extending the cache by d entries takes O(d) time.</summary>
    /// <param name="n">The largest index, where 0 &lt;= n &lt; the prime modulus.</param>
    /// <returns>A read-only view of the shared cache whose element i is the inverse of i, with element zero equal to zero.</returns>
    public static ReadOnlySpan<StaticModInt<TMod>> GetInvs<TMod>(int n)
        where TMod : struct, IStaticMod => Table<TMod, InvOp<TMod>>.Get(n);

    /// <summary>Gets factorials through n; a cache hit is O(1), and extending the cache by d entries takes O(d) time.</summary>
    /// <param name="n">The largest index, where 0 &lt;= n &lt; the modulus.</param>
    /// <returns>A read-only view of the shared cache whose element i is i factorial.</returns>
    public static ReadOnlySpan<StaticModInt<TMod>> GetFacts<TMod>(int n)
        where TMod : struct, IStaticMod => Table<TMod, FactOp<TMod>>.Get(n);

    /// <summary>Gets inverse factorials through n; a cache hit is O(1), and extending the cache by d entries takes O(d + log p) time under modulus p.</summary>
    /// <param name="n">The largest index, where 0 &lt;= n &lt; the prime modulus.</param>
    /// <returns>A read-only view of the shared cache whose element i is the inverse of i factorial.</returns>
    public static ReadOnlySpan<StaticModInt<TMod>> GetInvFacts<TMod>(int n)
        where TMod : struct, IStaticMod => Table<TMod, InvFactOp<TMod>>.Get(n);

    /// <summary>Computes a binomial coefficient in O(1) time after caching through n; the first extension through n takes O(n + log p) time under modulus p.</summary>
    /// <param name="n">The nonnegative number of elements, less than the prime modulus.</param>
    /// <param name="k">The number to choose.</param>
    /// <returns>The binomial coefficient, or zero when k is outside [0, n].</returns>
    public static StaticModInt<TMod> Binom<TMod>(int n, int k) where TMod : struct, IStaticMod {
        if (k < 0 || k > n) return 0;
        ReadOnlySpan<StaticModInt<TMod>> facts = GetFacts<TMod>(n);
        ReadOnlySpan<StaticModInt<TMod>> invFacts = GetInvFacts<TMod>(n);
        return facts[n] * invFacts[k] * invFacts[n - k];
    }

    /// <summary>Computes the number of ordered selections in O(1) time after caching through n; the first extension through n takes O(n + log p) time under modulus p.</summary>
    /// <param name="n">The nonnegative number of elements, less than the prime modulus.</param>
    /// <param name="k">The number to select.</param>
    /// <returns>The permutation count, or zero when k is outside [0, n].</returns>
    public static StaticModInt<TMod> Perm<TMod>(int n, int k) where TMod : struct, IStaticMod {
        if (k < 0 || k > n) return 0;
        ReadOnlySpan<StaticModInt<TMod>> facts = GetFacts<TMod>(n);
        ReadOnlySpan<StaticModInt<TMod>> invFacts = GetInvFacts<TMod>(n);
        return facts[n] * invFacts[n - k];
    }

    private static bool SqrtExists<T>(this T value) where T : IModInt<T> {
        return value.Pow((ulong)(T.Mod >> 1)) == T.One;
    }

    /// <summary>Computes a modular square root under a prime modulus in expected O(log p) time.</summary>
    /// <param name="value">The residue whose square root is required.</param>
    /// <param name="result">The smaller square root when one exists, or zero otherwise.</param>
    /// <returns>Whether the residue has a square root.</returns>
    public static bool Sqrt<T>(this T value, out T result) where T : IModInt<T> {
        if (!InternalMath.IsPrime(T.Mod)) {
            throw new InvalidOperationException();
        }
        int val = value.Value;
        result = T.Zero;
        if (val == 0) {
            return true;
        } else if (val == 1) {
            result = T.One;
            return true;
        }
        if (SqrtExists(value)) {
            T r = T.CreateChecked(rnd.Next(1, T.Mod));
            T v = r * r - value;
            while (true) {
                if (v != T.Zero && !SqrtExists(v)) break;
                r = T.CreateChecked(rnd.Next(1, T.Mod));
                v = r * r - value;
            }
            int exp = (T.Mod + 1) >> 1;
            T res0 = T.One, res1 = T.Zero;
            T base0 = r, base1 = T.One;
            while (exp > 0) {
                if (exp % 2 == 1) {
                    (res0, res1) = (res0 * base0 + res1 * base1 * v, res0 * base1 + res1 * base0);
                }
                (base0, base1) = (base0 * base0 + base1 * base1 * v, base0 * base1 + base1 * base0);
                exp >>= 1;
            }
            result = res0.Value > (T.Zero - res0).Value ? T.Zero - res0 : res0;
            return true;
        } else {
            return false;
        }
    }
}
