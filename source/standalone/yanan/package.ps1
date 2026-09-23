param([string]$Executable = '', [string]$OutputDirectory = '')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
if (-not $Executable) { $Executable = Join-Path $PSScriptRoot 'bin/yanan-1.0.0.exe' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $root 'release-assets' }
$output = [IO.Path]::GetFullPath($OutputDirectory)
$bundle = Join-Path $output 'yanan-1.0.0-preview'
New-Item -ItemType Directory -Force -Path $bundle | Out-Null
Copy-Item -LiteralPath $Executable -Destination (Join-Path $bundle 'yanan-1.0.0.exe') -Force
Copy-Item -LiteralPath (Join-Path $root 'docs/USER_GUIDE.md'), (Join-Path $root 'docs/RIGHTS.md'), (Join-Path $root 'docs/QA_REPORT.md'), (Join-Path $root 'RELEASE_NOTES.md') -Destination $bundle -Force
$exeHash = Get-FileHash -Algorithm SHA256 -LiteralPath (Join-Path $bundle 'yanan-1.0.0.exe')
('{0}  yanan-1.0.0.exe' -f $exeHash.Hash.ToLowerInvariant()) | Set-Content -LiteralPath (Join-Path $bundle 'SHA256SUMS.txt') -Encoding ASCII
$zip = Join-Path $output 'yanan-1.0.0-preview.zip'
Compress-Archive -Path (Join-Path $bundle '*') -DestinationPath $zip -Force
$exe = Join-Path $output 'yanan-1.0.0.exe'
Copy-Item -LiteralPath (Join-Path $bundle 'yanan-1.0.0.exe') -Destination $exe -Force
@($exe, $zip) | ForEach-Object {
    $hash = Get-FileHash -Algorithm SHA256 -LiteralPath $_
    '{0}  {1}' -f $hash.Hash.ToLowerInvariant(), [IO.Path]::GetFileName($_)
} | Set-Content -LiteralPath (Join-Path $output 'SHA256SUMS.txt') -Encoding ASCII
Get-ChildItem -LiteralPath $output -File | Select-Object Name, Length
