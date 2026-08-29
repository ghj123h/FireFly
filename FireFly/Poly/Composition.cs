using AtCoder;
using AtCoder.Internal;
using System;
using System.Numerics;

namespace FireFly;

public ref partial struct Poly<TMod> where TMod : struct, IStaticMod {
    /// <summary>Computes f(x + c) in O(N log N) time and O(N) auxiliary space when N is less than the prime modulus and a length-2N transform is supported.</summary>
    /// <param name="c">The constant shift.</param>
    /// <returns>The shifted polynomial with the same truncation length.</returns>
    public Poly<TMod> Shift(StaticModInt<TMod> c) {
        int n = coef.Length;
        ReadOnlySpan<StaticModInt<TMod>> fact = Numerics.GetFacts<TMod>(n - 1);
        ReadOnlySpan<StaticModInt<TMod>> invFact = Numerics.GetInvFacts<TMod>(n - 1);
        StaticModInt<TMod>[] a = new StaticModInt<TMod>[n];
        StaticModInt<TMod>[] b = new StaticModInt<TMod>[n];
        StaticModInt<TMod> p = 1;
        for (int i = 0; i < n; ++i) {
            a[n - i - 1] = coef[i] * fact[i];
            b[i] = p * invFact[i];
            p *= c;
        }
        a = MathLib.Convolution(a, b);
        StaticModInt<TMod>[] res = new StaticModInt<TMod>[n];
        for (int i = 0; i < n; ++i) {
            res[i] = a[n - i - 1] * invFact[i];
        }
        return new Poly<TMod>(res);
    }

    /// <summary>Computes f(g(x)) modulo x^N in O(M(N) log N) time, where M(N) is polynomial multiplication time, and O(N log N) auxiliary space when g has the same truncation length and zero constant term and the prime modulus supports a length-2N transform.</summary>
    /// <param name="g">The inner polynomial.</param>
    /// <returns>The composition modulo the truncation length.</returns>
    public Poly<TMod> Comp(Poly<TMod> g) {
        int n = coef.Length;
        StaticModInt<TMod>[] f = coef.ToArray();
        if (n == 1) return new Poly<TMod>(f);

        int log = BitOperations.TrailingZeroCount((uint)n);
        StaticModInt<TMod>[] w = new StaticModInt<TMod>[n];
        StaticModInt<TMod>[] iw = new StaticModInt<TMod>[n];
        StaticModInt<TMod> dw = CompositionNtt.IRoots[log + 1];
        StaticModInt<TMod> idw = CompositionNtt.Roots[log + 1];
        StaticModInt<TMod> p = 1, ip = 1;
        int r = 0;
        for (int i = 0; i < n; ++i) {
            w[r] = p;
            iw[r] = ip;
            p *= dw;
            ip *= idw;
            int bit = n >> 1;
            while ((r & bit) != 0) {
                r ^= bit;
                bit >>= 1;
            }
            r ^= bit;
        }

        StaticModInt<TMod>[] q = new StaticModInt<TMod>[n << 2];
        for (int i = 0; i < n; ++i) q[i] = -g.coef[i];
        StaticModInt<TMod>[] tmp = new StaticModInt<TMod>[n << 1];
        StaticModInt<TMod>[] tmp2 = new StaticModInt<TMod>[n];
        StaticModInt<TMod>[] res = CompRec(f, w, iw, tmp, tmp2, n, 1, q);
        Array.Resize(ref res, n);
        Array.Reverse(res);
        return new Poly<TMod>(res);
    }

    private static StaticModInt<TMod>[] CompRec(
        StaticModInt<TMod>[] f,
        StaticModInt<TMod>[] w,
        StaticModInt<TMod>[] iw,
        StaticModInt<TMod>[] tmp,
        StaticModInt<TMod>[] tmp2,
        int n,
        int k,
        StaticModInt<TMod>[] q) {
        // n * k remains equal to the truncation length.
        if (n == 1) {
            Array.Reverse(f);
            CompositionNtt.TransposeInverse(f);
            StaticModInt<TMod> ik = new StaticModInt<TMod>(k).Inv();
            StaticModInt<TMod>[] ans = new StaticModInt<TMod>[k << 2];
            for (int i = 0; i < k; ++i) ans[i << 1] = f[i] * ik;
            return ans;
        }

        if (n <= k) {
            CompDoubleY(q, iw, tmp.AsSpan(0, k), n, k, 1, n, false);
            CompTransformX(q, tmp.AsSpan(0, n << 1), n, 0, k << 1, false);
        } else {
            CompTransformX(q, tmp.AsSpan(0, n << 1), n, 0, k, false);
            CompDoubleY(q, iw, tmp.AsSpan(0, k), n, k, 0, n << 1, false);
        }

        int nk2 = n * k << 1;
        for (int i = 0; i < nk2; ++i) q[i] += 1;
        for (int i = nk2; i < nk2 << 1; ++i) q[i] -= 1;

        StaticModInt<TMod>[] next = new StaticModInt<TMod>[q.Length];
        Span<StaticModInt<TMod>> row = tmp.AsSpan(0, n);
        for (int y = 0; y < k << 1; ++y) {
            int off = (n << 1) * y;
            for (int x = 0; x < n; ++x) {
                row[x] = q[off + (x << 1)] * q[off + (x << 1) + 1];
            }
            CompositionNtt.Inverse(row);
            row[..(n >> 1)].CopyTo(next.AsSpan(n * y));
        }
        for (int y = 0; y < k << 2; ++y) next[n * y] = 0;

        StaticModInt<TMod>[] res = CompRec(f, w, iw, tmp, tmp2, n >> 1, k << 1, next);
        Span<StaticModInt<TMod>> b = tmp2.AsSpan(0, n);
        Span<StaticModInt<TMod>> c = tmp.AsSpan(0, n << 1);
        for (int y = (k << 1) - 1; y >= 0; --y) {
            res.AsSpan(n * y, n >> 1).CopyTo(b);
            b[(n >> 1)..].Clear();
            CompositionNtt.TransposeInverse(b);
            int off = (n << 1) * y;
            for (int x = 0; x < n; ++x) {
                b[x] *= w[x];
                c[x << 1] = q[off + (x << 1) + 1] * b[x];
                c[(x << 1) + 1] = -q[off + (x << 1)] * b[x];
            }
            c.CopyTo(res.AsSpan(off));
        }

        if (n <= k) {
            CompTransformX(res, tmp.AsSpan(0, n << 1), n, 0, k << 1, true);
            CompDoubleY(res, iw, tmp.AsSpan(0, k), n, k, 0, n, true);
        } else {
            CompDoubleY(res, iw, tmp.AsSpan(0, k), n, k, 0, n << 1, true);
            CompTransformX(res, tmp.AsSpan(0, n << 1), n, 0, k, true);
        }
        return res;
    }

    private static void CompDoubleY(
        StaticModInt<TMod>[] a,
        StaticModInt<TMod>[] iw,
        Span<StaticModInt<TMod>> b,
        int n,
        int k,
        int l,
        int r,
        bool transpose) {
        StaticModInt<TMod> z = iw[k >> 1];
        if (!transpose) {
            for (int x = l; x < r; ++x) {
                for (int y = 0; y < k; ++y) b[y] = a[(n << 1) * y + x];
                CompositionNtt.Inverse(b);
                StaticModInt<TMod> p = 1;
                for (int y = 1; y < k; ++y) {
                    p *= z;
                    b[y] *= p;
                }
                CompositionNtt.Forward(b);
                for (int y = 0; y < k; ++y) a[(n << 1) * (k + y) + x] = b[y];
            }
        } else {
            for (int x = l; x < r; ++x) {
                for (int y = 0; y < k; ++y) b[y] = a[(n << 1) * (k + y) + x];
                CompositionNtt.TransposeForward(b);
                StaticModInt<TMod> p = 1;
                for (int y = 1; y < k; ++y) {
                    p *= z;
                    b[y] *= p;
                }
                CompositionNtt.TransposeInverse(b);
                for (int y = 0; y < k; ++y) a[(n << 1) * y + x] += b[y];
            }
        }
    }

    private static void CompTransformX(
        StaticModInt<TMod>[] a,
        Span<StaticModInt<TMod>> b,
        int n,
        int l,
        int r,
        bool transpose) {
        int len = n << 1;
        for (int y = l; y < r; ++y) {
            a.AsSpan(len * y, len).CopyTo(b);
            if (transpose) CompositionNtt.TransposeForward(b);
            else CompositionNtt.Forward(b);
            b.CopyTo(a.AsSpan(len * y));
        }
    }

    // These radix-4 transforms and their matrix transposes share the same ordering.
    private static class CompositionNtt {
        public static readonly StaticModInt<TMod>[] Roots;
        public static readonly StaticModInt<TMod>[] IRoots;
        private static readonly StaticModInt<TMod>[] rate2;
        private static readonly StaticModInt<TMod>[] irate2;
        private static readonly StaticModInt<TMod>[] rate3;
        private static readonly StaticModInt<TMod>[] irate3;

        static CompositionNtt() {
            int mod = StaticModInt<TMod>.Mod;
            int maxBase = BitOperations.TrailingZeroCount((uint)(mod - 1));
            Roots = new StaticModInt<TMod>[maxBase + 1];
            IRoots = new StaticModInt<TMod>[maxBase + 1];
            rate2 = new StaticModInt<TMod>[maxBase + 1];
            irate2 = new StaticModInt<TMod>[maxBase + 1];
            rate3 = new StaticModInt<TMod>[maxBase + 1];
            irate3 = new StaticModInt<TMod>[maxBase + 1];

            StaticModInt<TMod> root = new StaticModInt<TMod>(InternalMath.PrimitiveRoot<TMod>());
            Roots[maxBase] = root.Pow((mod - 1) >> maxBase);
            IRoots[maxBase] = Roots[maxBase].Inv();
            for (int i = maxBase - 1; i >= 0; --i) {
                Roots[i] = Roots[i + 1] * Roots[i + 1];
                IRoots[i] = IRoots[i + 1] * IRoots[i + 1];
            }

            StaticModInt<TMod> p = 1, ip = 1;
            for (int i = 0; i <= maxBase - 2; ++i) {
                rate2[i] = Roots[i + 2] * p;
                irate2[i] = IRoots[i + 2] * ip;
                p *= IRoots[i + 2];
                ip *= Roots[i + 2];
            }
            p = 1;
            ip = 1;
            for (int i = 0; i <= maxBase - 3; ++i) {
                rate3[i] = Roots[i + 3] * p;
                irate3[i] = IRoots[i + 3] * ip;
                p *= IRoots[i + 3];
                ip *= Roots[i + 3];
            }
        }

        public static void Forward(Span<StaticModInt<TMod>> a) {
            int h = BitOperations.TrailingZeroCount((uint)a.Length);
            int len = 0;
            StaticModInt<TMod> imag = Roots[2];
            if ((h & 1) != 0) {
                int p = 1 << (h - 1);
                for (int i = 0; i < p; ++i) {
                    StaticModInt<TMod> r = a[i + p];
                    a[i + p] = a[i] - r;
                    a[i] += r;
                }
                ++len;
            }
            for (; len + 1 < h; len += 2) {
                int p = 1 << (h - len - 2);
                for (int i = 0; i < p; ++i) {
                    StaticModInt<TMod> a0 = a[i];
                    StaticModInt<TMod> a1 = a[i + p];
                    StaticModInt<TMod> a2 = a[i + 2 * p];
                    StaticModInt<TMod> a3 = a[i + 3 * p];
                    StaticModInt<TMod> x = (a1 - a3) * imag;
                    StaticModInt<TMod> a0a2 = a0 + a2;
                    StaticModInt<TMod> a1a3 = a1 + a3;
                    StaticModInt<TMod> a0na2 = a0 - a2;
                    a[i] = a0a2 + a1a3;
                    a[i + p] = a0a2 - a1a3;
                    a[i + 2 * p] = a0na2 + x;
                    a[i + 3 * p] = a0na2 - x;
                }
                StaticModInt<TMod> rot = rate3[0];
                for (int s = 1; s < 1 << len; ++s) {
                    int off = s << (h - len);
                    StaticModInt<TMod> rot2 = rot * rot;
                    StaticModInt<TMod> rot3 = rot2 * rot;
                    for (int i = 0; i < p; ++i) {
                        StaticModInt<TMod> a0 = a[i + off];
                        StaticModInt<TMod> a1 = a[i + off + p] * rot;
                        StaticModInt<TMod> a2 = a[i + off + 2 * p] * rot2;
                        StaticModInt<TMod> a3 = a[i + off + 3 * p] * rot3;
                        StaticModInt<TMod> x = (a1 - a3) * imag;
                        StaticModInt<TMod> a0a2 = a0 + a2;
                        StaticModInt<TMod> a1a3 = a1 + a3;
                        StaticModInt<TMod> a0na2 = a0 - a2;
                        a[i + off] = a0a2 + a1a3;
                        a[i + off + p] = a0a2 - a1a3;
                        a[i + off + 2 * p] = a0na2 + x;
                        a[i + off + 3 * p] = a0na2 - x;
                    }
                    rot *= rate3[BitOperations.TrailingZeroCount(~(uint)s)];
                }
            }
        }

        public static void Inverse(Span<StaticModInt<TMod>> a) {
            int h = BitOperations.TrailingZeroCount((uint)a.Length);
            int len = h;
            StaticModInt<TMod> iimag = IRoots[2];
            for (; len > 1; len -= 2) {
                int p = 1 << (h - len);
                for (int i = 0; i < p; ++i) {
                    StaticModInt<TMod> a0 = a[i];
                    StaticModInt<TMod> a1 = a[i + p];
                    StaticModInt<TMod> a2 = a[i + 2 * p];
                    StaticModInt<TMod> a3 = a[i + 3 * p];
                    StaticModInt<TMod> x = (a2 - a3) * iimag;
                    StaticModInt<TMod> a0na1 = a0 - a1;
                    StaticModInt<TMod> a0a1 = a0 + a1;
                    StaticModInt<TMod> a2a3 = a2 + a3;
                    a[i] = a0a1 + a2a3;
                    a[i + p] = a0na1 + x;
                    a[i + 2 * p] = a0a1 - a2a3;
                    a[i + 3 * p] = a0na1 - x;
                }
                StaticModInt<TMod> irot = irate3[0];
                for (int s = 1; s < 1 << (len - 2); ++s) {
                    int off = s << (h - len + 2);
                    StaticModInt<TMod> irot2 = irot * irot;
                    StaticModInt<TMod> irot3 = irot2 * irot;
                    for (int i = 0; i < p; ++i) {
                        StaticModInt<TMod> a0 = a[i + off];
                        StaticModInt<TMod> a1 = a[i + off + p];
                        StaticModInt<TMod> a2 = a[i + off + 2 * p];
                        StaticModInt<TMod> a3 = a[i + off + 3 * p];
                        StaticModInt<TMod> x = (a2 - a3) * iimag;
                        StaticModInt<TMod> a0na1 = a0 - a1;
                        StaticModInt<TMod> a0a1 = a0 + a1;
                        StaticModInt<TMod> a2a3 = a2 + a3;
                        a[i + off] = a0a1 + a2a3;
                        a[i + off + p] = (a0na1 + x) * irot;
                        a[i + off + 2 * p] = (a0a1 - a2a3) * irot2;
                        a[i + off + 3 * p] = (a0na1 - x) * irot3;
                    }
                    irot *= irate3[BitOperations.TrailingZeroCount(~(uint)s)];
                }
            }
            if (len >= 1) {
                int p = 1 << (h - 1);
                for (int i = 0; i < p; ++i) {
                    StaticModInt<TMod> x = a[i] - a[i + p];
                    a[i] += a[i + p];
                    a[i + p] = x;
                }
            }
            StaticModInt<TMod> inv = new StaticModInt<TMod>(a.Length).Inv();
            for (int i = 0; i < a.Length; ++i) a[i] *= inv;
        }

        public static void TransposeForward(Span<StaticModInt<TMod>> a) {
            int h = BitOperations.TrailingZeroCount((uint)a.Length);
            int len = h;
            StaticModInt<TMod> imag = Roots[2];
            while (len > 0) {
                if (len == 1) {
                    int p = 1 << (h - len);
                    StaticModInt<TMod> rot = 1;
                    for (int s = 0; s < 1 << (len - 1); ++s) {
                        int off = s << (h - len + 1);
                        for (int i = 0; i < p; ++i) {
                            StaticModInt<TMod> l = a[i + off];
                            StaticModInt<TMod> r = a[i + off + p];
                            a[i + off] = l + r;
                            a[i + off + p] = (l - r) * rot;
                        }
                        rot *= rate2[BitOperations.TrailingZeroCount(~(uint)s)];
                    }
                    --len;
                } else {
                    int p = 1 << (h - len);
                    StaticModInt<TMod> rot = 1;
                    for (int s = 0; s < 1 << (len - 2); ++s) {
                        int off = s << (h - len + 2);
                        StaticModInt<TMod> rot2 = rot * rot;
                        StaticModInt<TMod> rot3 = rot2 * rot;
                        for (int i = 0; i < p; ++i) {
                            StaticModInt<TMod> a0 = a[i + off];
                            StaticModInt<TMod> a1 = a[i + off + p];
                            StaticModInt<TMod> a2 = a[i + off + 2 * p];
                            StaticModInt<TMod> a3 = a[i + off + 3 * p];
                            StaticModInt<TMod> x = (a2 - a3) * imag;
                            a[i + off] = a0 + a1 + a2 + a3;
                            a[i + off + p] = (a0 - a1 + x) * rot;
                            a[i + off + 2 * p] = (a0 + a1 - a2 - a3) * rot2;
                            a[i + off + 3 * p] = (a0 - a1 - x) * rot3;
                        }
                        rot *= rate3[BitOperations.TrailingZeroCount(~(uint)s)];
                    }
                    len -= 2;
                }
            }
        }

        public static void TransposeInverse(Span<StaticModInt<TMod>> a) {
            StaticModInt<TMod> inv = new StaticModInt<TMod>(a.Length).Inv();
            for (int i = 0; i < a.Length; ++i) a[i] *= inv;

            int h = BitOperations.TrailingZeroCount((uint)a.Length);
            int len = 0;
            StaticModInt<TMod> iimag = IRoots[2];
            while (len < h) {
                if (len == h - 1) {
                    int p = 1 << (h - len - 1);
                    StaticModInt<TMod> irot = 1;
                    for (int s = 0; s < 1 << len; ++s) {
                        int off = s << (h - len);
                        for (int i = 0; i < p; ++i) {
                            StaticModInt<TMod> l = a[i + off];
                            StaticModInt<TMod> r = a[i + off + p] * irot;
                            a[i + off] = l + r;
                            a[i + off + p] = l - r;
                        }
                        irot *= irate2[BitOperations.TrailingZeroCount(~(uint)s)];
                    }
                    ++len;
                } else {
                    int p = 1 << (h - len - 2);
                    StaticModInt<TMod> irot = 1;
                    for (int s = 0; s < 1 << len; ++s) {
                        StaticModInt<TMod> irot2 = irot * irot;
                        StaticModInt<TMod> irot3 = irot2 * irot;
                        int off = s << (h - len);
                        for (int i = 0; i < p; ++i) {
                            StaticModInt<TMod> a0 = a[i + off];
                            StaticModInt<TMod> a1 = a[i + off + p] * irot;
                            StaticModInt<TMod> a2 = a[i + off + 2 * p] * irot2;
                            StaticModInt<TMod> a3 = a[i + off + 3 * p] * irot3;
                            StaticModInt<TMod> x = (a1 - a3) * iimag;
                            a[i + off] = a0 + a2 + a1 + a3;
                            a[i + off + p] = a0 + a2 - a1 - a3;
                            a[i + off + 2 * p] = a0 - a2 + x;
                            a[i + off + 3 * p] = a0 - a2 - x;
                        }
                        irot *= irate3[BitOperations.TrailingZeroCount(~(uint)s)];
                    }
                    len += 2;
                }
            }
        }
    }
}
