using FireFly.DS.Operators;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FireFly.DS;

/// <summary>Represents a Fenwick tree over a commutative monoid.</summary>
public class Fenwick<T, TOp>
    where TOp: struct, AtCoder.ISegtreeOperator<T> {
    /// <summary>Stores the number of elements and the current lazy-clear version.</summary>
    protected int n, time = 0;
    /// <summary>Stores the version in which each tree node was last used.</summary>
    protected int[] t;
    /// <summary>Stores the one-based Fenwick tree nodes.</summary>
    protected T[] C;
    private TOp op = default;
    private static int lb(int x) => x & -x;
    /// <summary>Gets the number of elements.</summary>
    public int Count => n;
    /// <summary>Creates a Fenwick tree whose elements are the operator identity.</summary>
    /// <param name="n">The number of elements; it must be nonnegative.</param>
    public Fenwick(int n) {
        this.n = n;
        C = new T[n + 1];
        t = new int[n + 1];
        Array.Fill(C, op.Identity);
    }
    /// <summary>Builds a Fenwick tree from the given elements in linear time; the default value of T must be the operator identity.</summary>
    /// <param name="arr">The elements in zero-based order; it must not be null.</param>
    public Fenwick(T[] arr) { // note that the index of arr starts at 0, while C starts at 1
        n = arr.Length;
        C = new T[n + 1];
        for (int i = 1; i <= n; ++i) {
            C[i] = op.Operate(C[i], arr[i - 1]);
            int j = i + lb(i);
            if (j <= n) C[j] = op.Operate(C[j], C[i]);
        }
        t = new int[n + 1];
    }
    /// <summary>Gets a one-based tree node, treating a node cleared in an earlier version as the identity.</summary>
    /// <param name="i">The one-based tree-node index; it must be between 1 and <see cref="Count"/>.</param>
    /// <returns>The aggregate stored in the tree node.</returns>
    protected T this[int i] {
        get {
            if (t[i] < time) {
                t[i] = time;
                C[i] = op.Identity;
            }
            return C[i];
        }
    }
    /// <summary>Combines a value into one element in O(log n) time.</summary>
    /// <param name="i">The one-based element index; it must be between 1 and <see cref="Count"/>.</param>
    /// <param name="value">The value to combine into the element.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(int i, T value) { // note that this i starts at 1
        for (; i <= n; i += lb(i)) C[i] = op.Operate(this[i], value);
    }
    /// <summary>Computes a prefix aggregate in O(log n) time.</summary>
    /// <param name="i">The number of initial elements to include; values outside the valid range are clamped.</param>
    /// <returns>The aggregate of the first <paramref name="i"/> elements after clamping.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Query(int i) {
        i = Math.Min(i, n);
        i = Math.Max(i, 0);
        T res = op.Identity;
        for (; i > 0; i -= lb(i)) res = op.Operate(res, this[i]);
        return res;
    }

    /// <summary>Logically resets all elements to the operator identity in O(1) time.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() {
        ++time;
    }
}

/// <summary>Represents an additive Fenwick tree.</summary>
public class Fenwick<T> : Fenwick<T, AddOp<T>> where T: INumber<T> {
    /// <summary>Creates an additive Fenwick tree whose elements are zero.</summary>
    /// <param name="n">The number of elements; it must be nonnegative.</param>
    public Fenwick(int n) : base(n) { }
    /// <summary>Builds an additive Fenwick tree from the given elements in linear time.</summary>
    /// <param name="arr">The elements in zero-based order; it must not be null.</param>
    public Fenwick(T[] arr) : base(arr) { }
    /// <summary>Finds the first one-based position whose prefix sum is at least the given value in O(log n) time.</summary>
    /// <param name="k">The positive target sum; it must not exceed the total sum, and prefix sums must be nondecreasing.</param>
    /// <returns>The smallest one-based position whose prefix sum is at least <paramref name="k"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Kth(T k) {
        int x = 0;
        T sum = T.Zero;
        for (int i = BitOperations.Log2((uint)n); i >= 0; --i) {
            x += 1 << i;
            if (x >= n || sum + this[x] >= k) {
                x -= 1 << i;
            } else {
                sum += this[x];
            }
        }
        return x + 1;
    }
}
