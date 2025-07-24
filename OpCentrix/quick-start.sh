#!/bin/bash
# OpCentrix Quick Start Script for Linux/Mac
# This script tests and starts your OpCentrix manufacturing scheduler

echo "========================================"
echo "   OpCentrix SLS Manufacturing Scheduler"
echo "========================================"
echo

echo "[1/4] Checking .NET 8 installation..."
if ! command -v dotnet &> /dev/null; then
    echo "? .NET 8 SDK not found!"
    echo "Please install .NET 8 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi
echo "? .NET 8 SDK found"

echo
echo "[2/4] Building application..."
if ! dotnet build --verbosity quiet --nologo; then
    echo "? Build failed! Check for compilation errors."
    exit 1
fi
echo "? Build successful"

echo
echo "[3/4] Checking core files..."
MISSING_FILES=""
if [ ! -f "Pages/Scheduler/Index.cshtml" ]; then
    MISSING_FILES="Scheduler Page"
fi
if [ ! -f "Pages/Admin/Index.cshtml" ]; then
    MISSING_FILES="$MISSING_FILES Admin Panel"
fi
if [ ! -f "wwwroot/js/scheduler-ui.js" ]; then
    MISSING_FILES="$MISSING_FILES JavaScript"
fi

if [ -n "$MISSING_FILES" ]; then
    echo "? Missing critical files:$MISSING_FILES"
    exit 1
fi
echo "? All core files present"

echo
echo "[4/4] Starting OpCentrix..."
echo
echo "?? OpCentrix is starting..."
echo "?? Open your browser to: http://localhost:5000"
echo "?? Login with: admin / admin123"
echo
echo "??  Press Ctrl+C to stop the application"
echo

# Start the application
dotnet run

# This line only executes if dotnet run exits
echo
echo "OpCentrix has stopped."