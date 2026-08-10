# Third-Party Notices

FireFly relies on the following open-source projects. These notices are provided for license and source traceability.

## ac-library-csharp

- Project: [kzrnm/ac-library-csharp](https://github.com/kzrnm/ac-library-csharp)
- Package version: `4.1.3`
- License for the core library source: [CC0 1.0 Universal](https://github.com/kzrnm/ac-library-csharp/blob/v4.1.3/Source/ac-library-csharp/LICENSE)

FireFly references the `ac-library-csharp` NuGet package. SourceExpander-generated submissions may contain the core library source covered by CC0 1.0 Universal. CC0 does not require attribution; this notice is retained to make the source and license easy to audit.

The upstream repository states that files outside `Source/ac-library-csharp` are MIT-licensed. If such files are copied into FireFly in the future, their applicable MIT copyright and permission notices must be preserved.

## SourceExpander

- Project: [kzrnm/SourceExpander](https://github.com/kzrnm/SourceExpander)
- Package version: `9.1.2`
- License: [MIT](https://github.com/kzrnm/SourceExpander/blob/v9.1.2/LICENSE)

FireFly uses SourceExpander as a build-time source embedder and generator. The `SourceExpander.Embedder` package is a private build dependency and is not exposed as a runtime dependency of `Soy.FireFly`.
