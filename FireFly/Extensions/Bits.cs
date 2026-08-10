using System.Numerics;

namespace FireFly.Extensions;

/// <summary>Provides bit-mask enumeration helpers.</summary>
public static class Bits {
    /// <summary>Enumerates the values of the set bits from least to most significant in O(popcount(x)) time.</summary>
    /// <param name="x">The nonnegative bit mask.</param>
    /// <returns>The powers of two corresponding to the set bits.</returns>
    public static IEnumerable<int> GetLowbits(this int x) {
        while (x > 0) {
            int low = x & -x;
            yield return low;
            x ^= low;
        }
    }

    /// <summary>Enumerates the positions of the set bits from least to most significant in O(popcount(x)) time.</summary>
    /// <param name="x">The nonnegative bit mask.</param>
    /// <returns>The zero-based positions of the set bits.</returns>
    public static IEnumerable<int> GetSignificantBits(this int x) {
        while (x > 0) {
            int low = x & -x;
            yield return BitOperations.TrailingZeroCount(low);
            x ^= low;
        }
    }

    /// <summary>Enumerates all n-bit masks with exactly k set bits in increasing order and O(C(n, k)) time.</summary>
    /// <param name="n">The number of available low-order bits, where 1 &lt;= n &lt; 31.</param>
    /// <param name="k">The number of set bits, where 1 &lt;= k &lt;= n.</param>
    /// <returns>The masks with exactly k set bits.</returns>
    public static IEnumerable<int> Gosper(int n, int k) {
        int s = (1 << k) - 1, m = 1 << n;
        while (s < m) {
            yield return s;
            int low = s & -s;
            int r = s + low;
            s = ((s ^ r) >> (BitOperations.TrailingZeroCount(low) + 2)) | r;
        }
    }
}
