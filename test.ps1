param(
    [string]$Configuration = "Debug",
    [string]$Project = ""
)

# FireflyTest CLI smoke test. Visual Studio is not required.
$proj = if ($Project) {
    [IO.Path]::GetFullPath($Project)
}
else {
    Join-Path $PSScriptRoot "FireflyTest\FireflyTest.csproj"
}

dotnet build $proj --configuration $Configuration --nologo `
    -p:GeneratePackageOnBuild=false `
    -clp:ErrorsOnly
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$writerOut = dotnet run --project $proj `
    --configuration $Configuration `
    --no-build `
    --no-restore `
    -p:GeneratePackageOnBuild=false `
    -- --buffered-writer-tests
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$writerActual = ($writerOut -join "`n").Trim()
if ($writerActual -ne "BufferedWriter tests passed.") {
    Write-Error "BufferedWriter tests failed: '$writerActual'."
    exit 1
}

$numericsOut = dotnet run --project $proj `
    --configuration $Configuration `
    --no-build `
    --no-restore `
    -p:GeneratePackageOnBuild=false `
    -- --numerics-tests
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$numericsActual = ($numericsOut -join "`n").Trim()
if ($numericsActual -ne "Numerics tests passed.") {
    Write-Error "Numerics tests failed: '$numericsActual'."
    exit 1
}

$treapOut = dotnet run --project $proj `
    --configuration $Configuration `
    --no-build `
    --no-restore `
    -p:GeneratePackageOnBuild=false `
    -- --treap-tests
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$treapActual = ($treapOut -join "`n").Trim()
if ($treapActual -ne "Treap tests passed.") {
    Write-Error "Treap tests failed: '$treapActual'."
    exit 1
}

$caseData = "4 3`n1 2`n1 3`n3 4`n"
$expected = "1 3 2 0"
$out = $caseData | dotnet run --project $proj `
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

Write-Host $writerActual
Write-Host $numericsActual
Write-Host $treapActual
Write-Host "FireflyTest passed: $actual"
