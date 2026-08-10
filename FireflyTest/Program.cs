// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using FireFly.IO;
using Z = AtCoder.StaticModInt<AtCoder.Mod998244353>;
using DZ = AtCoder.DynamicModInt<AtCoder.DynamicModIntId0>;
using FireFly;
using AtCoder;
using FireFly.DS;
using FireFly.Graph;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Text;

SourceExpander.Expander.Expand();
if (args.Length > 0 && args[0] == "--numerics-tests") {
    TestNumerics();
    return;
}
if (args.Length > 0 && args[0] == "--buffered-writer-tests") {
    TestBufferedWriter();
    return;
}
if (args.Length > 0 && args[0] == "--treap-tests") {
    TestTreap();
    return;
}

BufferedReader br = new(Console.OpenStandardInput(), 1 << 16);
BufferedWriter bw = new();
int n = br.ReadInt32(), m = br.ReadInt32();
Graph<Directed, NoWeight> G = new(n, m);
while (m-- > 0) {
    int u = br.ReadInt32() - 1, v = br.ReadInt32() - 1;
    G.AddEdge(v, u);
}
var res = G.TopoSort();
bw.AppendJoin(res!);
bw.Output();

static void TestTreap() {
    TestTreapBasics();
    TestTreapComparer();
    TestTreapAggregate();
    TestTreapRandom();
    Console.WriteLine("Treap tests passed.");
}

static void TestTreapBasics() {
    Treap<int> tree = new(16);
    List<int> values = new();
    CheckTreap("empty", tree, values);

    int[] input = new[] { 0, 5, 5, -2, int.MinValue, int.MaxValue, 3, 0 };
    foreach (int value in input) {
        tree.Add(value);
        values.Add(value);
    }
    values.Sort();
    CheckTreap("initial values", tree, values);

    if (!tree.Remove(5)) throw new Exception("Treap failed to remove a repeated value.");
    values.Remove(5);
    if (tree.Remove(4)) throw new Exception("Treap removed a missing value.");
    if (!tree.Remove(int.MinValue)) throw new Exception("Treap failed to remove int.MinValue.");
    values.Remove(int.MinValue);
    CheckTreap("removals", tree, values);

    tree.Clear();
    values.Clear();
    CheckTreap("clear", tree, values);
    tree.Add(0);
    values.Add(0);
    CheckTreap("default value", tree, values);

    if (typeof(Treap<int>).GetProperty(nameof(Treap<int>.Comparer))!.CanWrite) {
        throw new Exception("Treap comparer is still writable.");
    }
}

static void TestTreapComparer() {
    Treap<string> words = new(4, StringComparer.OrdinalIgnoreCase);
    words.Add("A");
    words.Add("a");
    words.Add("b");
    if (words.Count != 3 || words.CountOf("a") != 2 || words[0] != "A" || words[1] != "A") {
        throw new Exception("Treap comparer-equivalent representative failed.");
    }
    if (!words.TryPrev("b", out string prev) || prev != "A") {
        throw new Exception("Treap custom-comparer predecessor failed.");
    }
    if (!words.TryNext("a", out string next) || next != "b") {
        throw new Exception("Treap custom-comparer successor failed.");
    }
    if (!words.Remove("a") || words.CountOf("A") != 1 || words[0] != "A") {
        throw new Exception("Treap custom-comparer removal failed.");
    }

    IComparer<int> abs = Comparer<int>.Create((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));
    Treap<int, long, TreapSumOp> sum = new(abs);
    sum.Add(-2);
    sum.Add(2);
    if (sum.Count != 2 || sum[0] != -2 || sum[1] != -2 || sum.AllProd != -4) {
        throw new Exception("Treap aggregate did not use the stored representative.");
    }
}

