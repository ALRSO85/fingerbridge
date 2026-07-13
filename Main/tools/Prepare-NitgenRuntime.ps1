param(
    [string]$SdkBinPath = "",
    [string]$Configuration = "Debug",
    [switch]$CopyToTesterBin
)

$ErrorActionPreference = "Stop"

function Write-Step([string]$Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Resolve-RepoRoot {
    $scriptDir = Split-Path -Parent $MyInvocation.ScriptName
    return (Resolve-Path (Join-Path $scriptDir "..")).Path
}

function Test-NitgenBin([string]$Path) {
    if ([string]::IsNullOrWhiteSpace($Path)) { return $false }
    if (-not (Test-Path $Path)) { return $false }

    $managed = Join-Path $Path "NITGEN.SDK.NBioBSP.dll"
    $native = Join-Path $Path "NBioBSP.dll"
    return ((Test-Path $managed) -and (Test-Path $native))
}

function Find-NitgenBinCandidates {
    $roots = @()

    if ($env:ProgramFiles) { $roots += $env:ProgramFiles }
    if (${env:ProgramFiles(x86)}) { $roots += ${env:ProgramFiles(x86)} }
    if ($env:ProgramW6432) { $roots += $env:ProgramW6432 }

    $roots = $roots | Where-Object { $_ -and (Test-Path $_) } | Select-Object -Unique

    $candidates = New-Object System.Collections.Generic.List[string]

    foreach ($root in $roots) {
        $common = @(
            "NITGEN",
            "NITGEN eNBSP",
            "NITGEN eNBSP x64",
            "NITGEN eNBioBSP",
            "NITGEN eNBioBSP x64",
            "eNBioBSP",
            "eNBSP"
        )

        foreach ($name in $common) {
            $base = Join-Path $root $name
            if (-not (Test-Path $base)) { continue }

            $possible = @(
                $base,
                (Join-Path $base "Bin"),
                (Join-Path $base "SDK"),
                (Join-Path $base "SDK\Bin"),
                (Join-Path $base "SDK\Bin\x86"),
                (Join-Path $base "SDK\Bin\x64"),
                (Join-Path $base "Bin\x86"),
                (Join-Path $base "Bin\x64")
            )

            foreach ($p in $possible) {
                if (Test-Path $p) { $candidates.Add($p) }
            }
        }

        Get-ChildItem -Path $root -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match "NITGEN|NBio|eNBio|eNBSP" } |
            ForEach-Object {
                $possible = @(
                    $_.FullName,
                    (Join-Path $_.FullName "Bin"),
                    (Join-Path $_.FullName "SDK\Bin"),
                    (Join-Path $_.FullName "SDK\Bin\x86"),
                    (Join-Path $_.FullName "SDK\Bin\x64")
                )

                foreach ($p in $possible) {
                    if (Test-Path $p) { $candidates.Add($p) }
                }
            }
    }

    return $candidates | Select-Object -Unique
}

$repoRoot = Resolve-RepoRoot
$targetLib = Join-Path $repoRoot "lib\nitgen"
$testerBin = Join-Path $repoRoot ("src\FingerBridge.WinFormsTester\bin\" + $Configuration)

Write-Host "Nitgen/eNBioBSP runtime preparation"
Write-Host "RepoRoot: $repoRoot"
Write-Host "Target lib: $targetLib"

if (-not (Test-Path $targetLib)) {
    New-Item -ItemType Directory -Path $targetLib | Out-Null
}

if ([string]::IsNullOrWhiteSpace($SdkBinPath)) {
    Write-Step "Procurando pasta Bin do SDK Nitgen/eNBioBSP"
    $found = $null
    foreach ($candidate in Find-NitgenBinCandidates) {
        if (Test-NitgenBin $candidate) {
            $found = $candidate
            break
        }
    }

    if (-not $found) {
        Write-Host "Nao encontrei automaticamente a pasta Bin do SDK." -ForegroundColor Yellow
        Write-Host "Instale o eNBioBSP_v5.2.0.6.exe e rode novamente informando -SdkBinPath." -ForegroundColor Yellow
        Write-Host "Exemplo:"
        Write-Host "powershell -ExecutionPolicy Bypass -File .\tools\Prepare-NitgenRuntime.ps1 -SdkBinPath 'C:\Program Files (x86)\NITGEN eNBSP\SDK\Bin'"
        exit 2
    }

    $SdkBinPath = $found
}

$SdkBinPath = (Resolve-Path $SdkBinPath).Path

if (-not (Test-NitgenBin $SdkBinPath)) {
    throw "A pasta informada nao contem NITGEN.SDK.NBioBSP.dll e NBioBSP.dll: $SdkBinPath"
}

Write-Step "Copiando DLLs do SDK"
Write-Host "Source: $SdkBinPath"
Get-ChildItem -Path $SdkBinPath -Filter *.dll -File | ForEach-Object {
    $dest = Join-Path $targetLib $_.Name
    Copy-Item $_.FullName $dest -Force
    Write-Host "Copiado: $($_.Name)"
}

if ($CopyToTesterBin) {
    if (-not (Test-Path $testerBin)) {
        New-Item -ItemType Directory -Path $testerBin | Out-Null
    }

    Write-Step "Copiando DLLs para bin do tester"
    Get-ChildItem -Path $targetLib -Filter *.dll -File | ForEach-Object {
        Copy-Item $_.FullName (Join-Path $testerBin $_.Name) -Force
        Write-Host "Tester bin: $($_.Name)"
    }
}

Write-Step "Validando arquivos obrigatorios"
$required = @("NITGEN.SDK.NBioBSP.dll", "NBioBSP.dll")
$missing = @()
foreach ($file in $required) {
    if (-not (Test-Path (Join-Path $targetLib $file))) {
        $missing += $file
    }
}

if ($missing.Count -gt 0) {
    throw "DLLs obrigatorias ausentes em lib\nitgen: $($missing -join ', ')"
}

Write-Host "OK. Agora compile o projeto src\FingerBridge.Providers.NitgenHamsterDx." -ForegroundColor Green
