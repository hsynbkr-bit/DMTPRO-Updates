<#
.SYNOPSIS
  Refuse to publish Windows executables without valid, timestamped Authenticode signatures.
.DESCRIPTION
  Run after signing. Call with exact certificate Subject from your validated organization's
  signing certificate. Never use a self-signed certificate for public distribution.
  This script does not sign files, accept a certificate, or bypass Windows protection.
.EXAMPLE
  .\scripts\assert-authenticode.ps1 -ExpectedPublisherSubject "CN=Registered Company Name, O=Registered Company Name, C=TR" -Paths ".\out\DMTPROLauncher.exe",".\out\DMTPRO.exe",".\out\DMTPRO_Setup.exe"
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory=$true)][string]$ExpectedPublisherSubject,
  [Parameter(Mandatory=$true)][ValidateCount(1,50)][string[]]$Paths
)
$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($ExpectedPublisherSubject)) {
  throw 'Publisher subject must be the exact CA-validated certificate Subject.'
}
$failed = @()
foreach ($p in $Paths) {
  if (!(Test-Path -LiteralPath $p -PathType Leaf)) {
    $failed += "Missing file: $p"
    continue
  }
  $sig = Get-AuthenticodeSignature -FilePath $p
  $publisher = if ($sig.SignerCertificate) { $sig.SignerCertificate.Subject } else { '<none>' }
  $timestamped = ($null -ne $sig.TimeStamperCertificate)
  $status = [string]$sig.Status
  Write-Host "$p -> signature=$status; publisher=$publisher; timestamp=$timestamped"
  if ($status -ne 'Valid') { $failed += "Signature invalid or missing: $p ($status)" }
  if ($publisher -cne $ExpectedPublisherSubject) { $failed += "Publisher is not the CA-validated intended identity: $p" }
  if (!$timestamped) { $failed += "Missing trusted timestamp: $p" }
}
if ($failed.Count -gt 0) {
  $failed | ForEach-Object { Write-Error $_ -ErrorAction Continue }
  throw 'DMTPRO release must be blocked until signed PE identities and timestamps validate.'
}
Write-Host "SIGNATURE_GATE_PASS: $($Paths.Count) files with valid intended publisher and timestamp."
