param([string]$Executable = '', [string]$OutputDirectory = '', [switch]$WindowQa, [switch]$ContractsOnly)
$ErrorActionPreference = 'Stop'
if (-not $Executable) { $Executable = Join-Path $PSScriptRoot 'bin\yanan-1.0.0.exe' }
$exe = (Resolve-Path -LiteralPath $Executable).Path
if (-not $OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'bin\qa' }
$qa = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Force -Path $qa | Out-Null
function Invoke-Check([string[]]$Arguments) {
    $quoted = @($Arguments | ForEach-Object { '"' + $_ + '"' })
    $process = Start-Process -FilePath $exe -ArgumentList $quoted -WindowStyle Hidden -PassThru
    if (-not $process.WaitForExit(120000)) { $process.Kill(); throw 'QA timed out after 120 seconds' }
    if ($process.ExitCode -ne 0) { throw "QA failed with exit code $($process.ExitCode): $($Arguments -join ' ')" }
}
Invoke-Check @('--contract-test', (Join-Path $qa 'contract-tests.json'))
if ($ContractsOnly) { return }
Invoke-Check @('--self-test', (Join-Path $qa 'self-test.json'), (Join-Path $qa 'self-test-preview.png'))
$report = Get-Content -Raw -LiteralPath (Join-Path $qa 'self-test.json') | ConvertFrom-Json
if (-not $report.ok -or $report.errors.Count -ne 0) { throw 'Resource self-test did not pass' }
if ($WindowQa) { Invoke-Check @('--qa-window', '10000') }
@{ ok=$true; scope='automated-only'; windowQa=[bool]$WindowQa; manualAcceptance=$false } | ConvertTo-Json | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $qa 'verification.json')
