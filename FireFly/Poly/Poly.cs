using AtCoder;
using System;

namespace FireFly;

/// <summary>Represents a polynomial over a static modular field modulo x^N for a positive power-of-two N. Copies alias the same coefficient span, while arithmetic operations allocate new storage. Convolution-based operations require transform lengths supported by the modulus.</summary>
public ref partial struct Poly<TMod> where TMod : struct, IStaticMod {
    private Span<StaticModInt<TMod>> coef;

    /// <summary>Creates a zero polynomial modulo x^(2^k).</summary>
    /// <param name="k">The exponent of the truncation length, where 0 &lt;= k &lt; 31.</param>
    public Poly(int k) {
        coef = new StaticModInt<TMod>[1 << k];
    }

    /// <summary>Wraps coefficient storage whose positive length is a power of two.</summary>
    /// <param name="coef">The coefficient storage to alias without copying.</param>
    public Poly(Span<StaticModInt<TMod>> coef) {
        this.coef = coef;
    }

    /// <summary>Gets the truncation length.</summary>
    public int Length => coef.Length;

    /// <summary>Gets a reference to a coefficient.</summary>
    /// <param name="i">The zero-based coefficient index in the range [0, Length).</param>
    /// <returns>A reference to the coefficient.</returns>
    public ref StaticModInt<TMod> this[int i] => ref coef[i];

    /// <summary>Gets the aliased coefficient storage.</summary>
    /// <returns>The wrapped coefficient span, whose writes modify this polynomial.</returns>
    public Span<StaticModInt<TMod>> AsSpan() => coef;

    /// <summary>Adds two polynomials with the same truncation length in O(N) time.</summary>
    /// <param name="f">The first polynomial.</param>
    /// <param name="g">The second polynomial.</param>
    /// <returns>The sum modulo the truncation length.</returns>
    public static Poly<TMod> operator +(Poly<TMod> f, Poly<TMod> g) {
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[f.Length];
        for (int i = 0; i < res.Length; ++i) {
            res[i] = f.coef[i] + g.coef[i];
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Multiplies two polynomials with the same truncation length in O(N log N) time.</summary>
    /// <param name="f">The first polynomial.</param>
    /// <param name="g">The second polynomial under the same modulus.</param>
    /// <returns>The product modulo the truncation length.</returns>
    public static Poly<TMod> operator *(Poly<TMod> f, Poly<TMod> g) {
        StaticModInt<TMod>[] res = MathLib.Convolution<TMod>(f.coef, g.coef);
        return new Poly<TMod>(res.AsSpan(0, f.Length));
    }
}