static void TestTreapAggregate() {
    Treap<int, long, TreapSumOp> tree = new(16);
    List<int> values = new();
    int[] input = new[] { 5, 1, 5, -3, 0, 8, 1 };
    foreach (int value in input) {
        tree.Add(value);
        values.Add(value);
    }
    values.Sort();
    CheckSumTreap("sum initial", tree, values);

    tree.Remove(5);
    values.Remove(5);
    tree.Remove(1);
    values.Remove(1);
    CheckSumTreap("sum removals", tree, values);

    tree.Clear();
    values.Clear();
    CheckSumTreap("sum clear", tree, values);

    Treap<char, string, TreapStringOp> text = new();
    foreach (char c in new[] { 'c', 'a', 'b', 'b' }) text.Add(c);
    if (text.AllProd != "abbc" || text.Prod(1, 3) != "bb" || text.Prod(2, 2) != string.Empty) {
        throw new Exception("Treap noncommutative aggregate order failed.");
    }
}

static void TestTreapRandom() {
    const int iterations = 10000;
    Random rnd = new(20260810);
    Treap<int> tree = new(128);
    Treap<int, long, TreapSumOp> sum = new(128);
    List<int> values = new();

    for (int step = 0; step < iterations; ++step) {
        int value = rnd.Next(-100, 101);
        if (rnd.Next(2) == 0) {
            tree.Add(value);
            sum.Add(value);
            values.Insert(NaiveLowerBound(values, value), value);
        } else {
            int p = NaiveLowerBound(values, value);
            bool expected = p < values.Count && values[p] == value;
            bool removed = tree.Remove(value);
            bool sumRemoved = sum.Remove(value);
            if (removed != expected || sumRemoved != expected) {
                throw new Exception($"Treap random removal failed at step {step}.");
            }
            if (expected) values.RemoveAt(p);
        }

        if (step % 23 == 0) {
            CheckTreap($"random step {step}", tree, values);
            CheckRandomProd($"random step {step}", sum, values, rnd);
        }
    }
    CheckTreap("random final", tree, values);
    CheckRandomProd("random final", sum, values, rnd);
}

static void CheckTreap(string name, Treap<int> tree, List<int> expected) {
    if (tree.Count != expected.Count) {
        throw new Exception($"Treap {name} count: expected {expected.Count}, got {tree.Count}.");
    }

    int i = 0;
    foreach (int value in tree) {
        if (i >= expected.Count || value != expected[i]) {
            throw new Exception($"Treap {name} enumeration failed at {i}.");
        }
        ++i;
    }
    if (i != expected.Count) throw new Exception($"Treap {name} enumeration ended early.");

    for (i = 0; i < expected.Count; ++i) {
        if (tree[i] != expected[i]) throw new Exception($"Treap {name} index failed at {i}.");
    }

    const int marker = 123456789;
    int[] copy = new int[expected.Count + 2];
    Array.Fill(copy, marker);
    tree.CopyTo(copy, 1);
    if (copy[0] != marker || copy[copy.Length - 1] != marker) {
        throw new Exception($"Treap {name} CopyTo wrote outside its range.");
    }
    for (i = 0; i < expected.Count; ++i) {
        if (copy[i + 1] != expected[i]) throw new Exception($"Treap {name} CopyTo failed at {i}.");
    }

    int[] probes = new[] { int.MinValue, -101, -5, -1, 0, 1, 5, 101, int.MaxValue };
    foreach (int value in probes) {
        int l = NaiveLowerBound(expected, value);
        int r = NaiveUpperBound(expected, value);
        if (tree.LowerBound(value) != l || tree.UpperBound(value) != r) {
            throw new Exception($"Treap {name} bound failed for {value}.");
        }
        if (tree.CountOf(value) != r - l || tree.Contains(value) != (l < r)) {
            throw new Exception($"Treap {name} membership failed for {value}.");
        }

        bool hasPrev = l > 0;
        bool foundPrev = tree.TryPrev(value, out int prev);
        int expectedPrev = hasPrev ? expected[l - 1] : default;
        if (foundPrev != hasPrev || prev != expectedPrev) {
            throw new Exception($"Treap {name} predecessor failed for {value}.");
        }

        bool hasNext = r < expected.Count;
        bool foundNext = tree.TryNext(value, out int next);
        int expectedNext = hasNext ? expected[r] : default;
        if (foundNext != hasNext || next != expectedNext) {
            throw new Exception($"Treap {name} successor failed for {value}.");
        }
    }
}

