@echo off
REM GLAT Quick Verification Script for Windows
REM This script verifies the project structure and readiness before deployment
REM Usage: verify.bat

setlocal enabledelayedexpansion

set "ERRORS=0"
set "WARNINGS=0"

echo.
echo ==========================================
echo GLAT Project Verification Script
echo ==========================================
echo.

REM Check required software
echo 1. Checking System Requirements...
echo ---

where docker >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] docker installed
) else (
    echo [WARN] docker not found
    set /a WARNINGS+=1
)

where docker-compose >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [OK] docker-compose installed
) else (
    echo [WARN] docker-compose not found
    set /a WARNINGS+=1
)

echo.

REM Check backend structure
echo 2. Checking Backend Structure (.NET 9.0)...
echo ---

if exist "src\backend\Lexart.GLAT.Api\" (
    echo [OK] src\backend\Lexart.GLAT.Api
) else (
    echo [ERROR] src\backend\Lexart.GLAT.Api not found
    set /a ERRORS+=1
)

if exist "src\backend\Lexart.GLAT.Api\Lexart.GLAT.Api.csproj" (
    echo [OK] .csproj file
) else (
    echo [ERROR] .csproj file not found
    set /a ERRORS+=1
)

if exist "src\backend\Lexart.GLAT.Api\Program.cs" (
    echo [OK] Program.cs
) else (
    echo [ERROR] Program.cs not found
    set /a ERRORS+=1
)

if exist "src\backend\Lexart.GLAT.Api\appsettings.json" (
    echo [OK] appsettings.json
) else (
    echo [ERROR] appsettings.json not found
    set /a ERRORS+=1
)

echo.

REM Check frontend structure
echo 3. Checking Frontend Structure (React 18+)...
echo ---

if exist "src\frontend\glat-web\" (
    echo [OK] src\frontend\glat-web
) else (
    echo [ERROR] src\frontend\glat-web not found
    set /a ERRORS+=1
)

if exist "src\frontend\glat-web\package.json" (
    echo [OK] package.json
) else (
    echo [ERROR] package.json not found
    set /a ERRORS+=1
)

if exist "src\frontend\glat-web\vite.config.js" (
    echo [OK] vite.config.js
) else (
    echo [ERROR] vite.config.js not found
    set /a ERRORS+=1
)

echo.

REM Check native C library
echo 4. Checking Native C Library...
echo ---

if exist "src\native\c\" (
    echo [OK] src\native\c
) else (
    echo [ERROR] src\native\c not found
    set /a ERRORS+=1
)

if exist "src\native\c\health_calculator.h" (
    echo [OK] health_calculator.h
) else (
    echo [ERROR] health_calculator.h not found
    set /a ERRORS+=1
)

if exist "src\native\c\health_calculator.c" (
    echo [OK] health_calculator.c
) else (
    echo [ERROR] health_calculator.c not found
    set /a ERRORS+=1
)

echo.

REM Check configuration files
echo 5. Checking Configuration Files...
echo ---

if exist "docker-compose.yml" (
    echo [OK] docker-compose.yml
) else (
    echo [ERROR] docker-compose.yml not found
    set /a ERRORS+=1
)

if exist "docker\Dockerfile" (
    echo [OK] docker\Dockerfile
) else (
    echo [ERROR] docker\Dockerfile not found
    set /a ERRORS+=1
)

if exist "docker\Dockerfile.frontend" (
    echo [OK] docker\Dockerfile.frontend
) else (
    echo [ERROR] docker\Dockerfile.frontend not found
    set /a ERRORS+=1
)

echo.

REM Check documentation
echo 6. Checking Documentation...
echo ---

if exist "README.md" (
    echo [OK] README.md
) else (
    echo [ERROR] README.md not found
    set /a ERRORS+=1
)

if exist "assets-api.yaml" (
    echo [OK] assets-api.yaml
) else (
    echo [ERROR] assets-api.yaml not found
    set /a ERRORS+=1
)

echo.

REM Summary
echo ==========================================
echo Verification Summary
echo ==========================================
echo Errors: !ERRORS!
echo Warnings: !WARNINGS!
echo.

if !ERRORS! EQU 0 (
    echo.
    echo [SUCCESS] Project structure is ready for deployment
    echo.
    echo Next steps:
    echo   1. Run: docker-compose up --build
    echo   2. Wait for all containers to be healthy
    echo   3. Access frontend at http://localhost:3000
    echo   4. Access API at http://localhost:5000/swagger
    echo   5. Login with credentials: demo / demo123
    exit /b 0
) else (
    echo.
    echo [FAILURE] Project has structural issues that must be fixed
    exit /b 1
)
