using AtCoder;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FireFly.DS;

/// <summary>Defines how ordered multiset values are converted to and combined as aggregates.</summary>
public interface ITreapOperator<T, TAggregate> : ISegtreeOperator<TAggregate> {
    /// <summary>Creates the aggregate of consecutive equal values in O(1) time.</summary>
    /// <param name="value">The stored representative of the repeated value.</param>
    /// <param name="count">The positive number of copies.</param>
    /// <returns>The aggregate of <paramref name="count"/> copies of <paramref name="value"/>.</returns>
    TAggregate Create(T value, int count);
}

internal readonly struct TreapNone { }

internal readonly struct TreapNoneOp<T> : ITreapOperator<T, TreapNone> {
    public TreapNone Identity => default;
    public TreapNone Operate(TreapNone a, TreapNone b) => default;
    public TreapNone Create(T value, int count) => default;
}

internal struct TreapInfo<T, TAggregate> {
    internal int Left;
    internal int Right;
    internal int Size;
    internal int Repeat;
    internal int Priority;
    internal T Value;
    internal TAggregate Aggregate;
}

internal sealed class TreapCore<T, TAggregate, TOp>
    where TOp : struct, ITreapOperator<T, TAggregate> {
    private readonly List<TreapInfo<T, TAggregate>> nodes;
    private readonly IComparer<T> comparer;
    private readonly Random rnd = new();
    private TOp op = default;
    private int root;

    internal int Count => nodes[root].Size;
    internal IComparer<T> Comparer => comparer;
    internal TAggregate AllProd => nodes[root].Aggregate;

    internal TreapCore(int capacity, IComparer<T>? comparer) {
        nodes = new(capacity + 1);
        this.comparer = comparer ?? System.Collections.Generic.Comparer<T>.Default;
        nodes.Add(new TreapInfo<T, TAggregate> { Aggregate = op.Identity });
    }

    private int NewNode(T value) {
        int u = nodes.Count;
        nodes.Add(new TreapInfo<T, TAggregate> {
            Size = 1,
            Repeat = 1,
            Priority = rnd.Next(),
            Value = value,
            Aggregate = op.Create(value, 1)
        });
        return u;
    }

    private void Update(int u) {
        TreapInfo<T, TAggregate> node = nodes[u];
        node.Size = nodes[node.Left].Size + node.Repeat + nodes[node.Right].Size;
        TAggregate value = op.Create(node.Value, node.Repeat);
        node.Aggregate = op.Operate(op.Operate(nodes[node.Left].Aggregate, value), nodes[node.Right].Aggregate);
        nodes[u] = node;
    }

    private int RotateLeft(int u) {
        TreapInfo<T, TAggregate> node = nodes[u];
        int v = node.Right;
        TreapInfo<T, TAggregate> next = nodes[v];
        node.Right = next.Left;
        next.Left = u;
        nodes[u] = node;
        nodes[v] = next;
        Update(u);
        Update(v);
        return v;
    }

    private int RotateRight(int u) {
        TreapInfo<T, TAggregate> node = nodes[u];
        int v = node.Left;
        TreapInfo<T, TAggregate> next = nodes[v];
        node.Left = next.Right;
        next.Right = u;
        nodes[u] = node;
        nodes[v] = next;
        Update(u);
        Update(v);
        return v;
    }

    private int Add(int u, T value) {
        if (u == 0) return NewNode(value);

        TreapInfo<T, TAggregate> node = nodes[u];
        int cmp = comparer.Compare(node.Value, value);
        if (cmp == 0) {
            ++node.Repeat;
            nodes[u] = node;
            Update(u);
            return u;
        }
        if (cmp > 0) {
            node.Left = Add(node.Left, value);
            nodes[u] = node;
            if (nodes[node.Left].Priority < node.Priority) return RotateRight(u);
        } else {
            node.Right = Add(node.Right, value);
            nodes[u] = node;
            if (nodes[node.Right].Priority < node.Priority) return RotateLeft(u);
        }
        Update(u);
        return u;
    }

    internal void Add(T value) {
        root = Add(root, value);
    }

    private (int Root, bool Removed) Remove(int u, T value) {
        if (u == 0) return (0, false);

        TreapInfo<T, TAggregate> node = nodes[u];
        int cmp = comparer.Compare(node.Value, value);
        if (cmp > 0) {
            (int child, bool removed) = Remove(node.Left, value);
            if (!removed) return (u, false);
            node.Left = child;
            nodes[u] = node;
            Update(u);
            return (u, true);
        }
        if (cmp < 0) {
            (int child, bool removed) = Remove(node.Right, value);
            if (!removed) return (u, false);
            node.Right = child;
            nodes[u] = node;
            Update(u);
            return (u, true);
        }
        if (node.Repeat > 1) {
            --node.Repeat;
            nodes[u] = node;
            Update(u);
            return (u, true);
        }
        if (node.Left == 0) return (node.Right, true);
        if (node.Right == 0) return (node.Left, true);

        if (nodes[node.Left].Priority < nodes[node.Right].Priority) {
            u = RotateRight(u);
            node = nodes[u];
            (node.Right, _) = Remove(node.Right, value);
        } else {
            u = RotateLeft(u);
            node = nodes[u];
            (node.Left, _) = Remove(node.Left, value);
        }
        nodes[u] = node;
        Update(u);
        return (u, true);
    }

    internal bool Remove(T value) {
        bool removed;
        (root, removed) = Remove(root, value);
        return removed;
    }

    internal int LowerBound(T value) {
        int u = root, res = 0;
        while (u != 0) {
            TreapInfo<T, TAggregate> node = nodes[u];
            if (comparer.Compare(node.Value, value) >= 0) {
                u = node.Left;
            } else {
                res += nodes[node.Left].Size + node.Repeat;
                u = node.Right;
            }
        }
        return res;
    }

    internal int UpperBound(T value) {
        int u = root, res = 0;
        while (u != 0) {
            TreapInfo<T, TAggregate> node = nodes[u];
            if (comparer.Compare(node.Value, value) > 0) {
                u = node.Left;
            } else {
                res += nodes[node.Left].Size + node.Repeat;
                u = node.Right;
            }
        }
        return res;
    }

    internal T GetAt(int index) {
        int u = root;
        while (u != 0) {
            TreapInfo<T, TAggregate> node = nodes[u];
            int leftSize = nodes[node.Left].Size;
            if (index < leftSize) {
                u = node.Left;
            } else if (index < leftSize + node.Repeat) {
                return node.Value;
            } else {
                index -= leftSize + node.Repeat;
                u = node.Right;
            }
        }
        return default!;
    }

    internal bool TryPrev(T value, out T result) {
        int u = root;
        bool found = false;
        result = default!;
        while (u != 0) {
            TreapInfo<T, TAggregate> node = nodes[u];
            if (comparer.Compare(node.Value, value) < 0) {
                result = node.Value;
                found = true;
                u = node.Right;
            } else {
                u = node.Left;
            }
        }
        return found;
    }

    internal bool TryNext(T value, out T result) {
        int u = root;
        bool found = false;
        result = default!;
        while (u != 0) {
            TreapInfo<T, TAggregate> node = nodes[u];
            if (comparer.Compare(node.Value, value) > 0) {
                result = node.Value;
                found = true;
                u = node.Left;
            } else {
                u = node.Right;
            }
        }
        return found;
    }

    internal int CountOf(T value) {
        int u = root;
        while (u != 0) {
            TreapInfo<T, TAggregate> node = nodes[u];
            int cmp = comparer.Compare(node.Value, value);
            if (cmp == 0) return node.Repeat;
            u = cmp > 0 ? node.Left : node.Right;
        }
        return 0;
    }

    private TAggregate Prod(int u, int l, int r) {
        if (l == r) return op.Identity;
        if (l == 0 && r == nodes[u].Size) return nodes[u].Aggregate;

        TreapInfo<T, TAggregate> node = nodes[u];
        int leftSize = nodes[node.Left].Size;
        int middleEnd = leftSize + node.Repeat;
        TAggregate res = op.Identity;
        if (l < leftSize) {
            res = op.Operate(res, Prod(node.Left, l, Math.Min(r, leftSize)));
        }
        int from = Math.Max(l, leftSize);
        int to = Math.Min(r, middleEnd);
        if (from < to) {
            res = op.Operate(res, op.Create(node.Value, to - from));
        }
        if (r > middleEnd) {
            res = op.Operate(res, Prod(node.Right, Math.Max(0, l - middleEnd), r - middleEnd));
        }
        return res;
    }

    internal TAggregate Prod(int l, int r) => Prod(root, l, r);

    private IEnumerable<T> InOrder(int u) {
        if (u == 0) yield break;
        TreapInfo<T, TAggregate> node = nodes[u];
        foreach (T value in InOrder(node.Left)) yield return value;
        for (int i = 0; i < node.Repeat; ++i) yield return node.Value;
        foreach (T value in InOrder(node.Right)) yield return value;
    }

    internal IEnumerator<T> GetEnumerator() => InOrder(root).GetEnumerator();

    internal void Clear() {
        nodes.Clear();
        nodes.Add(new TreapInfo<T, TAggregate> { Aggregate = op.Identity });
        root = 0;
    }

    internal void CopyTo(T[] array, int arrayIndex) {
        foreach (T value in InOrder(root)) array[arrayIndex++] = value;
    }
}

