#!/usr/bin/env bash

# GLAT Quick Verification Script
# This script verifies the project structure and readiness before deployment
# Usage: bash verify.sh

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

ERRORS=0
WARNINGS=0

echo "=========================================="
echo "GLAT Project Verification Script"
echo "=========================================="
echo ""

# Helper functions
check_file_exists() {
    if [ -f "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "${RED}✗${NC} $1 - NOT FOUND"
        ERRORS=$((ERRORS + 1))
        return 1
    fi
}

check_dir_exists() {
    if [ -d "$1" ]; then
        echo -e "${GREEN}✓${NC} $1"
        return 0
    else
        echo -e "${RED}✗${NC} $1 - NOT FOUND"
        ERRORS=$((ERRORS + 1))
        return 1
    fi
}

check_command() {
    if command -v "$1" &> /dev/null; then
        echo -e "${GREEN}✓${NC} $1 installed"
        return 0
    else
        echo -e "${YELLOW}⚠${NC} $1 not found"
        WARNINGS=$((WARNINGS + 1))
        return 1
    fi
}

# Check required software
echo "1. Checking System Requirements..."
echo "---"
check_command docker
check_command docker-compose
echo ""

# Check backend structure
echo "2. Checking Backend Structure (.NET 9.0)..."
echo "---"
check_dir_exists "src/backend/Lexart.GLAT.Api"
check_file_exists "src/backend/Lexart.GLAT.Api/Lexart.GLAT.Api.csproj"
check_file_exists "src/backend/Lexart.GLAT.Api/Program.cs"
check_file_exists "src/backend/Lexart.GLAT.Api/appsettings.json"
check_dir_exists "src/backend/Lexart.GLAT.Api/Domain/Entities"
check_dir_exists "src/backend/Lexart.GLAT.Api/Application"
check_dir_exists "src/backend/Lexart.GLAT.Api/Infrastructure"
check_dir_exists "src/backend/Lexart.GLAT.Api/Presentation/Controllers"
echo ""

# Check frontend structure
echo "3. Checking Frontend Structure (React 18+)..."
echo "---"
check_dir_exists "src/frontend/glat-web"
check_file_exists "src/frontend/glat-web/package.json"
check_file_exists "src/frontend/glat-web/vite.config.js"
check_file_exists "src/frontend/glat-web/tailwind.config.js"
check_file_exists "src/frontend/glat-web/index.html"
check_dir_exists "src/frontend/glat-web/src"
echo ""

# Check native C library
echo "4. Checking Native C Library..."
echo "---"
check_dir_exists "src/native/c"
check_file_exists "src/native/c/health_calculator.h"
check_file_exists "src/native/c/health_calculator.c"
check_file_exists "src/native/c/Makefile"
echo ""

# Check configuration files
echo "5. Checking Configuration Files..."
echo "---"
check_file_exists "docker-compose.yml"
check_file_exists "docker/Dockerfile"
check_file_exists "docker/Dockerfile.frontend"
check_file_exists ".dockerignore"
check_file_exists ".gitignore"
check_file_exists ".env.example"
echo ""

# Check documentation
echo "6. Checking Documentation..."
echo "---"
check_file_exists "README.md"
check_file_exists "IMPLEMENTATION_NOTES.md"
check_file_exists "DEPLOYMENT_CHECKLIST.md"
check_file_exists "CHANGELOG.md"
check_file_exists "AI_STRATEGY.md"
check_file_exists "Makefile"
echo ""

# Check API specification
echo "7. Checking API Specification..."
echo "---"
check_file_exists "assets-api.yaml"
echo ""

# Validate YAML syntax (if yq/jq available)
echo "8. Checking File Formats..."
echo "---"
if check_command python3; then
    if python3 -c "import yaml" 2>/dev/null; then
        if python3 -c "import yaml; yaml.safe_load(open('assets-api.yaml'))" 2>/dev/null; then
            echo -e "${GREEN}✓${NC} assets-api.yaml is valid YAML"
        else
            echo -e "${RED}✗${NC} assets-api.yaml has YAML errors"
            ERRORS=$((ERRORS + 1))
        fi
    fi
fi

if check_command node; then
    if node -c "src/frontend/glat-web/vite.config.js" 2>/dev/null; then
        echo -e "${GREEN}✓${NC} vite.config.js syntax is valid"
    else
        echo -e "${YELLOW}⚠${NC} Could not validate JavaScript syntax"
        WARNINGS=$((WARNINGS + 1))
    fi
fi
echo ""

# Check port availability (on Linux/macOS)
echo "9. Checking Port Availability..."
echo "---"
if command -v lsof &> /dev/null; then
    if lsof -Pi :3000 -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${YELLOW}⚠${NC} Port 3000 is already in use"
        WARNINGS=$((WARNINGS + 1))
    else
        echo -e "${GREEN}✓${NC} Port 3000 is available"
    fi
    
    if lsof -Pi :5000 -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${YELLOW}⚠${NC} Port 5000 is already in use"
        WARNINGS=$((WARNINGS + 1))
    else
        echo -e "${GREEN}✓${NC} Port 5000 is available"
    fi
    
    if lsof -Pi :1433 -sTCP:LISTEN -t >/dev/null 2>&1; then
        echo -e "${YELLOW}⚠${NC} Port 1433 is already in use"
        WARNINGS=$((WARNINGS + 1))
    else
        echo -e "${GREEN}✓${NC} Port 1433 is available"
    fi
fi
echo ""

# Summary
echo "=========================================="
echo "Verification Summary"
echo "=========================================="
echo -e "Errors: ${RED}$ERRORS${NC}"
echo -e "Warnings: ${YELLOW}$WARNINGS${NC}"
echo ""

if [ $ERRORS -eq 0 ]; then
    echo -e "${GREEN}✓ Project structure is ready for deployment${NC}"
    echo ""
    echo "Next steps:"
    echo "  1. Run: docker-compose up --build"
    echo "  2. Wait for all containers to be healthy"
    echo "  3. Access frontend at http://localhost:3000"
    echo "  4. Access API at http://localhost:5000/swagger"
    echo "  5. Login with credentials: demo / demo123"
    exit 0
else
    echo -e "${RED}✗ Project has structural issues that must be fixed${NC}"
    echo ""
    echo "Please resolve the above errors before proceeding."
    exit 1
fi
