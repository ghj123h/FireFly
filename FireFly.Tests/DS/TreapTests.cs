using System;
using System.Collections.Generic;
using FireFly.DS;
using Xunit;

namespace FireFly.Tests.DS;

public class TreapTests {
    [Fact]
    public void Basics() {
        Treap<int> tree = new(16);
        List<int> values = new();
        CheckTreap(tree, values);

        int[] input = [0, 5, 5, -2, int.MinValue, int.MaxValue, 3, 0];
        foreach (int value in input) {
            tree.Add(value);
            values.Add(value);
        }
        values.Sort();
        CheckTreap(tree, values);

        Assert.True(tree.Remove(5));
        values.Remove(5);
        Assert.False(tree.Remove(4));
        Assert.True(tree.Remove(int.MinValue));
        values.Remove(int.MinValue);
        CheckTreap(tree, values);

        tree.Clear();
        values.Clear();
        CheckTreap(tree, values);
        tree.Add(0);
        values.Add(0);
        CheckTreap(tree, values);

        Assert.False(typeof(Treap<int>).GetProperty(nameof(Treap<int>.Comparer))!.CanWrite);
    }

    [Fact]
    public void CustomComparer() {
        Treap<string> words = new(4, StringComparer.OrdinalIgnoreCase);
        words.Add("A");
        words.Add("a");
        words.Add("b");

        Assert.Equal(3, words.Count);
        Assert.Equal(2, words.CountOf("a"));
        Assert.Equal("A", words[0]);
        Assert.Equal("A", words[1]);
        Assert.True(words.TryPrev("b", out string prev));
        Assert.Equal("A", prev);
        Assert.True(words.TryNext("a", out string next));
        Assert.Equal("b", next);
        Assert.True(words.Remove("a"));
        Assert.Equal(1, words.CountOf("A"));
        Assert.Equal("A", words[0]);

        IComparer<int> abs = Comparer<int>.Create((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));
        Treap<int, long, TreapSumOp> sum = new(abs);
        sum.Add(-2);
        sum.Add(2);
        Assert.Equal(2, sum.Count);
        Assert.Equal(-2, sum[0]);
        Assert.Equal(-2, sum[1]);
        Assert.Equal(-4, sum.AllProd);
    }

    [Fact]
    public void Aggregates() {
        Treap<int, long, TreapSumOp> tree = new(16);
        List<int> values = new();
        int[] input = [5, 1, 5, -3, 0, 8, 1];
        foreach (int value in input) {
            tree.Add(value);
            values.Add(value);
        }
        values.Sort();
        CheckSumTreap(tree, values);

        tree.Remove(5);
        values.Remove(5);
        tree.Remove(1);
        values.Remove(1);
        CheckSumTreap(tree, values);

        tree.Clear();
        values.Clear();
        CheckSumTreap(tree, values);

        Treap<char, string, TreapStringOp> text = new();
        foreach (char c in new[] { 'c', 'a', 'b', 'b' }) text.Add(c);
        Assert.Equal("abbc", text.AllProd);
        Assert.Equal("bb", text.Prod(1, 3));
        Assert.Equal(string.Empty, text.Prod(2, 2));
    }

    [Fact]
    public void RandomOperationsMatchSortedList() {
        const int iterations = 10_000;
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
                Assert.Equal(expected, tree.Remove(value));
                Assert.Equal(expected, sum.Remove(value));
                if (expected) values.RemoveAt(p);
            }

