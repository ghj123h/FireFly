# Changelog

All notable changes to FireFly are documented in this file.

## [0.1.0-alpha.2] - 2026-11-25

### Added

- Polynomial Taylor shift (`Poly<TMod>.Shift`) and formal power series composition (`Poly<TMod>.Comp`).
- Generic integer `Numerics.Gcd` and `Numerics.Lcm` constrained to `IBinaryInteger<T>`.

### Changed

- Split modular combinatorics and modular arithmetic out of `Numerics` into `Comb` (`GetFacts`, `GetInvFacts`, `Binom`, `Perm`) and `ModMath` (`GetInvs`, `Sqrt`), backed by full-capacity doubling per-modulus caches in `FireFly.Internal`.
- Split the test suite into `FireFly.Tests` (xUnit regression tests) and `FireFly.Smoke` (SourceExpander and NuGet consumer check).

## [0.1.0-alpha.1] - 2026-08-19

### Added

- Initial public alpha of the FireFly competitive-programming library.
- C# 13 and `net9.0` baseline.
- SourceExpander support for single-file contest submissions.
- NuGet packaging metadata for the planned `Soy.FireFly` package.