/// <summary>Represents an ordered multiset implemented by a randomized treap. Comparer-equivalent insertions repeat the first stored representative. Removed nodes are not reused until <see cref="Clear"/> resets the node pool. Operations by key or rank take expected O(log d) time and O(d) worst-case time, where d is the number of comparer-equivalence classes.</summary>
public class Treap<T> : ICollection<T> {
    private readonly TreapCore<T, TreapNone, TreapNoneOp<T>> core;

    /// <summary>Gets the comparer that defines value order and equivalence.</summary>
    public IComparer<T> Comparer => core.Comparer;
    /// <summary>Gets the number of values, including repetitions.</summary>
    public int Count => core.Count;
    /// <summary>Gets whether the multiset is read-only.</summary>
    public bool IsReadOnly => false;

    /// <summary>Creates an empty ordered multiset.</summary>
    /// <param name="comparer">The comparer, or <c>null</c> to use the default comparer.</param>
    public Treap(IComparer<T>? comparer = null) : this(0, comparer) { }

    /// <summary>Creates an empty ordered multiset with capacity for distinct values.</summary>
    /// <param name="capacity">The nonnegative expected number of distinct values; it must be less than <see cref="int.MaxValue"/>.</param>
    /// <param name="comparer">The comparer, or <c>null</c> to use the default comparer.</param>
    public Treap(int capacity, IComparer<T>? comparer = null) {
        core = new(capacity, comparer);
    }

