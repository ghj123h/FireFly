using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace FireFly.IO;

/// <summary>Reads ASCII-compatible tokens and lines from a buffered byte stream without taking ownership of it.</summary>
public class BufferedReader {
    private readonly Stream sr;
    private readonly byte[] buffer;
    private int S = 0, T = 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void OmitWhiteSpace() {
        while (!EndOfStream && char.IsWhiteSpace((char)Peek())) Read();
    }

    /// <summary>Gets the next byte without advancing the reader.</summary>
    /// <returns>The next byte as a nonnegative integer, or -1 at the end of the stream.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Peek() {
        if (S == T) {
            T = sr.Read(buffer);
            if (T == 0) return -1;
            S = 0;
        }
        return buffer[S];
    }

    /// <summary>Reads and advances past the next byte.</summary>
    /// <returns>The next byte as a nonnegative integer, or -1 at the end of the stream.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Read() {
        int p = Peek();
        if (p >= 0) ++S;
        return p;
    }

    /// <summary>Reads the next line containing a non-CR byte in O(consumed bytes) time, ignoring CR bytes and empty LF-terminated lines.</summary>
    /// <returns>The line without line-ending bytes, or an empty string when no characters remain.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadLine() {
        int c;
        StringBuilder sb = new();
        while ((c = Read()) != -1) {
            if (c == '\r') continue; // omit '\r'
            if (c == '\n') {
                if (sb.Length > 0) break;
                else continue; // omit empty lines
            }
            sb.Append((char)c);
        }
        return sb.ToString();
    }

    /// <summary>Reads the next whitespace-delimited ASCII-compatible token in O(consumed bytes) time.</summary>
    /// <returns>The token, or an empty string at the end of the stream.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string ReadToken() {
        OmitWhiteSpace();
        StringBuilder sb = new();
        int c;
        while ((c = Peek()) != -1) {
            if (char.IsWhiteSpace((char)c)) break;
            sb.Append((char)c);
            Read();
        }
        return sb.ToString();
    }

    /// <summary>Creates a reader over a readable stream with the specified byte-buffer capacity.</summary>
    /// <param name="stream">The nonnull readable stream, which remains owned by the caller.</param>
    /// <param name="capacity">The positive number of bytes held in the input buffer.</param>
    public BufferedReader(Stream stream, int capacity) {
        if (capacity <= 0) {
            throw new ArgumentException("Capacity must be positive.");
        }
        sr = stream;
        buffer = new byte[capacity];
    }

    /// <summary>Gets whether no more bytes are available, refilling the buffer when necessary.</summary>
    public bool EndOfStream { get => Peek() == -1; }

    /// <summary>Reads a valid whitespace-delimited decimal integer with an optional leading sign and at least one following ASCII digit in O(consumed bytes) time.</summary>
    /// <returns>The parsed integer, which must be representable by the requested numeric type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T ReadInt<T>() where T : INumberBase<T> {
        char c;
        T res = T.Zero;
        bool neg = false;
        OmitWhiteSpace();
        if (!EndOfStream) {
            int ch = Peek();
            if (ch == '+') {
                Read();
            } else if (ch == '-') {
                neg = true;
                Read();
            }
        }
        while (!EndOfStream && char.IsDigit((char)Peek())) {
            c = (char)Read();
            res = res * T.CreateChecked(10) + T.CreateChecked(c - '0');
        }
        return neg ? -res : res;
    }

    /// <summary>Reads consecutive decimal integers into a zero-based array in O(count plus consumed bytes) time.</summary>
    /// <param name="count">The nonnegative number of integers to read, representable as an array length.</param>
    /// <returns>An array containing the requested integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ReadArray<T>(int count) where T : INumberBase<T> => ReadArray<T>(count, 0);

    /// <summary>Reads consecutive decimal integers into an array after a default-initialized prefix in O(count plus startIndex plus consumed bytes) time.</summary>
    /// <param name="count">The nonnegative number of integers to read.</param>
    /// <param name="startIndex">The nonnegative prefix length such that count plus startIndex is representable as an array length.</param>
    /// <returns>An array of length count plus startIndex containing the requested integers at the specified offset.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T[] ReadArray<T>(int count, int startIndex) where T : INumberBase<T> {
        T[] arr = new T[count + startIndex];
        for (int i = 0; i < count; ++i) arr[i + startIndex] = ReadInt<T>();
        return arr;
    }

    /// <summary>Reads a whitespace-delimited 32-bit signed decimal integer.</summary>
    /// <returns>The parsed integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int ReadInt32() => ReadInt<int>();

    /// <summary>Reads a whitespace-delimited 64-bit signed decimal integer.</summary>
    /// <returns>The parsed integer.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long ReadInt64() => ReadInt<long>();

    /// <summary>Reads consecutive 32-bit signed decimal integers in O(count plus consumed bytes) time.</summary>
    /// <param name="count">The nonnegative number of integers to read, representable as an array length.</param>
    /// <returns>An array containing the requested integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int[] ReadInt32(int count) => ReadArray<int>(count);

    /// <summary>Reads consecutive 64-bit signed decimal integers in O(count plus consumed bytes) time.</summary>
    /// <param name="count">The nonnegative number of integers to read, representable as an array length.</param>
    /// <returns>An array containing the requested integers.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long[] ReadInt64(int count) => ReadArray<long>(count);

    /// <summary>Reads a legacy decimal representation by adding fractional digits to the parsed integer part, even when that part is negative.</summary>
    /// <returns>The value produced by the legacy parser; callers requiring standard signed-decimal semantics must use ReadToken and a numeric parser.</returns>
    [Obsolete("Use ReadToken and double.Parse for now")]
    public double ReadDouble() {
        double res = ReadInt64();
        if ((char)Peek() == '.') {
            Read();
            double tail = 0.1;
            while (!EndOfStream && char.IsDigit((char)Peek())) {
                char c = (char)Read();
                res += (c - '0') * tail;
                tail *= 0.1;
            }
        }
        return res;
    }
}
