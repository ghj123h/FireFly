
namespace FireFly;

/// <summary>Stores one unspecified representative from each comparer-equivalence class in sorted order for coordinate compression.</summary>
public class Discrete<T> {
    private readonly List<T> list;
    private readonly IComparer<T> comparer;

    /// <summary>Creates a coordinate compression using the default comparer in O(n log n) time.</summary>
    /// <param name="elements">The nonnull, nonempty sequence of values to compress using a stable default total order.</param>
    public Discrete(IEnumerable<T> elements) : this(elements, null) { }

    /// <summary>Creates a coordinate compression using the specified comparer in O(n log n) time.</summary>
    /// <param name="elements">The nonnull, nonempty sequence of values to compress.</param>
    /// <param name="comparer">The stable total-order comparer, or null to use the default comparer.</param>
    public Discrete(IEnumerable<T> elements, IComparer<T>? comparer) {
        this.list = new List<T>(elements);
        this.comparer = comparer ?? Comparer<T>.Default;
        if (list.Count == 0) {
            throw new ArgumentException("You cannot discretize zero elements!");
        }
        list.Sort(this.comparer);
        int i, j;
        for (i = 0, j = 1; j < list.Count; ++j) {
            if (this.comparer.Compare(list[i], list[j]) != 0) {
                list[++i] = list[j];
            }
        }
        ++i;
        while (i < list.Count) {
            list.RemoveAt(list.Count - 1);
        }
    }

    /// <summary>Gets the number of distinct values.</summary>
    public int Count { get => list.Count; }

    /// <summary>Gets the value at a compressed index.</summary>
    /// <param name="index">The zero-based index in the range [0, Count).</param>
    /// <returns>The value at the specified index.</returns>
    public T Get(int index) { return list[index]; }

    /// <summary>Finds the lower-bound compressed index of a value in O(log n) time.</summary>
    /// <param name="index">The value whose insertion position is required.</param>
    /// <returns>The first index whose stored value is not less than the specified value, or Count if no such index exists.</returns>
    public int this[T index] {
        get {
            int j = list.BinarySearch(index, comparer);
            if (j < 0) {
                j = ~j;
            }
            return j;
        }
    }
}