    /// <summary>Adds one copy of a value in expected O(log d) time.</summary>
    /// <param name="item">The value to add.</param>
    public void Add(T item) => core.Add(item);

    /// <summary>Removes one copy of a value in expected O(log d) time.</summary>
    /// <param name="item">The value to remove.</param>
    /// <returns><c>true</c> if a copy was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(T item) => core.Remove(item);

    /// <summary>Counts values strictly less than the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose lower bound is queried.</param>
    /// <returns>The number of preceding values, including repetitions.</returns>
    public int LowerBound(T value) => core.LowerBound(value);

    /// <summary>Counts values less than or equivalent to the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose upper bound is queried.</param>
    /// <returns>The number of values not ordered after <paramref name="value"/>, including repetitions.</returns>
    public int UpperBound(T value) => core.UpperBound(value);

    /// <summary>Gets a value by its zero-based position in comparer order in expected O(log d) time.</summary>
    /// <param name="index">The zero-based position; it must be between zero inclusive and <see cref="Count"/> exclusive.</param>
    /// <returns>The value at <paramref name="index"/>.</returns>
    public T this[int index] => core.GetAt(index);

    /// <summary>Finds the greatest value strictly preceding the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose predecessor is queried.</param>
    /// <param name="result">The predecessor when one exists; otherwise, the default value.</param>
    /// <returns><c>true</c> if a predecessor exists; otherwise, <c>false</c>.</returns>
    public bool TryPrev(T value, out T result) => core.TryPrev(value, out result);

