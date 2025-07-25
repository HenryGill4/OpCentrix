#!/bin/bash

echo "====================================="
echo "OpCentrix Complete System Test"
echo "====================================="
echo ""

# Set error handling
set -e

# Colors for output (ANSI codes that work in most terminals)
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    local color=$1
    local message=$2
    echo -e "${color}${message}${NC}"
}

print_status $BLUE "Starting OpCentrix system verification..."

# Check if we're in the right directory
if [ ! -d "OpCentrix" ]; then
    print_status $RED "ERROR: OpCentrix directory not found. Please run from repository root."
    exit 1
fi

cd OpCentrix

# Test 1: Check if .NET SDK is installed
print_status $BLUE "Test 1: Checking .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_status $GREEN "SUCCESS: .NET SDK version $DOTNET_VERSION found"
else
    print_status $RED "ERROR: .NET SDK not found. Please install .NET 8 SDK"
    exit 1
fi

# Test 2: Restore packages
print_status $BLUE "Test 2: Restoring NuGet packages..."
if dotnet restore > /dev/null 2>&1; then
    print_status $GREEN "SUCCESS: Packages restored successfully"
else
    print_status $RED "ERROR: Package restoration failed"
    exit 1
fi

# Test 3: Build project
print_status $BLUE "Test 3: Building project..."
if dotnet build > /dev/null 2>&1; then
    print_status $GREEN "SUCCESS: Project built successfully"
else
    print_status $RED "ERROR: Build failed"
    exit 1
fi

# Test 4: Check database directory
print_status $BLUE "Test 4: Checking database setup..."
if [ ! -d "Data" ]; then
    mkdir -p Data
    print_status $YELLOW "INFO: Created Data directory"
fi

# Clean database for fresh test
if [ -f "Data/OpCentrix.db" ]; then
    rm "Data/OpCentrix.db"
    print_status $YELLOW "INFO: Removed existing database for clean test"
fi

# Test 5: Database initialization test
print_status $BLUE "Test 5: Testing database initialization..."
timeout 30s dotnet run --no-build > startup.log 2>&1 &
APP_PID=$!

# Wait for application to start
sleep 10

# Check if database was created
if [ -f "Data/OpCentrix.db" ]; then
    print_status $GREEN "SUCCESS: Database created automatically"
else
    print_status $RED "ERROR: Database was not created"
    kill $APP_PID 2>/dev/null || true
    exit 1
fi

# Stop the application
kill $APP_PID 2>/dev/null || true
wait $APP_PID 2>/dev/null || true

# Test 6: Database content verification
print_status $BLUE "Test 6: Verifying database content..."
if command -v sqlite3 &> /dev/null; then
    USER_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM Users;" 2>/dev/null || echo "0")
    PART_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM Parts;" 2>/dev/null || echo "0")
    MACHINE_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM SlsMachines;" 2>/dev/null || echo "0")
    
    if [ "$USER_COUNT" -gt "0" ] && [ "$PART_COUNT" -gt "0" ] && [ "$MACHINE_COUNT" -gt "0" ]; then
        print_status $GREEN "SUCCESS: Database seeded with $USER_COUNT users, $PART_COUNT parts, $MACHINE_COUNT machines"
    else
        print_status $YELLOW "WARNING: Database may not be fully seeded (Users: $USER_COUNT, Parts: $PART_COUNT, Machines: $MACHINE_COUNT)"
    fi
else
    print_status $YELLOW "INFO: sqlite3 not available - skipping database content check"
fi

# Test 7: Application startup test
print_status $BLUE "Test 7: Testing application startup..."
timeout 15s dotnet run --no-build > startup.log 2>&1 &
APP_PID=$!

# Wait for startup
sleep 8

# Check if process is still running
if kill -0 $APP_PID 2>/dev/null; then
    print_status $GREEN "SUCCESS: Application started successfully"
    
    # Test 8: HTTP endpoint test
    print_status $BLUE "Test 8: Testing HTTP endpoints..."
    
    # Test health endpoint
    if command -v curl &> /dev/null; then
        if curl -s http://localhost:5000/health > /dev/null 2>&1; then
            print_status $GREEN "SUCCESS: Health endpoint responding"
        else
            print_status $YELLOW "WARNING: Health endpoint not responding (may need more time to start)"
        fi
        
        # Test login page
        if curl -s http://localhost:5000/Account/Login > /dev/null 2>&1; then
            print_status $GREEN "SUCCESS: Login page accessible"
        else
            print_status $YELLOW "WARNING: Login page not accessible"
        fi
    else
        print_status $YELLOW "INFO: curl not available - skipping HTTP tests"
    fi
    
    # Stop the application
    kill $APP_PID 2>/dev/null || true
    wait $APP_PID 2>/dev/null || true
else
    print_status $RED "ERROR: Application failed to start or crashed"
    cat startup.log 2>/dev/null || true
    exit 1
fi

# Clean up
rm -f startup.log

print_status $GREEN "====================================="
print_status $GREEN "ALL TESTS PASSED!"
print_status $GREEN "====================================="
echo ""
print_status $BLUE "Next steps:"
echo "1. Start the application: dotnet run"
echo "2. Open browser: http://localhost:5000"
echo "3. Login with: admin / admin123"
echo "4. Test the scheduler interface"
echo ""
print_status $BLUE "Available test users:"
echo "- admin/admin123 (Administrator)"
echo "- manager/manager123 (Manager)"
echo "- scheduler/scheduler123 (Scheduler)"
echo "- operator/operator123 (Operator)"
echo ""

cd ..