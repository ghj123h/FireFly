using AtCoder;
using FireFly.Internal;
using System;

namespace FireFly;

/// <summary>Provides cached modular combinatorics. Returned spans alias shared per-modulus caches, and the members are not thread-safe.</summary>
public static class Comb {
    private readonly struct FactOp<TMod> : ITableOp<TMod> where TMod : struct, IStaticMod {
        public static StaticModInt<TMod>[] Init() => [1];

        public static void Extend(StaticModInt<TMod>[] values, int from, int to) {
            for (int i = from; i < to; ++i) {
                values[i] = values[i - 1] * i;
            }
        }
    }

    private readonly struct InvFactOp<TMod> : ITableOp<TMod> where TMod : struct, IStaticMod {
        public static StaticModInt<TMod>[] Init() => [1];

        public static void Extend(StaticModInt<TMod>[] values, int from, int to) {
            ReadOnlySpan<StaticModInt<TMod>> facts = GetFacts<TMod>(to - 1);
            values[to - 1] = facts[to - 1].Inv();
            for (int i = to - 1; i > from; --i) {
                values[i - 1] = values[i] * i;
            }
        }
    }

    /// <summary>Gets factorials through n; a cache hit is O(1), and growing the cache through n takes O(n) total time.</summary>
    /// <param name="n">The largest index, where 0 &lt;= n &lt; the modulus.</param>
    /// <returns>A read-only view of the shared cache whose element i is i factorial.</returns>
    public static ReadOnlySpan<StaticModInt<TMod>> GetFacts<TMod>(int n)
        where TMod : struct, IStaticMod => Table<TMod, FactOp<TMod>>.Get(n);

    /// <summary>Gets inverse factorials through n; a cache hit is O(1), and growing the cache through n takes O(n + log(n + 1) log p) total time under modulus p.</summary>
    /// <param name="n">The largest index, where 0 &lt;= n &lt; the prime modulus.</param>
    /// <returns>A read-only view of the shared cache whose element i is the inverse of i factorial.</returns>
    public static ReadOnlySpan<StaticModInt<TMod>> GetInvFacts<TMod>(int n)
        where TMod : struct, IStaticMod => Table<TMod, InvFactOp<TMod>>.Get(n);

    /// <summary>Computes a binomial coefficient in O(1) time after caching through n; growing the cache through n takes O(n + log(n + 1) log p) total time under modulus p.</summary>
    /// <param name="n">The nonnegative number of elements, less than the prime modulus.</param>
    /// <param name="k">The number to choose.</param>
    /// <returns>The binomial coefficient, or zero when k is outside [0, n].</returns>
    public static StaticModInt<TMod> Binom<TMod>(int n, int k) where TMod : struct, IStaticMod {
        if (k < 0 || k > n) return 0;
        ReadOnlySpan<StaticModInt<TMod>> facts = GetFacts<TMod>(n);
        ReadOnlySpan<StaticModInt<TMod>> invFacts = GetInvFacts<TMod>(n);
        return facts[n] * invFacts[k] * invFacts[n - k];
    }

    /// <summary>Computes the number of ordered selections in O(1) time after caching through n; growing the cache through n takes O(n + log(n + 1) log p) total time under modulus p.</summary>
    /// <param name="n">The nonnegative number of elements, less than the prime modulus.</param>
    /// <param name="k">The number to select.</param>
    /// <returns>The permutation count, or zero when k is outside [0, n].</returns>
    public static StaticModInt<TMod> Perm<TMod>(int n, int k) where TMod : struct, IStaticMod {
        if (k < 0 || k > n) return 0;
        ReadOnlySpan<StaticModInt<TMod>> facts = GetFacts<TMod>(n);
        ReadOnlySpan<StaticModInt<TMod>> invFacts = GetInvFacts<TMod>(n);
        return facts[n] * invFacts[n - k];
    }
}
