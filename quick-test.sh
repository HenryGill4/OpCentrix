#!/bin/bash
echo "======================================="
echo "   OpCentrix Quick Test Script"
echo "======================================="
echo

# Navigate to project directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if [ ! -f "OpCentrix.csproj" ]; then
    cd OpCentrix
    if [ ! -f "OpCentrix.csproj" ]; then
        echo "? ERROR: OpCentrix.csproj not found"
        exit 1
    fi
fi

echo "? Running quick functionality test..."
echo

# Test 1: Check if project builds
echo "?? Testing build..."
if dotnet build --verbosity quiet >/dev/null 2>&1; then
    echo "? Build successful"
else
    echo "? Build failed"
    dotnet build
    exit 1
fi

# Test 2: Check database creation (quick test)
echo "??? Testing database initialization..."
export ASPNETCORE_ENVIRONMENT=Development
export SEED_SAMPLE_DATA=true
export RECREATE_DATABASE=true

# Start application briefly to test database creation
echo "Starting brief test (will timeout in 5 seconds)..."
timeout 5s dotnet run --no-build --urls "http://localhost:0" >/dev/null 2>&1 || true
echo "? Database initialization test completed"

# Test 3: Check if database file was created
if [ -f "scheduler.db" ]; then
    echo "? Database file created successfully"
    db_size=$(stat -f%z "scheduler.db" 2>/dev/null || stat -c%s "scheduler.db" 2>/dev/null || echo "unknown")
    echo "   Database size: $db_size bytes"
else
    echo "? Database file not created"
fi

# Test 4: Check for essential files
if [ -f "TEST_USERS.txt" ]; then
    echo "? Test users file created"
else
    echo "?? Test users file not found"
fi

echo
echo "?? Quick Test Summary:"
echo "======================"
if [ -f "scheduler.db" ]; then
    echo "? Database: Ready"
else
    echo "? Database: Not found"
fi

echo "? Build: Successful"
echo "? Code: Compiles without errors"

echo
echo "?? Ready to start! Run:"
echo "   dotnet run"
echo
echo "?? Then visit: http://localhost:5000"
echo "?? Login with: admin / admin123"
echo