            if (step % 23 == 0) {
                CheckTreap(tree, values);
                CheckRandomProd(sum, values, rnd);
            }
        }
        CheckTreap(tree, values);
        CheckRandomProd(sum, values, rnd);
    }

    private static void CheckTreap(Treap<int> tree, List<int> expected) {
        Assert.Equal(expected.Count, tree.Count);

        int i = 0;
        foreach (int value in tree) {
            Assert.True(i < expected.Count);
            Assert.Equal(expected[i], value);
            ++i;
        }
        Assert.Equal(expected.Count, i);

        for (i = 0; i < expected.Count; ++i) Assert.Equal(expected[i], tree[i]);

        const int marker = 123456789;
        int[] copy = new int[expected.Count + 2];
        Array.Fill(copy, marker);
        tree.CopyTo(copy, 1);
        Assert.Equal(marker, copy[0]);
        Assert.Equal(marker, copy[^1]);
        for (i = 0; i < expected.Count; ++i) Assert.Equal(expected[i], copy[i + 1]);

        int[] probes = [int.MinValue, -101, -5, -1, 0, 1, 5, 101, int.MaxValue];
        foreach (int value in probes) {
            int l = NaiveLowerBound(expected, value);
            int r = NaiveUpperBound(expected, value);
            Assert.Equal(l, tree.LowerBound(value));
            Assert.Equal(r, tree.UpperBound(value));
            Assert.Equal(r - l, tree.CountOf(value));
            Assert.Equal(l < r, tree.Contains(value));

            bool hasPrev = l > 0;
            bool foundPrev = tree.TryPrev(value, out int prev);
            int expectedPrev = hasPrev ? expected[l - 1] : default;
            Assert.Equal(hasPrev, foundPrev);
            Assert.Equal(expectedPrev, prev);

            bool hasNext = r < expected.Count;
            bool foundNext = tree.TryNext(value, out int next);
            int expectedNext = hasNext ? expected[r] : default;
            Assert.Equal(hasNext, foundNext);
            Assert.Equal(expectedNext, next);
        }
    }

    private static void CheckSumTreap(Treap<int, long, TreapSumOp> tree, List<int> expected) {
        Assert.Equal(expected.Count, tree.Count);
        long all = 0;
        for (int i = 0; i < expected.Count; ++i) {
            all += expected[i];
            Assert.Equal(expected[i], tree[i]);
        }
        Assert.Equal(all, tree.AllProd);

        for (int l = 0; l <= expected.Count; ++l) {
            long prod = 0;
            for (int r = l; r <= expected.Count; ++r) {
                Assert.Equal(prod, tree.Prod(l, r));
                if (r < expected.Count) prod += expected[r];
            }
        }

        for (int k = 0; k <= expected.Count; ++k) {
            long top = 0;
            for (int i = expected.Count - k; i < expected.Count; ++i) top += expected[i];
            Assert.Equal(top, tree.Prod(expected.Count - k, expected.Count));
        }
    }

    private static void CheckRandomProd(
        Treap<int, long, TreapSumOp> tree,
        List<int> expected,
        Random rnd) {
        Assert.Equal(expected.Count, tree.Count);
        long all = 0;
        foreach (int value in expected) all += value;
        Assert.Equal(all, tree.AllProd);

        int i = 0;
        foreach (int value in tree) {
            Assert.True(i < expected.Count);
            Assert.Equal(expected[i], value);
            ++i;
        }
        Assert.Equal(expected.Count, i);

        for (int q = 0; q < 16; ++q) {
            int l = rnd.Next(expected.Count + 1);
            int r = rnd.Next(expected.Count + 1);
            if (l > r) (l, r) = (r, l);
            long prod = 0;
            for (i = l; i < r; ++i) prod += expected[i];
            Assert.Equal(prod, tree.Prod(l, r));
        }
    }

    private static int NaiveLowerBound(List<int> values, int value) {
        int l = 0, r = values.Count;
        while (l < r) {
            int m = (l + r) >> 1;
            if (values[m] < value) l = m + 1;
            else r = m;
        }
        return l;
    }

    private static int NaiveUpperBound(List<int> values, int value) {
        int l = 0, r = values.Count;
        while (l < r) {
            int m = (l + r) >> 1;
            if (values[m] <= value) l = m + 1;
            else r = m;
        }
        return l;
    }

    private readonly struct TreapSumOp : ITreapOperator<int, long> {
        public long Identity => 0;
        public long Operate(long a, long b) => a + b;
        public long Create(int value, int count) => (long)value * count;
    }

    private readonly struct TreapStringOp : ITreapOperator<char, string> {
        public string Identity => string.Empty;
        public string Operate(string a, string b) => a + b;
        public string Create(char value, int count) => new(value, count);
    }
}
