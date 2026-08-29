# FireFly

[English](https://github.com/ghj123h/FireFly/blob/main/README.md)

FireFly 是一个紧凑的 C# 13 算法竞赛库。它力求让竞赛代码保持直接，并尽量减少不必要的内存分配；借助 [SourceExpander](https://github.com/kzrnm/SourceExpander)，解题程序实际用到的源码可以展开为单文件 `Combined.csx`，直接用于提交。

> **项目状态：** FireFly 目前处于 `0.1` alpha 阶段，公开 API 在 `1.0` 之前仍可能调整。

## 安装

NuGet 包 ID 为 `Soy.FireFly`，首个公开版本为 `0.1.0-alpha.1`。该版本在 NuGet 上可用时，请将它与 SourceExpander 一并安装：

```sh
dotnet add path/to/Contest.csproj package Soy.FireFly --version 0.1.0-alpha.1
dotnet add path/to/Contest.csproj package SourceExpander --version 9.1.2
```

如果该版本尚未发布，或需要基于本地检出进行开发，请克隆本仓库，并用项目引用替代上面的 `Soy.FireFly` 包安装命令：

```sh
dotnet add path/to/Contest.csproj reference path/to/FireFly/FireFly.csproj
```

## 展开源码后提交

竞赛程序可以像使用普通类库一样使用 FireFly：

```csharp
using System;
using FireFly.Graph;

SourceExpander.Expander.Expand();

Graph<Directed, NoWeight> g = new(4, 3);
g.AddEdge(0, 1);
g.AddEdge(0, 2);
g.AddEdge(2, 3);

int[]? order = g.TopoSort();
Console.WriteLine(string.Join(" ", order!));
```

竞赛项目需要同时引用 FireFly 和 `SourceExpander` 生成器。构建或运行项目后，提交生成的 `Combined.csx` 即可。SourceExpander 会把程序所需的 FireFly 源码附加到该文件中，因此评测机不需要 FireFly DLL。

## 包含内容

- 二分查找、最长递增子序列、离散化和滑动窗口最大值等通用算法。
- 树状数组（Fenwick tree）、有序映射等数据结构。
- 使用邻接表存储的有向图与无向图，以及拓扑排序和最短路算法。
- 面向竞赛程序的缓冲输入与输出。
- 位掩码、数组和字典辅助方法。
- 以 `ac-library-csharp` 类型为基础的模意义下的组合数学与多项式算法。

`FireFly.Tests` 包含 xUnit 回归测试；`FireFly.Smoke` 是用于验证 SourceExpander 和 NuGet 消费方式的小型自包含程序。

## 兼容性

FireFly 以 `net9.0` 为目标框架，并将源码语言基线设为 C# 13。生成后的源码面向 Codeforces、AtCoder 和 Library Checker 当前提供的 C# 环境；提交前仍应确认评测平台正在使用的语言版本。目前不承诺兼容 Native AOT。

## 构建与测试

在仓库根目录执行：

```powershell
dotnet build FireFly.sln -c Release -p:GeneratePackageOnBuild=false
.\test.ps1
```

## 许可证与致谢

FireFly 以 [MIT License](LICENSE) 许可发布。

本库依赖 [ac-library-csharp](https://github.com/kzrnm/ac-library-csharp)，其核心库源码依据 CC0 1.0 发布；同时使用 [SourceExpander](https://github.com/kzrnm/SourceExpander) 生成单文件源码。详情请参阅[第三方声明](THIRD-PARTY-NOTICES.md)。
