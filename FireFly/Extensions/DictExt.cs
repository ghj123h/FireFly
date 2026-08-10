using System.Numerics;
using System.Runtime.CompilerServices;

namespace FireFly.Extensions;

/// <summary>Provides dictionary update extensions.</summary>
public static class DictExt {
    /// <summary>Adds a numeric value at a key using one membership test followed by an indexer read and write or an insertion.</summary>
    /// <param name="mp">The nonnull writable dictionary to modify.</param>
    /// <param name="key">A key accepted by the dictionary.</param>
    /// <param name="value">The value to add.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Inc<TKey, TValue>(this IDictionary<TKey, TValue> mp, TKey key, TValue value)
        where TValue : INumberBase<TValue> {
        if (mp.ContainsKey(key)) mp[key] += value;
        else mp.Add(key, value);
    }

    /// <summary>Appends a value using one dictionary lookup and one list append, creating and inserting a list when the key is absent; the list implementation must support Add.</summary>
    /// <param name="mp">The nonnull writable dictionary to modify; stored lists must be nonnull and support Add.</param>
    /// <param name="key">A key accepted by the dictionary whose list receives the value.</param>
    /// <param name="value">The value to append.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Inc<TKey, TList, TValue>(this IDictionary<TKey, TList> mp, TKey key, TValue value)
        where TList : IList<TValue>, new() {
        if (mp.TryGetValue(key, out var list)) list.Add(value);
        else mp.Add(key, new TList { value });
    }
}
