@echo off
REM Northwnd API Startup Script for Windows
REM This script builds and starts the Northwnd API with Swagger UI

setlocal enabledelayedexpansion

set PROJECT_DIR=%CD%
set API_PROJECT=%PROJECT_DIR%\Test.API\Northwnd.API.csproj

echo =================================
echo Northwnd API - Startup Script
echo =================================
echo.

REM Check if .NET is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo [X] Error: .NET SDK is not installed or not in PATH
    echo Please install .NET 6.0 SDK from: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
echo [OK] .NET SDK found: %DOTNET_VERSION%
echo.

REM Restore dependencies
echo [*] Restoring dependencies...
dotnet restore "%PROJECT_DIR%\Northwnd.sln" --quiet
echo [OK] Dependencies restored
echo.

REM Build solution
echo [*] Building solution...
dotnet build "%PROJECT_DIR%\Northwnd.sln" --configuration Release --quiet
if %ERRORLEVEL% NEQ 0 (
    echo [X] Build failed
    pause
    exit /b 1
)
echo [OK] Build completed
echo.

REM Start API with Swagger
echo [*] Starting Northwnd API...
echo.
echo ============================================================
echo Swagger UI will be available at:
echo   https://localhost:7123
echo   or
echo   http://localhost:5123
echo.
echo API Documentation: https://localhost:7123/swagger
echo Swagger JSON: https://localhost:7123/swagger/v1/swagger.json
echo.
echo Press Ctrl+C to stop the server
echo ============================================================
echo.

dotnet run --project "%API_PROJECT%" --configuration Release
pause
