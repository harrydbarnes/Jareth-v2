# Jareth Sideload Installation Script
# This script installs the self-signed certificate and the MSIX package for testing.
param(
    [Parameter(Mandatory=$true)]
    [string]$MsixPath,
    
    [string]$CertPath
)

# Validate MSIX file exists
if (-not (Test-Path $MsixPath)) {
    Write-Error "MSIX file not found: $MsixPath"
    exit 1
}

# If no cert path specified, look for .cer file alongside the MSIX
if (-not $CertPath) {
    $certDir = Split-Path $MsixPath -Parent
    $CertPath = Get-ChildItem -Path $certDir -Filter "*.cer" | Select-Object -First 1 -ExpandProperty FullName
}

# Install certificate to Trusted People store (requires elevation)
if ($CertPath -and (Test-Path $CertPath)) {
    Write-Host "Installing certificate from $CertPath..."
    try {
        $cert = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2($CertPath)
        $store = New-Object System.Security.Cryptography.X509Certificates.X509Store("TrustedPeople", "LocalMachine")
        $store.Open("ReadWrite")
        $store.Add($cert)
        $store.Close()
        Write-Host "Certificate installed successfully."
    }
    catch {
        Write-Warning "Could not install certificate. You may need to run this script as Administrator."
        Write-Warning "Alternatively, enable Developer Mode in Windows Settings > Privacy & Security > For developers."
    }
}

# Install the MSIX package
Write-Host "Installing Jareth..."
try {
    Add-AppxPackage -Path $MsixPath
    Write-Host "Jareth installed successfully! You can find it in your Start menu."
}
catch {
    Write-Error "Installation failed: $_"
    Write-Host ""
    Write-Host "Troubleshooting:"
    Write-Host "1. Enable Developer Mode in Windows Settings"
    Write-Host "2. Or right-click the .msix file and select 'Install'"
    Write-Host "3. Or run this script as Administrator"
    exit 1
}
