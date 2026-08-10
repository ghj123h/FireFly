using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace FireFly.IO;

/// <summary>Buffers UTF-8 output in memory until it is written to an underlying stream, without taking ownership of that stream.</summary>
public class BufferedWriter {
    private readonly Stream sw;
    private byte[] buffer = new byte[1 << 16];
    private int p;

    /// <summary>Creates a buffered writer for standard output.</summary>
    public BufferedWriter() : this(Console.OpenStandardOutput()) { }

    /// <summary>Creates a buffered writer over the specified writable stream.</summary>
    /// <param name="stream">The nonnull writable stream, which remains owned by the caller.</param>
    public BufferedWriter(Stream stream) { sw = stream; }

    private void EnsureCapacity(int count) {
        if (p + count <= buffer.Length) return;
        Array.Resize(ref buffer, Math.Max(buffer.Length << 1, p + count));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Write(byte c) {
        EnsureCapacity(1);
        buffer[p++] = c;
    }

    private void Write(char c) {
        if (c < 0x80) {
            Write((byte)c);
            return;
        }

        EnsureCapacity(3);
        Span<char> s = stackalloc char[1];
        s[0] = c;
        p += Encoding.UTF8.GetBytes(s, buffer.AsSpan(p));
    }

    private void Write(string s) {
        int count = Encoding.UTF8.GetByteCount(s);
        EnsureCapacity(count);
        p += Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, p);
    }

    private void Write<T>(T num) where T : IBinaryInteger<T> {
        bool neg = num < T.Zero;
        if (neg) Write((byte)'-');

        int l = p;
        T ten = T.CreateChecked(10);
        do {
            T d = num % ten;
            if (neg) d = -d;
            Write((byte)('0' + int.CreateChecked(d)));
            num /= ten;
        } while (num != T.Zero);
        Array.Reverse(buffer, l, p - l);
    }

    /// <summary>Appends an integer in decimal notation in amortized O(digits) time without flushing the buffer.</summary>
    /// <param name="num">The integer to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append<T>(T num) where T : IBinaryInteger<T> => Write(num);

    /// <summary>Appends a string encoded as UTF-8 in amortized O(encoded bytes) time without flushing the buffer.</summary>
    /// <param name="s">The nonnull string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(string s) => Write(s);

    /// <summary>Appends an LF byte without flushing the buffer.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine() => Write((byte)'\n');

    /// <summary>Appends an integer in decimal notation followed by an LF byte in amortized O(digits) time.</summary>
    /// <param name="num">The integer to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine<T>(T num) where T : IBinaryInteger<T> {
        Write(num);
        Write((byte)'\n');
    }

    /// <summary>Appends a UTF-8 string followed by an LF byte in amortized O(encoded bytes) time.</summary>
    /// <param name="s">The nonnull string to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendLine(string s) {
        Write(s);
        Write((byte)'\n');
    }

    /// <summary>Appends "Yes" followed by an LF byte.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendYes() {
        Write((byte)'Y');
        Write((byte)'e');
        Write((byte)'s');
        Write((byte)'\n');
    }

    /// <summary>Appends "No" followed by an LF byte.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendNo() {
        Write((byte)'N');
        Write((byte)'o');
        Write((byte)'\n');
    }

    /// <summary>Appends "Yes" for a true condition or "No" otherwise, followed by an LF byte.</summary>
    /// <param name="suc">The condition that selects the text to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendYes(bool suc) {
        if (suc) AppendYes();
        else AppendNo();
    }

    /// <summary>Appends decimal integers separated by spaces and terminated by an LF byte in amortized O(output bytes) time.</summary>
    /// <param name="values">The nonnull integers to enumerate once and append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendJoin<T>(IEnumerable<T> values) where T : IBinaryInteger<T> => AppendJoin(' ', values);

    /// <summary>Appends decimal integers separated by a character and terminated by an LF byte in amortized O(output bytes) time.</summary>
    /// <param name="separator">The separator written between adjacent integers.</param>
    /// <param name="values">The nonnull integers to enumerate once and append.</param>
    public void AppendJoin<T>(char separator, IEnumerable<T> values) where T : IBinaryInteger<T> {
        bool first = true;
        foreach (T num in values) {
            if (!first) Write(separator);
            Write(num);
            first = false;
        }
        Write((byte)'\n');
    }

    /// <summary>Appends decimal integers separated by a string and terminated by an LF byte in amortized O(output bytes) time.</summary>
    /// <param name="separator">The nonnull UTF-8 separator written between adjacent integers.</param>
    /// <param name="values">The nonnull integers to enumerate once and append.</param>
    public void AppendJoin<T>(string separator, IEnumerable<T> values) where T : IBinaryInteger<T> {
        bool first = true;
        foreach (T num in values) {
            if (!first) Write(separator);
            Write(num);
            first = false;
        }
        Write((byte)'\n');
    }

    /// <summary>Appends decimal integers separated by spaces and terminated by an LF byte in amortized O(output bytes) time.</summary>
    /// <param name="values">The integers to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AppendJoin<T>(ReadOnlySpan<T> values) where T : IBinaryInteger<T> => AppendJoin(' ', values);

    /// <summary>Appends decimal integers separated by a character and terminated by an LF byte in amortized O(output bytes) time.</summary>
    /// <param name="separator">The separator written between adjacent integers.</param>
    /// <param name="values">The integers to append.</param>
    public void AppendJoin<T>(char separator, ReadOnlySpan<T> values) where T : IBinaryInteger<T> {
        for (int i = 0; i < values.Length; ++i) {
            if (i > 0) Write(separator);
            Write(values[i]);
        }
        Write((byte)'\n');
    }

    /// <summary>Does nothing and is retained only for compatibility.</summary>
    /// <param name="format">The ignored format string.</param>
    /// <param name="args">The ignored arguments.</param>
    [Obsolete]
    public void AppendFormat(string format, params object?[] args) { }

    /// <summary>Copies all buffered bytes to the stream in O(buffered bytes) local work, flushes it, and clears the buffered content; underlying I/O costs are additional.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Output() {
        sw.Write(buffer, 0, p);
        sw.Flush();
        p = 0;
    }
}
