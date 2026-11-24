using AtCoder;
using System;

namespace FireFly.Internal;

internal static class Table<TMod, TOp>
    where TMod : struct, IStaticMod
    where TOp : struct, ITableOp<TMod> {
    private static StaticModInt<TMod>[] values = TOp.Init();

    internal static ReadOnlySpan<StaticModInt<TMod>> Get(int n) {
        int cur = values.Length;
        if (cur <= n) {
            int len = (int)Math.Min((long)StaticModInt<TMod>.Mod, Math.Max((long)cur << 1, n + 1L));
            Array.Resize(ref values, len);
            TOp.Extend(values, cur, len);
        }
        return values.AsSpan(0, n + 1);
    }
}
