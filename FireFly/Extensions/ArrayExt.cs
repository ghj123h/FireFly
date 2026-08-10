namespace FireFly.Extensions;

/// <summary>Provides multidimensional array extensions.</summary>
public static class ArrayExt {
    /// <summary>Creates an O(1) row-major span that aliases every element of a two-dimensional array.</summary>
    /// <param name="array">The two-dimensional array to wrap.</param>
    /// <returns>A span over all elements of the array.</returns>
    public static Span<T> AsSpan<T>(this T[,] array) => asSpan<T>(array);

    /// <summary>Creates an O(1) row-major span that aliases every element of a three-dimensional array.</summary>
    /// <param name="array">The three-dimensional array to wrap.</param>
    /// <returns>A span over all elements of the array.</returns>
    public static Span<T> AsSpan<T>(this T[,,] array) => asSpan<T>(array);
    static Span<T> asSpan<T>(Array array)
        => System.Runtime.InteropServices.MemoryMarshal.CreateSpan(
            ref System.Runtime.CompilerServices.Unsafe.As<byte, T>(
                ref System.Runtime.InteropServices.MemoryMarshal.GetArrayDataReference(array)
            ), array.Length);
}
