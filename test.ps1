param(
    [string]$Configuration = "Release",
    [string]$Project = ""
)

# Runs the repository regression tests and smoke test. When Project is set, only
# the smoke contract is run for the supplied package-consumer project.
$smoke = if ($Project) {
    [IO.Path]::GetFullPath($Project)
}
else {
    Join-Path $PSScriptRoot "FireFly.Smoke\FireFly.Smoke.csproj"
}

$build = if ($Project) {
    $smoke
}
else {
    Join-Path $PSScriptRoot "FireFly.sln"
}

dotnet build $build --configuration $Configuration --nologo `
    -p:GeneratePackageOnBuild=false `
    -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $Project) {
    $tests = Join-Path $PSScriptRoot "FireFly.Tests\FireFly.Tests.csproj"
    dotnet test $tests --configuration $Configuration --nologo `
        --no-build `
        --no-restore `
        -p:GeneratePackageOnBuild=false
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$caseData = "4 3`n1 2`n1 3`n3 4`n"
$expected = "1 3 2 0"
$out = $caseData | dotnet run --project $smoke `
    --configuration $Configuration `
    --no-build `
    --no-restore `
    -p:GeneratePackageOnBuild=false
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$actual = ($out -join "`n").Trim()
if ($actual -ne $expected) {
    Write-Error "Expected '$expected', got '$actual'."
    exit 1
}

Write-Host "FireFly.Smoke passed: $actual"
