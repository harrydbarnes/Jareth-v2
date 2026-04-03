# Script to generate a self-signed certificate for local development MSIX signing
param(
    [string]$CertPassword = "JarethDev123!",
    [string]$OutputPath = ".\dev-cert.pfx"
)

$cert = New-SelfSignedCertificate `
    -Type Custom `
    -Subject "CN=JarethDev" `
    -KeyUsage DigitalSignature `
    -FriendlyName "Jareth Development Certificate" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

$securePassword = ConvertTo-SecureString -String $CertPassword -Force -AsPlainText
Export-PfxCertificate -Cert "Cert:\CurrentUser\My\$($cert.Thumbprint)" -FilePath $OutputPath -Password $securePassword

Write-Host "Certificate exported to $OutputPath"
Write-Host "Thumbprint: $($cert.Thumbprint)"
Write-Host "Remember to add the certificate to your Trusted People store for sideloading."
