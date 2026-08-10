# FireFly Agent Code Guide

本文件适用于整个仓库。FireFly 是面向算法竞赛和源码展开提交的 C# 算法库，不是业务系统。代码应当短、直接、可展开、复杂度明确；不要把一个算法包装成企业应用。仓库以 `net9.0` 和 C# 13 为共同基线，因此所有会被展开的源码必须严格兼容 C# 13，并且不得依赖仅在 .NET 10 中提供的 API。

## 风格来源

- 优先参考本仓库中与任务最接近的代码，尤其是 `Utils.cs`、`Discrete.cs`、`DS/Fenwick.cs`、`Graph/`、`IO/` 和 `Extensions/`。
- 同时参考 [ac-library-csharp 的核心源码](https://github.com/kzrnm/ac-library-csharp/tree/main/Source/ac-library-csharp)：公开 API 使用标准算法术语，核心实现由数组、列表和紧凑循环组成。
- `FireFly/Poly/**`、`FireFly/DS/Treap.cs`、`FireFly/DS/FhqTreap.cs` 写法混乱，**不得**作为命名、结构、格式或 API 设计范例。`FireFly/Poly/**` 仅可作为 XML 注释格式的参考。只有任务直接修改这些文件时，才读取其行为和兼容性要求。
- ac-library-csharp 中为 NuGet、调试器展示、文档生成和源码嵌入维护的 `DebugView`、大量 XML 文档、`EditorBrowsable`、条件编译包装等不属于本仓库的默认风格，不要照搬。
- 修改已有文件时保持局部一致，不为统一风格顺手重写无关代码。

## 决策顺序

按以下顺序取舍：

1. 正确性和题意/API 约定；
2. 时间、空间复杂度；
3. 最小且直接的实现；
4. 与附近代码一致；
5. 有实际收益的局部优化；
6. 只有当前需求确实需要时才考虑扩展性。

两个方案都正确时，选择类型更少、层级更少、分配更少、代码路径更短的方案。不要为“以后也许会用”增加结构。

## 开始编码前

1. 找到仓库内最接近的实现和 ac-library-csharp 中对应算法，确认命名、索引和复杂度。
2. 明确当前任务要求的最小公开 API。没有用户要求，不扩展任务边界。
3. 优先复用 BCL、现有 `Graph`、`Fenwick`、运算符接口和 ac-library-csharp 类型，不重复造同类抽象。
4. 保留现有公开 API 和索引约定；破坏性改名或改语义必须由任务明确要求。

## C# 13 兼容性（硬约束）

- **禁止任何 C# 14 或更高版本才支持的语法，以及 preview 或实验语法**。本机安装了更高版本 SDK、项目能以 `net9.0` 构建或 IDE 没有报错，都不能替代明确的语言版本检查。
- 新增或修改的代码必须能以 `/langversion:13` 编译。代码审查时按 C# 13 判断；不要依赖 C# 14 的扩展成员、空条件赋值、字段支持属性或其他后续语法。
- C# 12 和 C# 13 的正式语法可以使用，包括集合表达式、普通类型的主构造函数、非数组 `params` 集合、`ref struct` 接口实现和 partial 属性/索引器。仍应遵守本仓库的直接实现与最少分配原则，不能只为使用新语法增加结构。
- 目标框架保持 `net9.0`。即使 Library Checker 当前使用 .NET 10，也不得调用 .NET 10 才新增的 BCL API，因为 Codeforces 和 AtCoder 的共同运行基线仍是 .NET 9。
- 当前共同边界是：Codeforces 的 C# 13/.NET 9、AtCoder 的 C# 13/.NET 9，以及 Library Checker 的 C# 14/.NET 10。选择提交语言时必须使用对应的现代 C# 选项；本仓库取三者安全交集 C# 13，而不是把 Library Checker 的版本当成上限。
- 仓库旧代码中若已有 C# 14+ 语法，不得把它当成范例。任务触及相关代码时，在不扩大任务范围的前提下改写为 C# 13。

## 发布权限（硬约束）

- 只有用户在**当前对话回合明确要求发布**时，才可以执行任何外部发布动作。先前讨论过发布、版本号已经就绪、代码进入 `main`，都不构成发布授权。
- 未获当轮明确授权时，不得创建或推送发布标签、运行手动发布工作流、执行 `dotnet nuget push`、创建 GitHub Release，或修改 NuGet Trusted Publishing 策略。
- 普通 push、pull request、合并和 CI 只能构建、测试、打包并保存内部构件，绝不能自动发布 NuGet 包或 GitHub Release。

## 完全信任调用方（硬约束）

竞赛题保证输入满足约束，库的调用者也被视为完全可信。**不要做任何输入有效性检查。**

- 不检查 `null`、下标、区间顺序、长度、容量、是否有序、数值正负、模数性质、图端点或其他题目已保证的前置条件。
- 不为无效输入抛出 `ArgumentException`、`ArgumentNullException`、`ArgumentOutOfRangeException` 等异常。
- 不添加 Guard、Validator、`TryValidate`、防御性复制、兜底值、格式容错或包住调用的 `try/catch`。
- 前置条件需要说明时，用一句注释写清索引或数学约束，然后直接执行算法，不在运行时验证。
- 只禁止验证调用方输入；算法本身的必要分支仍须保留。例如 Dijkstra 的已访问判断、拓扑排序检测到环后返回 `null`、模平方根不存在时返回 `false`，都属于算法结果或状态转移，不是输入检查。
- 实现内部可用断言检查开发者维护的不变量，但断言不得用于验证公开参数，也不得进入竞赛热路径。
- 现有输入检查不是新代码的范例。修改恰好涉及它时可直接移除，但不要仅为清理旧检查扩张当前任务。

## 禁止企业化设计

除非用户明确要求，或已经有两个真实用例证明需要，否则不要添加：

- `Service`、`Manager`、`Provider`、`Repository`、`Factory`、`Builder`、`Handler`、`Controller`、`Context`、`Options` 等分层类型；
- DTO、Request/Response、Result 包装类，或仅转发一次调用的 facade；
- 依赖注入、IoC、日志、遥测、配置系统、事件总线、重试策略、缓存框架和序列化层；
- 只有一个实现的接口、空基类，或只为测试而抽象出的适配层；
- 无实际并发需求的 `async`、`Task`、`CancellationToken`、锁和线程安全包装；
- 为未来需求准备的开关、策略、钩子、插件点和额外重载；
- 新项目、新 NuGet 包或新的架构目录。

算法本身需要的零开销抽象是允许的，例如 `ISegtreeOperator<T>`、`IGraphDirection<TSelf>` 和小型运算符 `struct`。它们必须直接参与泛型算法，不能只是套模式。

## 命名

公开类型、方法和属性使用简短准确的 PascalCase，优先采用公认算法名：`Dijkstra`、`TopoSort`、`LowerBound`、`Merge`、`Leader`、`Sum`。不要把标准术语扩写成句子。

局部变量和算法内部字段通常应在 1～8 个字符内；超过约 12 个字符前先确认它确实消除了歧义。短作用域中的公认单字母变量是首选，不是坏味道：

| 含义 | 推荐 | 避免 |
| --- | --- | --- |
| 元素数、边数 | `n`, `m` | `numberOfGraphVertices` |
| 循环/位置 | `i`, `j`, `k`, `p` | `currentIterationIndex` |
| 半开区间 | `l`, `r`, `len` | `leftBoundaryIndex`, `rightBoundaryIndex` |
| 图顶点、边权 | `u`, `v`, `w` | `currentSourceVertex`, `destinationVertexWeight` |
| 起点、终点 | `s`, `t`；公开参数也可用 `from`, `to` | `startingVertexIdentifier` |
| 距离、入度、访问标记 | `dist`, `deg`, `vis` | `calculatedDistanceValues` |
| 队列、映射、操作 | `q`, `mp`, `op` | `vertexProcessingQueue` |
| 结果、答案、临时值 | `res`, `ans`, `tmp` | `algorithmExecutionResult`, `temporaryValueHolder` |
| 数组/缓冲区 | `a`, `b`, `d`, `buf` | `inputValueCollectionData` |

规则：

- 变量名表达一个概念即可，不重复类型、作用域和上下文。
- 避免无信息量后缀：`Data`、`Info`、`Object`、`Instance`、`Implementation`、`Service`、`Manager`、`Provider`。
- 公开参数可比局部变量稍完整，如 `vertexCount`、`edgeCapacity`、`weight`；仍不要写成长句。
- 泛型参数用 `T`，或使用仓库已有的短语义名：`TWeight`、`TOp`、`TDir`、`TMod`。不要叠加多个角色词。
- 新字段默认使用简短 lowerCamel；如果正在修改的类型统一使用 `_field`，则跟随该类型。不要仅为改前缀触碰旧字段。
- 单字母名称只用于约定俗成且作用域较小的值；跨越较长方法时用 `dist`、`head`、`size` 等短词补足语义。

## API 设计

- 一个数据结构对应一个直接可用的类型；一个算法优先对应一个静态方法或扩展方法。
- 参数只包含算法真正需要的量。不要增加 options 对象、上下文对象或默认无用的回调。
- 新 API 默认使用 0 基索引和半开区间 `[l, r)`；已有类型另有约定时服从已有约定，例如不要悄悄改变现有 `Fenwick` 的索引语义。
- 返回最自然的值：基本类型、数组、`Span<T>`、元组、`bool` + `out`，或用 `null` 表示算法性失败。不要仅为给一个值命名而创建结果类。
- 只为当前真实调用方式提供构造函数和重载。先有用例，再加便利 API。
- 泛型只用于数值类型、权值、运算符等算法本身的变化维度，不为“通用性”泛化所有对象。
- 能复用 BCL 接口时直接复用；不要为了包装 `List<T>` 或 `Dictionary<TKey,TValue>` 再定义一套接口。
- 所有公开算法都必须能按下一节说明被 SourceExpander 合并进单文件竞赛程序。

## XML 注释

- `FireFly` 主库中所有调用方可见的 API 都必须有英文 XML 注释，包括 `public`、`protected` 类型及成员，以及接口中隐式公开的成员。覆盖类、结构体、接口、枚举及其成员、委托、构造函数、方法、属性、索引器、字段、常量、事件、运算符和嵌套类型；不为 `internal`、`private` 或显式接口实现补公开 API 注释。
- XML 注释的顶层标签只允许 `<summary>`、`<param>` 和 `<returns>`。可以在这些标签内使用 `<paramref>`、`<see>`、`<c>` 等内联标记；不得使用 `<typeparam>`、`<remarks>`、`<exception>`、`<value>`、`<inheritdoc>`、`<example>` 或其他顶层标签。泛型参数的角色确有必要时写入 `<summary>` 正文。
- 每个公开声明参数列表中的参数都必须有且仅有一个同名 `<param>`，包括委托、索引器、位置 record 以及 `in`、`ref`、`out` 参数；不为属性 setter 或事件访问器的隐式 `value` 添加 `<param>`。非 `void` 方法、非 `void` 委托、运算符和索引器必须有 `<returns>`；构造函数、`void` 方法、普通属性、字段和事件不得添加 `<returns>`。位置 record 在类型声明上用 `<param>` 说明位置参数；同一个 partial 类型只在一个声明处写类型注释。
- 注释是给调用方的契约，必须准确说明行为、有效输入的前置条件、索引与区间约定、失败或哨兵结果、是否修改或包装传入存储等可观察语义。直接陈述调用方必须满足的条件，不添加对应的运行时检查，也不在每个 API 中重复“本库不检查参数”。
- 英文使用简短完整的句子并以句号结尾。非 `O(1)` 或复杂度不直观的公开操作应在 `<summary>` 中注明时间复杂度；仅在额外空间显著时说明空间复杂度。`FireFly/Poly/**` 的既有注释可作为动词、句式和标签布局参考，但不得照搬其中含糊或缺失的前置条件。
- `FireFly/DS/Treap.cs` 和 `FireFly/DS/FhqTreap.cs` 正在等待重构。在重构任务明确涉及它们之前，不读取、不修改，也不为其补注释、pragma 或验证辅助标记。

## SourceExpander 与竞赛提交

本库通过 [kzrnm/SourceExpander](https://github.com/kzrnm/SourceExpander) 把实际用到的 FireFly 源码复制到算法竞赛主程序中，而不是在评测机上依赖 FireFly DLL。

工作方式：

1. `FireFly` 项目引用 `SourceExpander.Embedder`。编译库时，它把源码和依赖信息嵌入程序集元数据。
2. 竞赛项目同时引用 FireFly 和 `SourceExpander`，并在 `Program.cs` 中调用 `SourceExpander.Expander.Expand()`。
3. 编译或运行竞赛项目后，SourceExpander 生成 `Combined.csx`，把主程序实际引用到的 FireFly 类型及其依赖放进 `#region Expanded ...`。
4. 提交 `Combined.csx` 的单文件内容。评测机编译复制后的源码，因此其中每一行都必须符合仓库的 C# 13/.NET 9 共同基线。

例如，竞赛主程序只需要正常使用库：

```csharp
using System;
using FireFly.Graph;

SourceExpander.Expander.Expand();

int n = int.Parse(Console.ReadLine()!);
Graph<Directed, NoWeight> g = new(n, n);
// 读边并调用 g.AddEdge(...)
int[]? ord = g.TopoSort();
Console.WriteLine(string.Join(" ", ord!));
```

生成的 `Combined.csx` 在主程序后追加所需源码，概念上类似：

```csharp
// 原主程序
int[]? ord = g.TopoSort();

#region Expanded by https://github.com/kzrnm/SourceExpander
namespace FireFly.Graph {
    public static class DirectedExtensions {
        // TopoSort<T> 的完整方法体会被复制到这里
    }
    // Graph<TDir, TWeight>、Directed、GraphCore<T> 等依赖也会一并复制
}
#endregion
```

因此：

- 每个源文件显式写出自己需要的 `using`；不要依赖项目的 `ImplicitUsings` 或竞赛主程序碰巧已有的 using。
- 不依赖反射扫描、外部资源、运行时配置、当前工作目录或仅存在于 FireFly 项目中的生成文件。
- 类型依赖保持明确，辅助类型放在正常 namespace 中，使 SourceExpander 能追踪并复制。
- 展开结果才是最终产品。检查兼容性时必须考虑 `Combined.csx`，不能只看 FireFly DLL 是否构建成功。

## 实现风格

- 使用文件作用域 namespace、4 空格缩进，左花括号放在声明或控制语句同一行，与仓库主体一致。
- 小属性、小运算和简单转发可用表达式体；短 guard/循环体可单行。逻辑超过一句时使用清晰代码块。
- 优先普通数组、`List<T>`、`Span<T>`、`Queue<T>`、`PriorityQueue<TElement,TPriority>` 和直接循环。
- 已知容量时预分配。只有在数组随后会被完全赋值时才使用 `GC.AllocateUninitializedArray`。
- 热路径中，简单 `for`/`foreach` 优先于产生迭代器、闭包或中间集合的 LINQ 链。
- 自定义枚举器、`Unsafe`、池化和复杂位技巧必须有明确的性能或内存理由；不能作为默认模板。
- `readonly` 用于确实不再赋值的状态；`struct` 用于小型值或零状态运算符。不要机械地给所有类加 `sealed`。
- `[MethodImpl(AggressiveInlining)]` 只用于很小且明确的热方法，或为保持附近实现一致；不要装饰每个方法和访问器。
- 移除无用 `using`、死代码、注释掉的旧实现和无意义空行，但不要借机清理任务外文件。
- 注释只解释索引约定、关键不变量、复杂度或不直观技巧。不要逐行复述代码，不写大段模板化 XML 文档。
- 不添加输入检查、Guard、Validator 或输入异常；直接依赖题目和调用方满足前置条件。

## 正反例

不要这样写：

```csharp
public sealed class ShortestPathComputationService<TVertexIdentifier> {
    private readonly IGraphDataRepository<TVertexIdentifier> graphDataRepository;

    public ShortestPathComputationResult Execute(
        ShortestPathComputationRequest request,
        CancellationToken cancellationToken = default) {
        // ...
    }
}
```

应当直接写成算法：

```csharp
public static int[] Bfs(this Graph<Directed, NoWeight> graph, int from) {
    int n = graph.VertexCount;
    int[] dist = new int[n];
    Array.Fill(dist, -1);
    Queue<int> q = new();
    q.Enqueue(from);
    dist[from] = 0;

    while (q.Count > 0) {
        int u = q.Dequeue();
        foreach (int v in graph.GetNeighbors(u)) {
            if (dist[v] >= 0) continue;
            dist[v] = dist[u] + 1;
            q.Enqueue(v);
        }
    }
    return dist;
}
```

如果一个私有辅助方法只被调用一次，且内联后仍然清楚，直接内联。若它封装了独立不变量、重复逻辑或能显著降低主算法复杂度，才保留辅助方法。

## 测试与验证

- 数据结构和算法优先使用小型确定性样例；复杂算法再用朴素实现做小规模随机对拍。
- 不为测试引入 mock 框架、fixture 层、测试数据 builder 或新的测试抽象。
- 验证新增和修改的源码只使用 C# 13 或更早语法、没有调用 .NET 10-only API，并检查 SourceExpander 生成的 `Combined.csx`；默认 `net9.0` 构建成功不能替代展开结果检查。
- 忽略所有 SourceExpander 带来的警告。
- 用户要求构建时，应该使用 Release 配置构建至 `FireFly/bin/Release/net9.0` 中。
- 不依赖 Visual Studio 的启动项目冒烟测试：

  ```powershell
  .\test.ps1
  ```

- 修改 C# 后至少运行：

  ```powershell
  dotnet build FireFly.sln -c Release -p:GeneratePackageOnBuild=false
  ```

- 若修改 `FireflyTest/Program.cs` 做临时验证，提交结果前恢复与任务无关的试验代码。不要手改 `bin/`、`obj/`、`.nupkg` 或生成的 `Combined.csx`。

## 提交前自检

- 是否只实现了用户要求的功能？
- 是否能删掉某个新类型、接口、包装层、重载或配置项而不损失当前功能？能就删。
- 是否把局部变量写成了句子？能用 `n`、`l`、`r`、`u`、`dist`、`res` 等公认短名就缩短。
- 公开 API 是否使用标准算法术语，并保持已有索引语义？
- 新增或修改的每一处语法是否都属于 C# 13 或更早版本？
- 是否完全信任调用方，删除了所有输入有效性检查和防御性包装？
- 复杂度是否达到该算法的通常预期，且没有不必要分配？
- SourceExpander 展开后是否仍是自包含、显式 using、可由 C# 13 和 .NET 9 编译的源码？
- 是否误以 `Poly`、`Treap` 或 `FhqTreap` 为风格依据？
- 是否加入了未经请求的企业特性或依赖？
- 是否完成了最小构建和针对性验证？

向用户交付时只需简要说明改了什么、关键复杂度和验证结果，不要附带架构宣言。
