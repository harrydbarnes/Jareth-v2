# Install.ps1
# Jareth sideload installer helper.
# Installs the self-signed certificate (if needed) and then installs the MSIX package.
# Run as Administrator, or with Developer Mode enabled.
#
# Usage:
#   .\Install.ps1
#   .\Install.ps1 -MsixPath ".\path\to\Jareth.msix"

param(
    [string]$MsixPath = ""
)

$ErrorActionPreference = "Stop"

# Find the MSIX if not specified
if ([string]::IsNullOrEmpty($MsixPath)) {
    $candidates = Get-ChildItem -Recurse -Filter "*.msix" -ErrorAction SilentlyContinue |
                  Sort-Object LastWriteTime -Descending |
                  Select-Object -First 1
    if ($null -eq $candidates) {
        Write-Error "No .msix file found. Specify -MsixPath or place the .msix in the same directory."
        exit 1
    }
    $MsixPath = $candidates.FullName
}

Write-Host "Installing Jareth from: $MsixPath"
Write-Host ""

# Check for a .cer or .pfx next to the MSIX and trust it
$certDir = Split-Path $MsixPath
$certs = Get-ChildItem -Path $certDir -Filter "*.cer" -ErrorAction SilentlyContinue

foreach ($cert in $certs) {
    Write-Host "Trusting certificate: $($cert.Name)"
    try {
        $certObj = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($cert.FullName)
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("TrustedPeople", "LocalMachine")
        $store.Open("ReadWrite")
        $store.Add($certObj)
        $store.Close()
        Write-Host "Certificate trusted successfully."
    }
    catch {
        Write-Warning "Could not trust certificate automatically. You may need to run as Administrator."
        Write-Warning "Or enable Developer Mode in Windows Settings > Privacy & Security > For Developers."
    }
}

# Install the MSIX
Write-Host ""
Write-Host "Installing MSIX package..."
try {
    Add-AppxPackage -Path $MsixPath -ForceApplicationShutdown
    Write-Host ""
    Write-Host "Jareth installed successfully! Launch it from the Start menu."
}
catch {
    Write-Error "Installation failed: $_"
    Write-Host ""
    Write-Host "Alternatives:"
    Write-Host "  1. Enable Developer Mode: Settings > Privacy & Security > For Developers"
    Write-Host "  2. Right-click the .msix file and select 'Install'"
    Write-Host "  3. Run this script as Administrator"
    exit 1
}