    /// <summary>Finds the least value strictly following the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose successor is queried.</param>
    /// <param name="result">The successor when one exists; otherwise, the default value.</param>
    /// <returns><c>true</c> if a successor exists; otherwise, <c>false</c>.</returns>
    public bool TryNext(T value, out T result) => core.TryNext(value, out result);

    /// <summary>Returns an enumerator over all values in comparer order in O(n) time.</summary>
    /// <returns>An enumerator over all values, including repetitions.</returns>
    public IEnumerator<T> GetEnumerator() => core.GetEnumerator();

    /// <summary>Removes all values while retaining allocated capacity.</summary>
    public void Clear() => core.Clear();

    /// <summary>Determines whether an equivalent value exists in expected O(log d) time.</summary>
    /// <param name="item">The value to find.</param>
    /// <returns><c>true</c> if an equivalent value exists; otherwise, <c>false</c>.</returns>
    public bool Contains(T item) => core.CountOf(item) > 0;

    /// <summary>Counts copies equivalent to the given value in expected O(log d) time.</summary>
    /// <param name="value">The value to count.</param>
    /// <returns>The number of equivalent copies.</returns>
    public int CountOf(T value) => core.CountOf(value);

    /// <summary>Copies all values in comparer order to an array in O(n) time.</summary>
    /// <param name="array">The destination array with enough remaining space.</param>
    /// <param name="arrayIndex">The zero-based destination index.</param>
    public void CopyTo(T[] array, int arrayIndex) => core.CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>Represents an ordered multiset with monoid aggregates implemented by a randomized treap. Comparer-equivalent insertions repeat the first stored representative. Removed nodes are not reused until <see cref="Clear"/> resets the node pool. The default value of <c>TOp</c> must implement the aggregate operator. Operations by key, rank, or range take expected O(log d) time and O(d) worst-case time, where d is the number of comparer-equivalence classes.</summary>
public class Treap<T, TAggregate, TOp> : ICollection<T>
    where TOp : struct, ITreapOperator<T, TAggregate> {
    private readonly TreapCore<T, TAggregate, TOp> core;

    /// <summary>Gets the comparer that defines value order and equivalence.</summary>
    public IComparer<T> Comparer => core.Comparer;
    /// <summary>Gets the number of values, including repetitions.</summary>
    public int Count => core.Count;
    /// <summary>Gets whether the multiset is read-only.</summary>
    public bool IsReadOnly => false;
    /// <summary>Gets the aggregate of all values in comparer order, or the operator identity when empty.</summary>
    public TAggregate AllProd => core.AllProd;

    /// <summary>Creates an empty ordered multiset with monoid aggregates.</summary>
    /// <param name="comparer">The comparer, or <c>null</c> to use the default comparer.</param>
    public Treap(IComparer<T>? comparer = null) : this(0, comparer) { }

    /// <summary>Creates an empty ordered multiset with monoid aggregates and capacity for distinct values.</summary>
    /// <param name="capacity">The nonnegative expected number of distinct values; it must be less than <see cref="int.MaxValue"/>.</param>
    /// <param name="comparer">The comparer, or <c>null</c> to use the default comparer.</param>
    public Treap(int capacity, IComparer<T>? comparer = null) {
        core = new(capacity, comparer);
    }

    /// <summary>Adds one copy of a value in expected O(log d) time.</summary>
    /// <param name="item">The value to add.</param>
    public void Add(T item) => core.Add(item);

    /// <summary>Removes one copy of a value in expected O(log d) time.</summary>
    /// <param name="item">The value to remove.</param>
    /// <returns><c>true</c> if a copy was removed; otherwise, <c>false</c>.</returns>
    public bool Remove(T item) => core.Remove(item);

    /// <summary>Counts values strictly less than the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose lower bound is queried.</param>
    /// <returns>The number of preceding values, including repetitions.</returns>
    public int LowerBound(T value) => core.LowerBound(value);

    /// <summary>Counts values less than or equivalent to the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose upper bound is queried.</param>
    /// <returns>The number of values not ordered after <paramref name="value"/>, including repetitions.</returns>
    public int UpperBound(T value) => core.UpperBound(value);

    /// <summary>Gets a value by its zero-based position in comparer order in expected O(log d) time.</summary>
    /// <param name="index">The zero-based position; it must be between zero inclusive and <see cref="Count"/> exclusive.</param>
    /// <returns>The value at <paramref name="index"/>.</returns>
    public T this[int index] => core.GetAt(index);

    /// <summary>Finds the greatest value strictly preceding the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose predecessor is queried.</param>
    /// <param name="result">The predecessor when one exists; otherwise, the default value.</param>
    /// <returns><c>true</c> if a predecessor exists; otherwise, <c>false</c>.</returns>
    public bool TryPrev(T value, out T result) => core.TryPrev(value, out result);

    /// <summary>Finds the least value strictly following the given value in expected O(log d) time.</summary>
    /// <param name="value">The value whose successor is queried.</param>
    /// <param name="result">The successor when one exists; otherwise, the default value.</param>
    /// <returns><c>true</c> if a successor exists; otherwise, <c>false</c>.</returns>
    public bool TryNext(T value, out T result) => core.TryNext(value, out result);

    /// <summary>Computes the aggregate over a zero-based half-open range in comparer order in expected O(log d) time.</summary>
    /// <param name="l">The inclusive start position; it must be between zero and <paramref name="r"/>.</param>
    /// <param name="r">The exclusive end position; it must not exceed <see cref="Count"/>.</param>
    /// <returns>The aggregate over <c>[l, r)</c>, or the operator identity when the range is empty.</returns>
    public TAggregate Prod(int l, int r) => core.Prod(l, r);

    /// <summary>Returns an enumerator over all values in comparer order in O(n) time.</summary>
    /// <returns>An enumerator over all values, including repetitions.</returns>
    public IEnumerator<T> GetEnumerator() => core.GetEnumerator();

    /// <summary>Removes all values while retaining allocated capacity.</summary>
    public void Clear() => core.Clear();

    /// <summary>Determines whether an equivalent value exists in expected O(log d) time.</summary>
    /// <param name="item">The value to find.</param>
    /// <returns><c>true</c> if an equivalent value exists; otherwise, <c>false</c>.</returns>
    public bool Contains(T item) => core.CountOf(item) > 0;

    /// <summary>Counts copies equivalent to the given value in expected O(log d) time.</summary>
    /// <param name="value">The value to count.</param>
    /// <returns>The number of equivalent copies.</returns>
    public int CountOf(T value) => core.CountOf(value);

    /// <summary>Copies all values in comparer order to an array in O(n) time.</summary>
    /// <param name="array">The destination array with enough remaining space.</param>
    /// <param name="arrayIndex">The zero-based destination index.</param>
    public void CopyTo(T[] array, int arrayIndex) => core.CopyTo(array, arrayIndex);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
