using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireFly;

/// <summary>Provides binary-search and longest-increasing-subsequence algorithms.</summary>
public static class Utils {
    /// <summary>Finds the first position not less than a value in O(log n) time when indexing is O(1).</summary>
    /// <param name="a">The nonnull sequence sorted in nondecreasing order by a stable default total order.</param>
    /// <param name="x">The value whose lower bound is required.</param>
    /// <returns>The first index i such that a[i] is not less than x, or the sequence length if no such index exists.</returns>
    public static int LowerBound<T>(IList<T> a, T x) {
        int l = 0, r = a.Count;
        while (l < r) {
            int mid = l + (r - l) / 2;
            if (Comparer<T>.Default.Compare(a[mid], x) < 0) {
                l = mid + 1;
            } else {
                r = mid;
            }
        }
        return l;
    }

    /// <summary>Finds the first position greater than a value in O(log n) time when indexing is O(1).</summary>
    /// <param name="a">The nonnull sequence sorted in nondecreasing order by a stable default total order.</param>
    /// <param name="x">The value whose upper bound is required.</param>
    /// <returns>The first index i such that a[i] is greater than x, or the sequence length if no such index exists.</returns>
    public static int UpperBound<T>(IList<T> a, T x) {
        int l = 0, r = a.Count;
        while (l < r) {
            int mid = l + (r - l) / 2;
            if (Comparer<T>.Default.Compare(a[mid], x) <= 0) {
                l = mid + 1;
            } else {
                r = mid;
            }
        }
        return l;
    }

    /// <summary>Computes the length of a longest strictly increasing subsequence in O(n log n) time and O(n) space.</summary>
    /// <param name="a">The nonnull finite sequence to examine using a stable default total order.</param>
    /// <returns>The maximum length of a strictly increasing subsequence.</returns>
    public static int LongestStrictIncreasingSequence<T>(IEnumerable<T> a) {
        List<T> d = new();
        foreach (var x in a) {
            int j = LowerBound(d, x);
            if (j == d.Count) {
                d.Add(x);
            } else {
                d[j] = x;
            }
        }
        return d.Count;
    }

    /// <summary>Computes the length of a longest nondecreasing subsequence in O(n log n) time and O(n) space.</summary>
    /// <param name="a">The nonnull finite sequence to examine using a stable default total order.</param>
    /// <returns>The maximum length of a nondecreasing subsequence.</returns>
    public static int LongestIncreasingSequence<T>(IEnumerable<T> a) {
        List<T> d = new();
        foreach (var x in a) {
            int j = UpperBound(d, x);
            if (j == d.Count) {
                d.Add(x);
            } else {
                d[j] = x;
            }
        }
        return d.Count;
    }
}
