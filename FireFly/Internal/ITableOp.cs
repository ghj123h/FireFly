using AtCoder;

namespace FireFly.Internal;

internal interface ITableOp<TMod> where TMod : struct, IStaticMod {
    static abstract StaticModInt<TMod>[] Init();
    static abstract void Extend(StaticModInt<TMod>[] values, int from, int to);
}
