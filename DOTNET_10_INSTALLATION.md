# .NET 10 Installation Guide

## ⚠️ .NET SDK Not Found

Your system doesn't have .NET SDK installed. Here's how to install .NET 10.0:

---

## macOS Installation

### Option 1: Using Homebrew (Recommended)
```bash
# Install Homebrew if not already installed
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

# Install .NET 10.0
brew install dotnet

# Verify installation
dotnet --version
```

### Option 2: Direct Download
1. Visit: https://dotnet.microsoft.com/download/dotnet/10.0
2. Download: macOS x64 or ARM64 (Apple Silicon)
3. Run the installer
4. Verify:
```bash
dotnet --version
```

### Option 3: Using .NET Installer Script
```bash
curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --channel 10.0
```

---

## Windows Installation

### Option 1: Direct Download (Recommended)
1. Visit: https://dotnet.microsoft.com/download/dotnet/10.0
2. Download: Windows x64 installer
3. Run the installer
4. Verify:
```cmd
dotnet --version
```

### Option 2: Using Chocolatey
```powershell
choco install dotnet-sdk-10.0
```

### Option 3: Using scoop
```powershell
scoop bucket add versions
scoop install dotnet-sdk-10.0
```

---

## Linux Installation (Ubuntu/Debian)

```bash
# Add Microsoft package repository
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# Install .NET 10.0
sudo apt-get update
sudo apt-get install -y dotnet-sdk-10.0

# Verify installation
dotnet --version
```

---

## After Installation

Once .NET 10.0 is installed, verify it:

```bash
# Check version
dotnet --version

# List installed SDKs
dotnet --list-sdks

# List installed runtimes
dotnet --list-runtimes
```

Expected output:
```
10.0.0 (or higher)
```

---

## Next Steps

After installing .NET 10.0, you can:

### 1. Build the Solution
```bash
cd /Users/lashadokvadze/Desktop/Northwnd
dotnet build Northwnd.sln --configuration Release
```

### 2. Run Unit Tests
```bash
dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj
```

### 3. Build Only API Project
```bash
dotnet build Test.API/Northwnd.API.csproj
```

### 4. Start the API with Swagger
```bash
./start-api.sh  # macOS/Linux
start-api.bat   # Windows
```

Or manually:
```bash
dotnet run --project Test.API/Northwnd.API.csproj
```

---

## Troubleshooting

### Issue: "dotnet command not found" after installation

**macOS:**
```bash
# Add to ~/.zshrc or ~/.bash_profile
export PATH=$PATH:/usr/local/share/dotnet
source ~/.zshrc
```

**Windows:** Restart your terminal or IDE

**Linux:**
```bash
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc
```

### Issue: Multiple .NET versions installed

Check which .NET versions are installed:
```bash
dotnet --list-sdks
```

Set specific version for a project via `global.json`:
```json
{
  "sdk": {
    "version": "10.0.0"
  }
}
```

### Issue: Port already in use when starting API

```bash
# Use a different port
dotnet run --project Test.API/Northwnd.API.csproj -- --urls="https://localhost:7124"
```

### Issue: SSL Certificate error

```bash
# Trust .NET development certificate
dotnet dev-certs https --trust
```

---

## Project Requirements

The Northwnd project now requires:
- **Runtime**: .NET 10.0 or higher
- **SDK**: .NET 10.0 SDK

### Updated Dependencies (All .NET 10 Compatible)
- Microsoft.EntityFrameworkCore 8.0.0
- Microsoft.EntityFrameworkCore.SqlServer 8.0.0
- Swashbuckle.AspNetCore 6.6.2
- xunit 2.6.6
- xunit.runner.visualstudio 2.5.6
- Microsoft.NET.Test.Sdk 17.9.0
- Moq 4.20.70

---

## Verification Checklist

After installation, ensure:

✅ `dotnet --version` returns 10.0.0 or higher  
✅ `dotnet --list-sdks` shows .NET 10.0 installed  
✅ Can run: `dotnet restore Northwnd.sln`  
✅ Can run: `dotnet build Northwnd.sln --configuration Release`  
✅ Can run: `dotnet test Northwnd.UnitTest/Northwnd.UnitTest.csproj`  
✅ Can start API: `dotnet run --project Test.API/Northwnd.API.csproj`  

---

## Support

- [Official .NET Download](https://dotnet.microsoft.com/download/dotnet)
- [.NET Documentation](https://docs.microsoft.com/dotnet)
- [.NET Troubleshooting](https://docs.microsoft.com/en-us/dotnet/core/troubleshoot)

---

**Status**: Ready to install .NET 10.0  
**Last Updated**: April 2026

Once you've installed .NET 10.0, run these commands:
```bash
cd /Users/lashadokvadze/Desktop/Northwnd
dotnet build Northwnd.sln --configuration Release
./start-api.sh
```

Then open: **https://localhost:7123**