static void CheckSumTreap(string name, Treap<int, long, TreapSumOp> tree, List<int> expected) {
    if (tree.Count != expected.Count) throw new Exception($"Treap {name} count failed.");
    long all = 0;
    for (int i = 0; i < expected.Count; ++i) {
        all += expected[i];
        if (tree[i] != expected[i]) throw new Exception($"Treap {name} index failed at {i}.");
    }
    if (tree.AllProd != all) throw new Exception($"Treap {name} AllProd failed.");

    for (int l = 0; l <= expected.Count; ++l) {
        long prod = 0;
        for (int r = l; r <= expected.Count; ++r) {
            if (tree.Prod(l, r) != prod) {
                throw new Exception($"Treap {name} Prod failed for [{l}, {r}).");
            }
            if (r < expected.Count) prod += expected[r];
        }
    }

    for (int k = 0; k <= expected.Count; ++k) {
        long top = 0;
        for (int i = expected.Count - k; i < expected.Count; ++i) top += expected[i];
        if (tree.Prod(expected.Count - k, expected.Count) != top) {
            throw new Exception($"Treap {name} top-{k} sum failed.");
        }
    }
}

static void CheckRandomProd(
    string name,
    Treap<int, long, TreapSumOp> tree,
    List<int> expected,
    Random rnd) {
    if (tree.Count != expected.Count) throw new Exception($"Treap {name} aggregate count failed.");
    long all = 0;
    foreach (int value in expected) all += value;
    if (tree.AllProd != all) throw new Exception($"Treap {name} aggregate total failed.");

    int index = 0;
    foreach (int value in tree) {
        if (index >= expected.Count || value != expected[index]) {
            throw new Exception($"Treap {name} aggregate enumeration failed at {index}.");
        }
        ++index;
    }
    if (index != expected.Count) throw new Exception($"Treap {name} aggregate enumeration ended early.");

    for (int q = 0; q < 16; ++q) {
        int l = rnd.Next(expected.Count + 1);
        int r = rnd.Next(expected.Count + 1);
        if (l > r) (l, r) = (r, l);
        long prod = 0;
        for (int i = l; i < r; ++i) prod += expected[i];
        if (tree.Prod(l, r) != prod) {
            throw new Exception($"Treap {name} random Prod failed for [{l}, {r}).");
        }
    }
}

static int NaiveLowerBound(List<int> values, int value) {
    int l = 0, r = values.Count;
    while (l < r) {
        int m = (l + r) >> 1;
        if (values[m] < value) l = m + 1;
        else r = m;
    }
    return l;
}

static int NaiveUpperBound(List<int> values, int value) {
    int l = 0, r = values.Count;
    while (l < r) {
        int m = (l + r) >> 1;
        if (values[m] <= value) l = m + 1;
        else r = m;
    }
    return l;
}

