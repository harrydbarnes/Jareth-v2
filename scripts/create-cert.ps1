# create-cert.ps1
# Generates a self-signed certificate for local development MSIX signing.
# Run this script once before building locally in Visual Studio.
# Usage: .\scripts\create-cert.ps1 [-Password "yourpassword"]

param(
    [string]$Password = "JarethDev123!"
)

$certPassword = ConvertTo-SecureString -String $Password -Force -AsPlainText

Write-Host "Creating self-signed certificate for Jareth local development..."

$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=JarethDev" `
    -KeyUsage DigitalSignature `
    -FriendlyName "Jareth Local Dev Cert" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

$pfxPath = Join-Path $PSScriptRoot "..\local-dev-cert.pfx"
Export-PfxCertificate -Cert $cert -FilePath $pfxPath -Password $certPassword

Write-Host ""
Write-Host "Certificate created: $pfxPath"
Write-Host "Thumbprint: $($cert.Thumbprint)"
Write-Host ""
Write-Host "Add to your Jareth.csproj or pass to msbuild:"
Write-Host "  /p:PackageCertificateKeyFile=local-dev-cert.pfx"
Write-Host "  /p:PackageCertificatePassword=$Password"
Write-Host ""
Write-Host "NOTE: local-dev-cert.pfx is in .gitignore and will NOT be committed."
