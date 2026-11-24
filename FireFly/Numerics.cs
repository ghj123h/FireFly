using System.Numerics;

namespace FireFly;

/// <summary>Provides basic integer algorithms.</summary>
public static class Numerics {
    /// <summary>Computes the greatest common divisor using Euclid's algorithm in O(log(1 + max(|a|, |b|))) arithmetic operations.</summary>
    /// <param name="a">The first integer, whose absolute value must be representable in T.</param>
    /// <param name="b">The second integer, whose absolute value must be representable in T.</param>
    /// <returns>The nonnegative greatest common divisor, or zero when both integers are zero.</returns>
    public static T Gcd<T>(T a, T b) where T : IBinaryInteger<T> {
        a = T.Abs(a);
        b = T.Abs(b);
        while (b != T.Zero) {
            (a, b) = (b, a % b);
        }
        return a;
    }

    /// <summary>Computes the least common multiple in O(log(1 + max(|a|, |b|))) arithmetic operations. The result must be representable in T.</summary>
    /// <param name="a">The first integer, whose absolute value must be representable in T.</param>
    /// <param name="b">The second integer, whose absolute value must be representable in T.</param>
    /// <returns>The nonnegative least common multiple, or zero when either integer is zero.</returns>
    public static T Lcm<T>(T a, T b) where T : IBinaryInteger<T> {
        return a == T.Zero || b == T.Zero ? T.Zero : T.Abs(a / Gcd(a, b) * b);
    }
}
