param(
    [string]$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

$ErrorActionPreference = "Stop"

Write-Host "FingerBridge - Git safety check"
Write-Host "RepoRoot: $RepoRoot"

$blockedPatterns = @(
    "*.dll",
    "*.exe",
    "*.msi",
    "*.rar",
    "*.zip",
    "*.7z",
    "*SERIAL*",
    "*Serial*",
    "*serial*",
    "*.lic"
)

$allowed = @(
    "lib\futronic\README.md",
    "lib\futronic\.gitkeep",
    "lib\nitgen\README.md",
    "lib\nitgen\.gitkeep"
)

$findings = New-Object System.Collections.Generic.List[string]

foreach ($pattern in $blockedPatterns) {
    Get-ChildItem -Path $RepoRoot -Recurse -Force -File -Filter $pattern |
        Where-Object {
            $relative = $_.FullName.Substring($RepoRoot.Length).TrimStart('\', '/')
            $relativeNormalized = $relative -replace '/', '\'
            -not ($allowed -contains $relativeNormalized)
        } |
        ForEach-Object {
            $findings.Add($_.FullName.Substring($RepoRoot.Length).TrimStart('\', '/'))
        }
}

if ($findings.Count -gt 0) {
    Write-Host "Arquivos bloqueados encontrados:" -ForegroundColor Red
    $findings | Sort-Object -Unique | ForEach-Object { Write-Host "- $_" -ForegroundColor Red }
    throw "Remova arquivos proprietarios/binarios/licencas antes de publicar no Git."
}

Write-Host "OK: nenhum arquivo proprietario/binario/licenca bloqueado foi encontrado." -ForegroundColor Green