static void TestNumerics() {
    ReadOnlySpan<Z> first = Numerics.GetInvs<Mod998244353>(4);
    if (first.Length != 5 || first[0] != 0) throw new Exception("GetInvs initial prefix failed.");
    for (int i = 1; i < first.Length; ++i) {
        if (first[i] * i != 1) throw new Exception($"GetInvs failed at {i}.");
    }

    ReadOnlySpan<Z> grown = Numerics.GetInvs<Mod998244353>(1000);
    for (int i = 1; i < grown.Length; ++i) {
        if (grown[i] * i != 1) throw new Exception($"GetInvs growth failed at {i}.");
    }
    for (int i = 1; i < first.Length; ++i) {
        if (first[i] * i != 1) throw new Exception($"GetInvs snapshot failed at {i}.");
    }

    ReadOnlySpan<Z> prefix = Numerics.GetInvs<Mod998244353>(7);
    if (prefix.Length != 8) throw new Exception("GetInvs prefix length failed.");

    ReadOnlySpan<StaticModInt<Mod1000000007>> other = Numerics.GetInvs<Mod1000000007>(50);
    for (int i = 1; i < other.Length; ++i) {
        if (other[i] * i != 1) throw new Exception($"GetInvs generic cache failed at {i}.");
    }

    ReadOnlySpan<Z> facts = Numerics.GetFacts<Mod998244353>(5);
    if (facts.Length != 6 || facts[0] != 1) throw new Exception("GetFacts initial prefix failed.");
    for (int i = 1; i < facts.Length; ++i) {
        if (facts[i] != facts[i - 1] * i) throw new Exception($"GetFacts failed at {i}.");
    }

    ReadOnlySpan<Z> invFacts = Numerics.GetInvFacts<Mod998244353>(5);
    for (int i = 0; i < invFacts.Length; ++i) {
        if (facts[i] * invFacts[i] != 1) throw new Exception($"GetInvFacts failed at {i}.");
    }

    ReadOnlySpan<Z> grownFacts = Numerics.GetFacts<Mod998244353>(1000);
    ReadOnlySpan<Z> grownInvFacts = Numerics.GetInvFacts<Mod998244353>(1000);
    for (int i = 0; i < grownFacts.Length; ++i) {
        if (grownFacts[i] * grownInvFacts[i] != 1) {
            throw new Exception($"GetInvFacts growth failed at {i}.");
        }
    }
    for (int i = 0; i < facts.Length; ++i) {
        if (facts[i] * invFacts[i] != 1) throw new Exception($"Factorial snapshot failed at {i}.");
    }

    if (Numerics.Binom<Mod998244353>(5, 2) != 10 ||
        Numerics.Binom<Mod998244353>(5, 0) != 1 ||
        Numerics.Binom<Mod998244353>(5, 5) != 1) {
        throw new Exception("Binom failed.");
    }
    if (Numerics.Binom<Mod998244353>(5, -1) != 0 ||
        Numerics.Binom<Mod998244353>(5, 6) != 0) {
        throw new Exception("Binom range failed.");
    }

    if (Numerics.Perm<Mod998244353>(5, 2) != 20 ||
        Numerics.Perm<Mod998244353>(5, 0) != 1 ||
        Numerics.Perm<Mod998244353>(5, 5) != 120) {
        throw new Exception("Perm failed.");
    }
    if (Numerics.Perm<Mod998244353>(5, -1) != 0 ||
        Numerics.Perm<Mod998244353>(5, 6) != 0) {
        throw new Exception("Perm range failed.");
    }

    Z[] f = new Z[8];
    f[1] = 1;
    Poly<Mod998244353> poly = new(f);
    Poly<Mod998244353> exp = poly.Exp();
    Z fact = 1;
    for (int i = 0; i < exp.Length; ++i) {
        if (i > 0) fact *= i;
        if (exp[i] * fact != 1) throw new Exception($"Poly.Exp failed at {i}.");
    }

    TestPoly();

    Console.WriteLine("Numerics tests passed.");
}

