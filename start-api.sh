#!/bin/bash

# Northwnd API Startup Script
# This script builds and starts the Northwnd API with Swagger UI

set -e

PROJECT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
API_PROJECT="$PROJECT_DIR/Test.API/Northwnd.API.csproj"

echo "================================"
echo "Northwnd API - Startup Script"
echo "================================"
echo ""

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "❌ Error: .NET SDK is not installed or not in PATH"
    echo "Please install .NET 6.0 SDK from: https://dotnet.microsoft.com/download/dotnet/6.0"
    exit 1
fi

DOTNET_VERSION=$(dotnet --version)
echo "✓ .NET SDK found: $DOTNET_VERSION"
echo ""

# Restore dependencies
echo "📦 Restoring dependencies..."
dotnet restore "$PROJECT_DIR/Northwnd.sln" --quiet
echo "✓ Dependencies restored"
echo ""

# Build solution
echo "🔨 Building solution..."
dotnet build "$PROJECT_DIR/Northwnd.sln" --configuration Release --quiet
echo "✓ Build completed"
echo ""

# Start API with Swagger
echo "🚀 Starting Northwnd API..."
echo ""
echo "╔════════════════════════════════════════════════════════════╗"
echo "║ Swagger UI will be available at:                           ║"
echo "║ https://localhost:7123                                     ║"
echo "║ or                                                         ║"
echo "║ http://localhost:5123                                      ║"
echo "║                                                            ║"
echo "║ API Documentation: https://localhost:7123/swagger          ║"
echo "║ Swagger JSON: https://localhost:7123/swagger/v1/swagger.json║"
echo "║                                                            ║"
echo "║ Press Ctrl+C to stop the server                           ║"
echo "╚════════════════════════════════════════════════════════════╝"
echo ""

dotnet run --project "$API_PROJECT" --configuration Release
