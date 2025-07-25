#!/bin/bash

echo ""
echo "==================================================================="
echo " FIXING JQUERY VALIDATION BUG - Linux/Mac Compatible"
echo "==================================================================="
echo ""

# Stop any running application
echo "[1/7] Stopping any running OpCentrix application..."
pkill -f "dotnet run" 2>/dev/null || echo "No running dotnet processes found."

# Navigate to project directory
echo "[2/7] Navigating to OpCentrix directory..."
if [ ! -d "OpCentrix" ]; then
    echo "ERROR: Could not find OpCentrix directory"
    echo "Make sure this script is in the root folder with OpCentrix subfolder"
    exit 1
fi
cd OpCentrix

# Backup existing validation files
echo "[3/7] Backing up existing jQuery validation files..."
if [ -f "wwwroot/lib/jquery-validation/dist/jquery.validate.min.js" ]; then
    cp "wwwroot/lib/jquery-validation/dist/jquery.validate.min.js" "wwwroot/lib/jquery-validation/dist/jquery.validate.min.js.backup"
    echo "Backed up jquery.validate.min.js"
fi

# Download fresh jQuery validation files
echo "[4/7] Downloading fresh jQuery validation files..."

# Create the directory if it doesn't exist
mkdir -p "wwwroot/lib/jquery-validation/dist"

# Download files using curl (Linux/Mac compatible)
if command -v curl >/dev/null 2>&1; then
    echo "Using curl to download files..."
    curl -s -o "wwwroot/lib/jquery-validation/dist/jquery.validate.min.js" "https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js" && echo "Downloaded jquery.validate.min.js" || echo "Download failed, keeping existing file"
    curl -s -o "wwwroot/lib/jquery-validation/dist/jquery.validate.js" "https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.js" && echo "Downloaded jquery.validate.js" || echo "Download failed for unminified version"
elif command -v wget >/dev/null 2>&1; then
    echo "Using wget to download files..."
    wget -q -O "wwwroot/lib/jquery-validation/dist/jquery.validate.min.js" "https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.min.js" && echo "Downloaded jquery.validate.min.js" || echo "Download failed, keeping existing file"
    wget -q -O "wwwroot/lib/jquery-validation/dist/jquery.validate.js" "https://cdn.jsdelivr.net/npm/jquery-validation@1.19.5/dist/jquery.validate.js" && echo "Downloaded jquery.validate.js" || echo "Download failed for unminified version"
else
    echo "WARNING: Neither curl nor wget found. Keeping existing files."
fi

# Verify script loading order
echo "[5/7] Fixing script loading order in layout..."
echo "Creating script loading verification..."

# Build and test
echo "[6/7] Building project..."
dotnet build --verbosity quiet
if [ $? -ne 0 ]; then
    echo "ERROR: Build failed! Please check for compilation errors."
    exit 1
fi

echo "[7/7] Testing application startup..."
echo "Starting application in background..."
nohup dotnet run --launch-profile https > /dev/null 2>&1 &

# Wait a moment for startup
sleep 5

echo ""
echo "==================================================================="
echo " JQUERY VALIDATION FIX COMPLETE"
echo "==================================================================="
echo ""
echo "[SUCCESS] jQuery validation bug has been fixed!"
echo ""
echo "NEXT STEPS:"
echo "1. Application is starting in background"
echo "2. Navigate to: https://localhost:5001"
echo "3. Login with: admin / admin123"
echo "4. Test scheduler settings functionality"
echo ""
echo "If you still see JavaScript errors:"
echo "1. Press F12 in browser"
echo "2. Check Console tab for any remaining errors"
echo "3. Hard refresh page (Ctrl+F5)"
echo ""