static void TestPoly() {
    const int k = 4;
    const int n = 1 << k;
    Random rnd = new(20260810);

    Z[] wrapped = new Z[n];
    Poly<Mod998244353> view = new(wrapped);
    view[3] = 7;
    if (view.Length != n || wrapped[3] != 7) throw new Exception("Poly span wrapping failed.");
    Span<Z> stack = stackalloc Z[8];
    Poly<Mod998244353> stackView = new(stack);
    stackView[2] = 11;
    if (stack[2] != 11) throw new Exception("Poly stack span wrapping failed.");
    Poly<Mod998244353> zero = new(k);
    CheckPoly("Poly allocation", new Z[n], zero.AsSpan());

    Z[] a = RandomPoly(rnd, n), b = RandomPoly(rnd, n);
    Z[] aa = (Z[])a.Clone(), bb = (Z[])b.Clone();
    Poly<Mod998244353> pa = new(a), pb = new(b);
    Poly<Mod998244353> sum = pa + pb;
    Z[] expected = new Z[n];
    for (int i = 0; i < n; ++i) expected[i] = a[i] + b[i];
    CheckPoly("Poly operator +", expected, sum.AsSpan());
    Poly<Mod998244353> prod = pa * pb;
    CheckPoly("Poly operator *", NaiveMul(a, b), prod.AsSpan());
    CheckPoly("Poly operator left input", aa, a);
    CheckPoly("Poly operator right input", bb, b);

    a[0] = 3;
    aa = (Z[])a.Clone();
    pa = new Poly<Mod998244353>(a);
    Poly<Mod998244353> inv = pa.Inv();
    expected = NaiveMul(a, inv.AsSpan());
    if (expected[0] != 1) throw new Exception("Poly.Inv constant term failed.");
    for (int i = 1; i < n; ++i) {
        if (expected[i] != 0) throw new Exception($"Poly.Inv failed at {i}.");
    }
    CheckPoly("Poly.Inv input", aa, a);

    a = RandomPoly(rnd, n);
    a[0] = 0;
    pa = new Poly<Mod998244353>(a);
    Poly<Mod998244353> exp = pa.Exp();
    Poly<Mod998244353> log = exp.Log();
    CheckPoly("Poly Log/Exp", a, log.AsSpan());

    a[0] = 0;
    a[1] = 5;
    aa = (Z[])a.Clone();
    pa = new Poly<Mod998244353>(a);
    Poly<Mod998244353> pow = pa.Pow(5);
    expected = new Z[n];
    expected[0] = 1;
    for (int i = 0; i < 5; ++i) expected = NaiveMul(expected, a);
    CheckPoly("Poly.Pow", expected, pow.AsSpan());
    CheckPoly("Poly.Pow input", aa, a);

    a = RandomPoly(rnd, n);
    a[0] = 2;
    pa = new Poly<Mod998244353>(a);
    Poly<Mod998244353> square = pa * pa;
    if (!square.Sqrt(out Poly<Mod998244353> root)) throw new Exception("Poly.Sqrt rejected a square.");
    prod = root * root;
    CheckPoly("Poly.Sqrt", square.AsSpan(), prod.AsSpan());
    a = new Z[n];
    a[1] = 1;
    pa = new Poly<Mod998244353>(a);
    if (pa.Sqrt(out _)) throw new Exception("Poly.Sqrt accepted odd valuation.");

    a = RandomPoly(rnd, n);
    a[0] = 0;
    a[1] = 3;
    pa = new Poly<Mod998244353>(a);
    Poly<Mod998244353> compInv = pa.CompInv();
    expected = Compose(a, compInv.AsSpan());
    if (expected[0] != 0 || expected[1] != 1) throw new Exception("Poly.CompInv prefix failed.");
    for (int i = 2; i < n; ++i) {
        if (expected[i] != 0) throw new Exception($"Poly.CompInv failed at {i}.");
    }

    Z[] points = RandomPoly(rnd, 13);
    Z[] values = pa.Eval(points);
    for (int i = 0; i < points.Length; ++i) {
        if (values[i] != Eval(a, points[i])) throw new Exception($"Poly.Eval failed at {i}.");
    }
    Z q = 3, r = 5;
    values = pa.ChirpZ(q, r, 19);
    Z x = r;
    for (int i = 0; i < values.Length; ++i, x *= q) {
        if (values[i] != Eval(a, x)) throw new Exception($"Poly.ChirpZ failed at {i}.");
    }

    int[] sparseExp = new int[] { 0, 2, 5, 9 };
    Z[] sparseCoef = new Z[] { 1, 3, 4, 2 };
    int[] sparseExpCopy = (int[])sparseExp.Clone();
    Z[] sparseCoefCopy = (Z[])sparseCoef.Clone();
    a = Dense(n, sparseExp, sparseCoef);
    Poly<Mod998244353> sparse = SparsePoly.Inv<Mod998244353>(k, sparseExp, sparseCoef);
    CheckPoly("SparsePoly.Inv", new Poly<Mod998244353>(a).Inv().AsSpan(), sparse.AsSpan());
    sparse = SparsePoly.Log<Mod998244353>(k, sparseExp, sparseCoef);
    CheckPoly("SparsePoly.Log", new Poly<Mod998244353>(a).Log().AsSpan(), sparse.AsSpan());

    int[] expExp = new int[] { 1, 4, 7 };
    Z[] expCoef = new Z[] { 2, 5, 3 };
    a = Dense(n, expExp, expCoef);
    sparse = SparsePoly.Exp<Mod998244353>(k, expExp, expCoef);
    CheckPoly("SparsePoly.Exp", new Poly<Mod998244353>(a).Exp().AsSpan(), sparse.AsSpan());

    int[] powExp = new int[] { 2, 5, 9 };
    Z[] powCoef = new Z[] { 3, 7, 4 };
    a = Dense(n, powExp, powCoef);
    sparse = SparsePoly.Pow<Mod998244353>(k, 3, powExp, powCoef);
    CheckPoly("SparsePoly.Pow", new Poly<Mod998244353>(a).Pow(3).AsSpan(), sparse.AsSpan());

    int[] sqrtExp = new int[] { 2, 5, 9 };
    Z[] sqrtCoef = new Z[] { 1, 3, 8 };
    a = Dense(n, sqrtExp, sqrtCoef);
    Poly<Mod998244353> dense = new(a);
    if (!dense.Sqrt(out root) || !SparsePoly.Sqrt<Mod998244353>(k, sqrtExp, sqrtCoef, out sparse)) {
        throw new Exception("SparsePoly.Sqrt rejected a square-rootable polynomial.");
    }
    CheckPoly("SparsePoly.Sqrt", root.AsSpan(), sparse.AsSpan());
    CheckPoly("SparsePoly coefficients", sparseCoefCopy, sparseCoef);
    for (int i = 0; i < sparseExp.Length; ++i) {
        if (sparseExp[i] != sparseExpCopy[i]) throw new Exception("SparsePoly modified exponents.");
    }
}

