using AtCoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FireFly.DS.Operators;
/// <summary>Defines addition for value domains where the operation is associative and zero is its identity.</summary>
public struct AddOp<T> : ISegtreeOperator<T> where T : INumberBase<T> {
    /// <summary>Adds two values.</summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The sum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Operate(T a, T b) => a + b;
    /// <summary>Gets the additive identity.</summary>
    public readonly T Identity => T.Zero;
}
/// <summary>Defines minimum for value domains where the operation is associative and the maximum representable value is its identity.</summary>
public struct MinOp<T> : ISegtreeOperator<T> where T : INumber<T>, IMinMaxValue<T> {
    /// <summary>Returns the smaller of two values.</summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The smaller of <paramref name="a"/> and <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Operate(T a, T b) => T.Min(a, b);
    /// <summary>Gets the maximum representable value, which is the identity for minimum.</summary>
    public readonly T Identity => T.MaxValue;
}
/// <summary>Defines maximum for value domains where the operation is associative and the minimum representable value is its identity.</summary>
public struct MaxOp<T> : ISegtreeOperator<T> where T : INumber<T>, IMinMaxValue<T> {
    /// <summary>Returns the larger of two values.</summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The larger of <paramref name="a"/> and <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Operate(T a, T b) => T.Max(a, b);
    /// <summary>Gets the minimum representable value, which is the identity for maximum.</summary>
    public readonly T Identity => T.MinValue;
}
/// <summary>Defines the bitwise AND monoid.</summary>
public struct AndOp<T> : ISegtreeOperator<T> where T: IBinaryNumber<T> {
    /// <summary>Computes the bitwise AND of two values.</summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The bitwise AND of <paramref name="a"/> and <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Operate(T a, T b) => a & b;
    /// <summary>Gets the all-bits-set identity.</summary>
    public readonly T Identity => T.AllBitsSet;
}
/// <summary>Defines the bitwise OR monoid.</summary>
public struct OrOp<T> : ISegtreeOperator<T> where T : IBinaryNumber<T> {
    /// <summary>Computes the bitwise OR of two values.</summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The bitwise OR of <paramref name="a"/> and <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Operate(T a, T b) => a | b;
    /// <summary>Gets the zero identity.</summary>
    public readonly T Identity => T.Zero;
}
/// <summary>Defines the bitwise XOR monoid.</summary>
public struct XorOp<T> : ISegtreeOperator<T> where T : IBinaryNumber<T> {
    /// <summary>Computes the bitwise XOR of two values.</summary>
    /// <param name="a">The first value.</param>
    /// <param name="b">The second value.</param>
    /// <returns>The bitwise XOR of <paramref name="a"/> and <paramref name="b"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Operate(T a, T b) => a ^ b;
    /// <summary>Gets the zero identity.</summary>
    public readonly T Identity => T.Zero;
}
