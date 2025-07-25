#!/bin/bash

echo "====================================="
echo "OpCentrix Database Reset and Setup"
echo "====================================="
echo ""

# Check if OpCentrix directory exists
if [ ! -d "OpCentrix" ]; then
    echo "ERROR: OpCentrix directory not found. Please run from repository root."
    exit 1
fi

cd OpCentrix

echo "Step 1: Checking database directory..."
if [ ! -d "Data" ]; then
    mkdir -p Data
    echo "INFO: Created Data directory"
fi

echo "Step 2: Removing existing database..."
if [ -f "Data/OpCentrix.db" ]; then
    rm "Data/OpCentrix.db"
    echo "SUCCESS: Removed existing database"
else
    echo "INFO: No existing database found"
fi

echo "Step 3: Removing any existing migration files..."
if [ -d "Migrations" ]; then
    rm -rf "Migrations"
    echo "INFO: Removed existing migrations"
fi

echo "Step 4: Building project..."
if dotnet build > /dev/null 2>&1; then
    echo "SUCCESS: Project built successfully"
else
    echo "ERROR: Build failed"
    exit 1
fi

echo "Step 5: Creating fresh database..."
echo "INFO: Starting application to initialize database..."
timeout 20s dotnet run --no-build > database-init.log 2>&1 &
APP_PID=$!

# Wait for database creation
echo "INFO: Waiting for database initialization..."
sleep 15

# Stop the application
kill $APP_PID 2>/dev/null || true
wait $APP_PID 2>/dev/null || true

echo "Step 6: Verifying database creation..."
if [ -f "Data/OpCentrix.db" ]; then
    echo "SUCCESS: Database created successfully"
    
    # Check database content if sqlite3 is available
    if command -v sqlite3 &> /dev/null; then
        echo "Step 7: Verifying database content..."
        USER_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM Users;" 2>/dev/null || echo "0")
        PART_COUNT=$(sqlite3 Data/OpCentrix.db "SELECT COUNT(*) FROM Parts;" 2>/dev/null || echo "0")
        
        echo "SUCCESS: Database contains $USER_COUNT users and $PART_COUNT parts"
        
        echo ""
        echo "Sample users created:"
        sqlite3 -header -column Data/OpCentrix.db "SELECT Username, Role, FullName FROM Users LIMIT 5;" 2>/dev/null || echo "Could not retrieve user list"
    else
        echo "INFO: sqlite3 not available - cannot verify database content"
    fi
else
    echo "ERROR: Database was not created"
    echo "Check database-init.log for details:"
    cat database-init.log 2>/dev/null || echo "No log file found"
    exit 1
fi

# Clean up log file
rm -f database-init.log

echo ""
echo "====================================="
echo "DATABASE SETUP COMPLETE!"
echo "====================================="
echo ""
echo "The database has been created and seeded with sample data."
echo "You can now start the application with: dotnet run"
echo ""
echo "Login credentials:"
echo "- admin / admin123 (Administrator)"
echo "- manager / manager123 (Manager)"
echo "- scheduler / scheduler123 (Scheduler)"
echo ""

cd ..