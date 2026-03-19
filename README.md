# Jareth

> **Jareth** is a native Windows 11 meeting recorder and AI summariser. Record system audio and microphone, get transcripts, and generate AI summaries — all stored locally.

---

## Features

- 🎙️ **Audio recording** — Mic only, system audio only, or both mixed
- 📁 **Local storage** — All data saved to `Documents/Jareth/Meetings/`
- 🗂️ **Folder/category system** — Organise meetings into Work, Personal, etc.
- 🤖 **AI summaries** *(coming in Prompt 2)* — OpenAI, Anthropic, Gemini, or local models
- 📝 **Transcription** *(coming in Prompt 2)* — Whisper local or cloud
- 🔒 **Encrypted API keys** — DPAPI, tied to your Windows user account
- 🎨 **WinUI 3 native UI** — Mica backdrop, rounded cards, accent colour `#8ACE00`

---

## Tech Stack

| Layer | Choice |
|---|---|
| Language | C# 12 / .NET 8 |
| UI | WinUI 3 (Windows App SDK 1.5+) |
| Audio | NAudio 2.x (WASAPI loopback + WaveIn) |
| Database | SQLite via Microsoft.Data.Sqlite |
| Settings | JSON via System.Text.Json |
| MVVM | CommunityToolkit.Mvvm |
| DI | Microsoft.Extensions.DependencyInjection |

---

## Project Structure

```
Jareth/
  .github/
    workflows/
      pr-build.yml          # Builds on every PR, uploads MSIX artefact
      release.yml           # Builds on version tag, attaches to GitHub Release
  src/
    Jareth/                 # WinUI 3 packaged app (MSIX)
    Jareth.Core/            # Class library — models, services, helpers
    Jareth.Tests/           # xUnit test project
  assets/
    icons/
    fonts/
  scripts/
    create-cert.ps1         # Generate a local dev self-signed certificate
  Install.ps1               # Sideload installer helper for testers
  Jareth.sln
  README.md
```

---

## Getting Started (Development)

### Prerequisites

- Windows 11 (or Windows 10 1903+)
- Visual Studio 2022 17.8+ with:
  - **.NET desktop development** workload
  - **Windows App SDK** extension / workload
- .NET 8 SDK

### First-time Setup

1. Clone the repository:
   ```
   git clone https://github.com/harrydbarnes/Jareth-v2.git
   ```

2. Open `Jareth.sln` in Visual Studio 2022.

3. Generate a local development certificate:
   ```powershell
   .\scripts\create-cert.ps1
   ```

4. Set `Jareth` as the startup project and press **F5**.

---

## Running Tests

```bash
dotnet test src/Jareth.Tests --configuration Release --verbosity normal
```

---

## CI Setup

### Required GitHub Actions Secrets

| Secret | Description |
|---|---|
| `CERT_PASSWORD` | Password for the self-signed build certificate used to sign the MSIX |
| `PROD_CERT_PFX` | *(Optional)* Base64-encoded production `.pfx` for release signing |

### Setting Secrets

1. Go to **Settings → Secrets and variables → Actions** in your GitHub repository.
2. Click **New repository secret**.
3. Add `CERT_PASSWORD` with a strong password of your choice.

### PR Build Workflow

Every pull request targeting `main` or `develop` triggers `pr-build.yml`:

1. Restores NuGet packages
2. Builds the solution in Release
3. Runs unit tests
4. Generates a self-signed certificate
5. Builds the MSIX installer with MSBuild
6. Uploads the MSIX as a build artefact (retained 7 days)
7. Posts a comment on the PR with the result

### Release Workflow

Pushing a version tag (e.g. `v1.0.0`) triggers `release.yml`:

1. Validates that the tag matches `<Version>` in `Jareth.csproj`
2. Builds and tests as above
3. Signs with `PROD_CERT_PFX` if available, otherwise a self-signed cert
4. Creates a GitHub Release with auto-generated notes
5. Attaches the MSIX to the release

---

## Installing a Sideloaded Build

After downloading the MSIX artefact from GitHub Actions:

### Option A: Install.ps1 (recommended for testers)

```powershell
# From the directory containing the .msix file:
.\Install.ps1
# or specify the path:
.\Install.ps1 -MsixPath ".\Jareth.msix"
```

This script automatically trusts the self-signed certificate and installs the package.

### Option B: Manual

1. Enable **Developer Mode**: Settings → Privacy & Security → For Developers
2. Right-click the `.msix` file
3. Select **Install**

### Option C: PowerShell

```powershell
# Trust the certificate first (run as Administrator)
$cert = Get-PfxCertificate ".\build-cert.cer"
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("TrustedPeople","LocalMachine")
$store.Open("ReadWrite")
$store.Add($cert)
$store.Close()

# Install the package
Add-AppxPackage ".\Jareth_1.0.0_x64.msix"
```

---

## Audio Permissions

Jareth uses `WasapiLoopbackCapture` for system audio capture (no virtual cable required). The app manifest declares the `microphone` and `backgroundMediaRecording` capabilities.

On first launch, Windows may prompt you to grant microphone access. Accept this for full recording functionality.

---

## Branch Strategy

| Branch | Purpose |
|---|---|
| `main` | Stable, protected |
| `develop` | Integration branch |
| `feature/*` | Feature development |

PRs should target `develop` or `main`.

---

## Versioning

Version is defined in `src/Jareth/Jareth.csproj`:

```xml
<Version>1.0.0</Version>
```

When releasing, tag must match:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The release workflow validates this match and fails if they differ.

---

## Licence

MIT — see `LICENSE` for details.
