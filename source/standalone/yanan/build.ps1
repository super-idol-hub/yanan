param(
    [string]$FrameArchive = '',
    [string]$OutputDirectory = '',
    [switch]$SourceOnly
)
$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (-not (Test-Path -LiteralPath $compiler)) { throw 'The Windows .NET Framework compiler is required.' }
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'bin' }
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$name = if ($SourceOnly) { 'yanan-source-check.exe' } else { 'yanan-1.0.0.exe' }
$output = Join-Path ([IO.Path]::GetFullPath($OutputDirectory)) $name
$arguments = @('/nologo', '/target:winexe', '/optimize+', '/debug-', '/platform:anycpu', '/langversion:5', '/codepage:65001', '/warnaserror',
    '/reference:System.dll', '/reference:System.Core.dll', '/reference:System.Drawing.dll', '/reference:System.IO.Compression.dll', '/reference:System.Windows.Forms.dll',
    "/win32manifest:$PSScriptRoot\app.manifest", "/out:$output")
if (-not $SourceOnly) {
    if (-not $FrameArchive) { $FrameArchive = Join-Path $PSScriptRoot 'resources\noir-frames.zip' }
    $archive = (Resolve-Path -LiteralPath $FrameArchive).Path
    $arguments += "/resource:$archive,Yanan.Standalone.Frames.zip"
}
if (Test-Path -LiteralPath "$PSScriptRoot\yanan.ico") { $arguments += "/win32icon:$PSScriptRoot\yanan.ico" }
$arguments += "$PSScriptRoot\Program.cs", "$PSScriptRoot\ContractTests.cs", "$PSScriptRoot\InteractionQa.cs"
& $compiler $arguments
if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $LASTEXITCODE" }
Get-Item -LiteralPath $output | Select-Object Name, Length
