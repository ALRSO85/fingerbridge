param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.ScriptName
$repoRoot = (Resolve-Path (Join-Path $scriptDir "..")).Path
$libPath = Join-Path $repoRoot "lib\nitgen"
$testerBin = Join-Path $repoRoot ("src\FingerBridge.WinFormsTester\bin\" + $Configuration)

Write-Host "Nitgen/eNBioBSP runtime check"
Write-Host "RepoRoot: $repoRoot"

$paths = @(
    @{ Name = "lib\nitgen"; Path = $libPath },
    @{ Name = "tester bin"; Path = $testerBin }
)

$required = @("NITGEN.SDK.NBioBSP.dll", "NBioBSP.dll")
$optional = @("NBioAPI.dll", "NImgConv.dll", "NBioBSPCOM.dll")

foreach ($entry in $paths) {
    Write-Host ""
    Write-Host $entry.Name -ForegroundColor Cyan
    Write-Host $entry.Path

    if (-not (Test-Path $entry.Path)) {
        Write-Host "Pasta nao existe." -ForegroundColor Yellow
        continue
    }

    foreach ($file in $required) {
        $full = Join-Path $entry.Path $file
        if (Test-Path $full) {
            Write-Host "OK obrigatório: $file" -ForegroundColor Green
        } else {
            Write-Host "Faltando obrigatório: $file" -ForegroundColor Red
        }
    }

    foreach ($file in $optional) {
        $full = Join-Path $entry.Path $file
        if (Test-Path $full) {
            Write-Host "OK opcional: $file" -ForegroundColor Green
        } else {
            Write-Host "Nao encontrado opcional: $file" -ForegroundColor DarkYellow
        }
    }
}