static Z[] RandomPoly(Random rnd, int n) {
    Z[] a = new Z[n];
    for (int i = 0; i < n; ++i) a[i] = rnd.Next(20);
    return a;
}

static Z[] Dense(int n, ReadOnlySpan<int> exp, ReadOnlySpan<Z> coef) {
    Z[] a = new Z[n];
    for (int i = 0; i < exp.Length; ++i) a[exp[i]] = coef[i];
    return a;
}

static Z[] NaiveMul(ReadOnlySpan<Z> a, ReadOnlySpan<Z> b) {
    Z[] res = new Z[a.Length];
    for (int i = 0; i < a.Length; ++i) {
        for (int j = 0; i + j < res.Length; ++j) {
            res[i + j] += a[i] * b[j];
        }
    }
    return res;
}

static Z[] Compose(ReadOnlySpan<Z> f, ReadOnlySpan<Z> g) {
    int n = f.Length;
    Z[] res = new Z[n], pow = new Z[n];
    pow[0] = 1;
    for (int i = 0; i < n; ++i) {
        for (int j = 0; j < n; ++j) res[j] += f[i] * pow[j];
        pow = NaiveMul(pow, g);
    }
    return res;
}

static Z Eval(ReadOnlySpan<Z> f, Z x) {
    Z res = 0;
    for (int i = f.Length - 1; i >= 0; --i) res = res * x + f[i];
    return res;
}

static void CheckPoly(string name, ReadOnlySpan<Z> expected, ReadOnlySpan<Z> actual) {
    if (expected.Length != actual.Length) throw new Exception($"{name} length failed.");
    for (int i = 0; i < expected.Length; ++i) {
        if (expected[i] != actual[i]) throw new Exception($"{name} failed at {i}.");
    }
}

