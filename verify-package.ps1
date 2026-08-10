param(
    [string]$PackagePath = "",
    [string]$Configuration = "Release"
)

[xml]$libraryProject = Get-Content -LiteralPath (Join-Path $PSScriptRoot "FireFly\FireFly.csproj") -Raw
$version = $libraryProject.Project.PropertyGroup.Version | Select-Object -First 1
if (-not $PackagePath) {
    $PackagePath = Join-Path $PSScriptRoot "artifacts\packages\Soy.FireFly.$version.nupkg"
}
$package = (Resolve-Path -LiteralPath $PackagePath).Path

[xml]$testProject = Get-Content -LiteralPath (Join-Path $PSScriptRoot "FireflyTest\FireflyTest.csproj") -Raw
$sourceExpander = $testProject.Project.ItemGroup.PackageReference |
    Where-Object Include -eq "SourceExpander" |
    Select-Object -First 1
$sourceExpanderVersion = [string]$sourceExpander.Version

$tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
$tempName = "FireFly-Package-" + [Guid]::NewGuid().ToString("N")
$tempDir = [IO.Path]::GetFullPath((Join-Path $tempRoot $tempName))
if (-not $tempDir.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Refusing to use a temporary directory outside the system temp path."
}

$feed = Join-Path $tempDir "feed"
$consumer = Join-Path $tempDir "consumer"
New-Item -ItemType Directory -Path $feed, $consumer | Out-Null

try {
    Copy-Item -LiteralPath $package -Destination $feed
    dotnet new console --framework net9.0 --no-restore --force --output $consumer --name PackageCheck | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not create the package smoke-test project." }

    $project = Join-Path $consumer "PackageCheck.csproj"
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot "FireflyTest\Program.cs") `
        -Destination (Join-Path $consumer "Program.cs") -Force

    dotnet add $project package Soy.FireFly --version $version --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not add Soy.FireFly to the smoke-test project." }
    dotnet add $project package SourceExpander --version $sourceExpanderVersion --no-restore | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "Could not add SourceExpander to the smoke-test project." }

    dotnet restore $project `
        --source $feed `
        --source "https://api.nuget.org/v3/index.json" `
        --nologo
    if ($LASTEXITCODE -ne 0) { throw "Could not restore the package smoke-test project." }

    & (Join-Path $PSScriptRoot "test.ps1") `
        -Configuration $Configuration `
        -Project $project
    if ($LASTEXITCODE -ne 0) { throw "The installed-package smoke tests failed." }

    & (Join-Path $PSScriptRoot "verify-combined.ps1") `
        -Configuration $Configuration `
        -CombinedPath (Join-Path $consumer "Combined.csx")
    if ($LASTEXITCODE -ne 0) { throw "The installed-package Combined.csx check failed." }
}
finally {
    if (Test-Path -LiteralPath $tempDir) {
        $resolved = [IO.Path]::GetFullPath($tempDir)
        $name = [IO.Path]::GetFileName($resolved)
        if ($resolved.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase) -and
            $name.StartsWith("FireFly-Package-", [StringComparison]::Ordinal)) {
            Remove-Item -LiteralPath $resolved -Recurse -Force
        }
    }
}

Write-Host "The packed Soy.FireFly package restores, runs, and expands successfully."
