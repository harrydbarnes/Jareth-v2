# Jareth

**Jareth** is a Windows desktop meeting assistant built with WinUI 3 and .NET 8. It records meetings, generates transcripts, and produces AI-powered summaries — all stored locally on your machine.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI | WinUI 3 (Windows App SDK 1.5) |
| Framework | .NET 8 |
| Language | C# 12 |
| Architecture | MVVM (CommunityToolkit.Mvvm) |
| Audio | NAudio 2.2 |
| Database | Microsoft.Data.Sqlite |
| Packaging | MSIX sideload |
| CI/CD | GitHub Actions |
| Tests | xUnit 2.7 |

## Prerequisites

- **Windows 10** version 1904 or later (Windows 11 recommended)
- **Visual Studio 2022** 17.8+ with the following workloads:
  - .NET Desktop Development
  - Windows App SDK (C#)
- **.NET 8 SDK**
- **Developer Mode** enabled in Windows Settings → Privacy & Security → For Developers

## Getting Started

### Clone the repository

```bash
git clone https://github.com/<your-org>/Jareth-v2.git
cd Jareth-v2
```

### Build

```bash
dotnet restore Jareth.sln
dotnet build Jareth.sln --configuration Debug
```

Or open `Jareth.sln` in Visual Studio and build from there.

### Run

Set **Jareth** as the startup project in Visual Studio and press **F5**, or run:

```bash
dotnet run --project src/Jareth/Jareth.csproj
```

### Run Tests

```bash
dotnet test src/Jareth.Tests/Jareth.Tests.csproj --verbosity normal
```

## Project Structure

```
Jareth-v2/
├── Jareth.sln                  # Solution file
├── .github/workflows/
│   ├── pr-build.yml            # PR build + MSIX artifact
│   └── release.yml             # Tag-triggered release
├── assets/
│   ├── fonts/                  # Custom fonts
│   └── icons/                  # App icons
├── scripts/
│   ├── Install.ps1             # Sideload installation helper
│   └── create-cert.ps1         # Dev certificate generator
└── src/
    ├── Jareth/                 # WinUI 3 app (views, view-models, XAML)
    ├── Jareth.Core/            # Core library (models, services, helpers)
    └── Jareth.Tests/           # xUnit unit tests
```

## CI/CD Setup

Two GitHub Actions workflows are included:

### PR Build (`.github/workflows/pr-build.yml`)

Runs on every pull request to `main` or `develop`. Builds the solution, runs tests, produces a signed MSIX artifact, and posts a status comment on the PR.

### Release (`.github/workflows/release.yml`)

Triggered by pushing a tag matching `v*.*.*`. Validates the tag against the project version, builds, tests, creates a signed MSIX, and publishes a GitHub Release.

### Required Secrets

| Secret | Description |
|--------|-------------|
| `CERT_PASSWORD` | Password for the code-signing certificate (optional — falls back to a CI default) |
| `PROD_CERT_PFX` | Base64-encoded `.pfx` certificate for production releases (optional — a self-signed cert is generated if absent) |

## Installing a Sideloaded Build

1. **Enable Developer Mode** on the target machine (Settings → Privacy & Security → For Developers).
2. Download the MSIX artifact from the GitHub Actions run or release.
3. Run the installation script:

```powershell
.\scripts\Install.ps1 -MsixPath .\path\to\Jareth.msix
```

The script installs the bundled certificate into the Trusted People store and then installs the MSIX package. Administrator elevation is required for the certificate step.

Alternatively, double-click the `.msix` file directly if the signing certificate is already trusted.

## Portable / Unpackaged Execution

If you cannot install software (e.g., on a managed work laptop without administrator rights), you can run Jareth directly from the build output folder — no MSIX, no certificate, no elevated permissions required.

### Build unpackaged

```powershell
dotnet build src/Jareth/Jareth.csproj `
    --configuration Unpackaged `
    -p:RuntimeIdentifier=win-x64 `
    --self-contained true
```

The output is written to `src/Jareth/bin/Unpackaged/net8.0-windows10.0.19041.0/win-x64/`. Run `Jareth.exe` directly from that folder.

### What the Unpackaged configuration does

| Property | Value | Effect |
|----------|-------|--------|
| `WindowsPackageType` | `None` | No MSIX packaging; app runs as a plain `.exe` |
| `EnableMsixTooling` | `false` | Disables MSIX-specific MSBuild targets |
| `WindowsAppSDKSelfContained` | `true` | Windows App SDK binaries are copied next to the `.exe` |

> **Note:** The unpackaged build does not have an identity and therefore cannot use packaged-only Windows APIs (e.g., `Windows.Storage.ApplicationData`). Jareth stores all data in `%LOCALAPPDATA%\Jareth` in both packaged and unpackaged modes, so functionality is identical.

### Install the certificate without admin rights (MSIX sideload only)

If you do want to use the MSIX installer on a machine where you have no admin rights but _do_ have Developer Mode enabled, `scripts/Install.ps1` now writes the self-signed certificate to `CurrentUser\TrustedPeople` instead of `LocalMachine\TrustedPeople`, which requires no elevation:

```powershell
.\scripts\Install.ps1 -MsixPath .\Jareth.msix
```

## Audio Permissions

Jareth captures audio from your microphone and/or system audio. On first launch, Windows will prompt for microphone access — you must allow it for recording to work. System audio capture (loopback) does not require additional permissions.

## Contributing

1. Fork the repository.
2. Create a feature branch from `develop`: `git checkout -b feature/my-change develop`
3. Make your changes and add tests.
4. Ensure all tests pass: `dotnet test src/Jareth.Tests/Jareth.Tests.csproj`
5. Open a pull request against `develop`.

## License

This project is licensed under the [MIT License](LICENSE).