static void TestBufferedWriter() {
    CheckWriter("Append<T>()", w => {
        w.Append(0);
        w.Append(int.MinValue);
        w.Append(ulong.MaxValue);
        w.Append(BigInteger.Parse("-123456789012345678901234567890"));
    }, "0-214748364818446744073709551615-123456789012345678901234567890");
    CheckWriter("Append(string)", w => w.Append("萤火🔥"), "萤火🔥");
    CheckWriter("AppendLine()", w => w.AppendLine(), "\n");
    CheckWriter("AppendLine<T>()", w => {
        w.AppendLine(0);
        w.AppendLine(int.MinValue);
        w.AppendLine(ulong.MaxValue);
        w.AppendLine(BigInteger.Parse("-123456789012345678901234567890"));
    }, "0\n-2147483648\n18446744073709551615\n-123456789012345678901234567890\n");

    string s = new string('x', (1 << 16) + 1) + "萤火";
    CheckWriter("AppendLine(string)", w => w.AppendLine(s), s + "\n");
    CheckWriter("AppendYes()", w => w.AppendYes(), "Yes\n");
    CheckWriter("AppendNo()", w => w.AppendNo(), "No\n");
    CheckWriter("AppendYes(bool)", w => {
        w.AppendYes(true);
        w.AppendYes(false);
    }, "Yes\nNo\n");

    IEnumerable<int> values = new[] { 1, -2, 3 };
    CheckWriter("AppendJoin<T>(IEnumerable<T>)", w => w.AppendJoin(values), "1 -2 3\n");
    CheckWriter("AppendJoin<T>(char, IEnumerable<T>)", w => w.AppendJoin('界', values), "1界-2界3\n");
    CheckWriter("AppendJoin<T>(string, IEnumerable<T>)", w => w.AppendJoin(" | ", values), "1 | -2 | 3\n");
    CheckWriter("AppendJoin<T>(ReadOnlySpan<T>)", w => {
        ReadOnlySpan<short> span = new short[] { -4, 0, 5 };
        w.AppendJoin(span);
    }, "-4 0 5\n");
    CheckWriter("AppendJoin<T>(char, ReadOnlySpan<T>)", w => {
        ReadOnlySpan<ulong> span = new ulong[] { 6, ulong.MaxValue };
        w.AppendJoin(',', span);
    }, "6,18446744073709551615\n");
    CheckWriter("AppendJoin<T>(empty)", w => w.AppendJoin(Array.Empty<int>()), "\n");

#pragma warning disable CS0612, CS0618
    CheckWriter("AppendFormat", w => w.AppendFormat("{0}", 42), "");
#pragma warning restore CS0612, CS0618
    MethodInfo format = typeof(BufferedWriter).GetMethod(nameof(BufferedWriter.AppendFormat))!;
    if (format.GetCustomAttribute<ObsoleteAttribute>() is null) {
        throw new Exception("AppendFormat is not marked Obsolete.");
    }

    foreach (MethodInfo method in typeof(BufferedWriter).GetMethods()) {
        ParameterInfo[] parameters = method.GetParameters();
        if ((method.Name == nameof(BufferedWriter.Append) || method.Name == nameof(BufferedWriter.AppendLine)) &&
            !method.IsGenericMethod &&
            parameters.Length == 1 && parameters[0].ParameterType == typeof(object)) {
            throw new Exception($"{method.Name}(object) still exists.");
        }
        if (!method.IsGenericMethod ||
            method.Name != nameof(BufferedWriter.Append) &&
            method.Name != nameof(BufferedWriter.AppendLine) &&
            method.Name != nameof(BufferedWriter.AppendJoin)) continue;
        Type arg = method.GetGenericArguments()[0];
        bool found = false;
        foreach (Type constraint in arg.GetGenericParameterConstraints()) {
            if (constraint.IsGenericType && constraint.GetGenericTypeDefinition() == typeof(IBinaryInteger<>)) {
                found = true;
                break;
            }
        }
        if (!found) throw new Exception($"{method} is not restricted to integers.");
    }

    using MemoryStream stream = new();
    BufferedWriter writer = new(stream);
    writer.Append("first");
    if (stream.Length != 0) throw new Exception("BufferedWriter wrote before Output().");
    writer.Output();
    writer.Append("second");
    writer.Output();
    Equal("Output()", Encoding.UTF8.GetString(stream.ToArray()), "firstsecond");

    Console.WriteLine("BufferedWriter tests passed.");
}

static void CheckWriter(string name, Action<BufferedWriter> action, string expected) {
    using MemoryStream stream = new();
    BufferedWriter writer = new(stream);
    action(writer);
    if (stream.Length != 0) throw new Exception($"{name} wrote before Output().");
    writer.Output();
    Equal(name, Encoding.UTF8.GetString(stream.ToArray()), expected);
}

static void Equal(string name, string actual, string expected) {
    if (actual != expected) throw new Exception($"{name}: expected '{expected}', got '{actual}'.");
}

readonly struct TreapSumOp : ITreapOperator<int, long> {
    public long Identity => 0;
    public long Operate(long a, long b) => a + b;
    public long Create(int value, int count) => (long)value * count;
}

readonly struct TreapStringOp : ITreapOperator<char, string> {
    public string Identity => string.Empty;
    public string Operate(string a, string b) => a + b;
    public string Create(char value, int count) => new(value, count);
}
