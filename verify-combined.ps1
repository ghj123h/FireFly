param(
    [string]$Configuration = "Release",
    [string]$CombinedPath = ""
)

$combined = if ($CombinedPath) {
    [IO.Path]::GetFullPath($CombinedPath)
}
else {
    Join-Path $PSScriptRoot "FireFly.Smoke\Combined.csx"
}
if (-not (Test-Path -LiteralPath $combined)) {
    throw "Combined.csx was not generated. Run FireFly.Smoke first."
}

$source = Get-Content -LiteralPath $combined -Raw
foreach ($marker in @("#region Expanded by", "namespace FireFly", "namespace AtCoder")) {
    if (-not $source.Contains($marker, [StringComparison]::Ordinal)) {
        throw "Combined.csx is not self-contained: missing '$marker'."
    }
}

$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$tempName = "FireFly-Combined-" + [Guid]::NewGuid().ToString("N")
$tempDir = [IO.Path]::GetFullPath((Join-Path $tempRoot $tempName))
if (-not $tempDir.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to use a temporary directory outside the system temp path."
}

New-Item -ItemType Directory -Path $tempDir | Out-Null
try {
    dotnet new console --framework net9.0 --no-restore --force --output $tempDir --name CombinedCheck | Out-Null
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    Copy-Item -LiteralPath $combined -Destination (Join-Path $tempDir "Program.cs") -Force
    $project = Join-Path $tempDir "CombinedCheck.csproj"
    dotnet build $project --configuration $Configuration --nologo `
        -p:LangVersion=13 `
        -p:Nullable=annotations `
        -p:TreatWarningsAsErrors=true `
        -clp:ErrorsOnly
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    if (Test-Path -LiteralPath $tempDir) {
        $resolved = [IO.Path]::GetFullPath($tempDir)
        $name = [IO.Path]::GetFileName($resolved)
        if ($resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and
            $name.StartsWith("FireFly-Combined-", [StringComparison]::Ordinal)) {
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
    }
}

Write-Host "Combined.csx is self-contained and compiles with C# 13 on net9.0."
