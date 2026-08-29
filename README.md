# FireFly

[简体中文](https://github.com/ghj123h/FireFly/blob/main/README.zh-CN.md)

FireFly is a compact C# 13 algorithm library for competitive programming. It keeps contest code direct and allocation-conscious, while [SourceExpander](https://github.com/kzrnm/SourceExpander) turns the parts used by a solution into a single `Combined.csx` submission file.

> **Status:** FireFly is in `0.1` alpha. APIs may change before `1.0`.

## Installation

The NuGet package ID is `Soy.FireFly`, and the first release is `0.1.0-alpha.1`. When that version is available on NuGet, install it together with SourceExpander:

```sh
dotnet add path/to/Contest.csproj package Soy.FireFly --version 0.1.0-alpha.1
dotnet add path/to/Contest.csproj package SourceExpander --version 9.1.2
```

If the version has not been published yet, or when developing against a local checkout, clone this repository and use a project reference instead of the `Soy.FireFly` package command:

```sh
dotnet add path/to/Contest.csproj reference path/to/FireFly/FireFly.csproj
```

## Source-expanded submission

A contest program uses FireFly as an ordinary library:

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

The contest project must reference both FireFly and the `SourceExpander` generator. Build or run it, then submit the generated `Combined.csx`. SourceExpander appends the required FireFly source to that file, so the judge does not need the FireFly DLL.

## What is included

- General algorithms such as binary search, longest increasing subsequence, coordinate compression, and sliding-window maxima.
- Data structures including Fenwick trees and ordered maps.
- Directed and undirected adjacency-list graphs, topological sorting, and shortest-path algorithms.
- Buffered input and output for contest programs.
- Bit-mask, array, and dictionary helpers.
- Modular combinatorics and polynomial algorithms built around `ac-library-csharp` types.

`FireFly.Tests` contains the xUnit regression suite. `FireFly.Smoke` is a small, self-contained SourceExpander and NuGet consumer check.

## Compatibility

FireFly targets `net9.0` and sets the source-language baseline to C# 13. Generated source is intended for the current C# environments on Codeforces, AtCoder, and Library Checker; check a judge's active language version before submitting. Native AOT compatibility is not currently promised.

## Build and test

From the repository root:

```powershell
dotnet build FireFly.sln -c Release -p:GeneratePackageOnBuild=false
.\test.ps1
```

## License and acknowledgements

FireFly is distributed under the [MIT License](LICENSE).

The library depends on [ac-library-csharp](https://github.com/kzrnm/ac-library-csharp), whose core library source is released under CC0 1.0, and uses [SourceExpander](https://github.com/kzrnm/SourceExpander) for single-file generation. See [Third-Party Notices](THIRD-PARTY-NOTICES.md) for details.
