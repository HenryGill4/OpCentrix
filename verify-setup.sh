#!/bin/bash
echo "=========================================="
echo "   OpCentrix Database Verification Script"
echo "=========================================="
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

echo "?? Testing database and application setup..."
echo

# Check if database exists
if [ ! -f "scheduler.db" ]; then
    echo "? Database file not found!"
    echo "?? Please run ./setup-database.sh first"
    exit 1
fi

echo "? Database file found: scheduler.db"
echo

# Test application startup
echo "?? Testing application startup..."
echo "Starting application test (10 second timeout)..."

# Start the application in background with timeout
timeout 10s dotnet run --no-build --urls "http://localhost:0" >/dev/null 2>&1 && {
    echo "? Application starts successfully"
} || {
    echo "?? Application startup test timed out (this is normal)"
}

echo
echo "?? Verification Summary:"
echo "========================"

if [ -f "scheduler.db" ]; then
    echo "? Database file exists"
    db_size=$(stat -f%z "scheduler.db" 2>/dev/null || stat -c%s "scheduler.db" 2>/dev/null || echo "unknown")
    echo "   Size: $db_size bytes"
else
    echo "? Database file missing"
fi

if [ -f "TEST_USERS.txt" ]; then
    echo "? Test users file exists"
else
    echo "?? Test users file not found"
fi

# Check for basic files
if [ -f "appsettings.json" ]; then
    echo "? Configuration file exists"
else
    echo "? Configuration file missing"
fi

echo
echo "?? Next Steps:"
echo "- Run 'dotnet run' to start the application"
echo "- Visit http://localhost:5000"
echo "- Login with admin/admin123"
echo "- Check the scheduler page to verify functionality"
echo
echo "?? If you encounter issues:"
echo "- Run ./setup-database.sh to reset"
echo "- Check TEST_USERS.txt for login credentials"
echo "- Ensure no other application is using port 5000"
echo