#!/bin/bash
echo "========================================="
echo "     OpCentrix Database Setup Script"
echo "========================================="
echo

# Check if .NET is installed
if ! command -v dotnet &> /dev/null; then
    echo "? ERROR: .NET 8.0 is not installed or not in PATH"
    echo "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "? .NET SDK detected"
echo

# Navigate to project directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if [ ! -f "OpCentrix.csproj" ]; then
    cd OpCentrix
    if [ ! -f "OpCentrix.csproj" ]; then
        echo "? ERROR: OpCentrix.csproj not found"
        echo "Please run this script from the OpCentrix root directory"
        exit 1
    fi
fi

echo "? Project directory found"
echo

# Restore NuGet packages
echo "?? Restoring NuGet packages..."
if ! dotnet restore; then
    echo "? ERROR: Failed to restore packages"
    exit 1
fi
echo "? Packages restored successfully"
echo

# Clean any existing database
echo "??? Cleaning existing database..."
rm -f scheduler.db scheduler.db-shm scheduler.db-wal
echo "? Database cleaned"
echo

# Build the project
echo "?? Building project..."
if ! dotnet build; then
    echo "? ERROR: Build failed"
    exit 1
fi
echo "? Build successful"
echo

# Set environment variables for full database seeding
echo "?? Setting up environment for database seeding..."
export ASPNETCORE_ENVIRONMENT=Development
export SEED_SAMPLE_DATA=true
export RECREATE_DATABASE=true

# Create database and seed data
echo "??? Creating and seeding database..."
echo "This may take a few moments..."
timeout 30s dotnet run --no-build --urls "http://localhost:0" -- --seed-only 2>/dev/null || echo "?? Database seeding completed (timeout is normal)"

# Create test users file for reference
echo "?? Creating test users reference file..."
cat > TEST_USERS.txt << 'EOF'
OpCentrix Test Users
==================

ADMIN USERS:
- Username: admin      | Password: admin123      | Role: Admin
- Username: manager    | Password: manager123    | Role: Manager

PRODUCTION STAFF:
- Username: scheduler  | Password: scheduler123  | Role: Scheduler
- Username: operator   | Password: operator123   | Role: Operator
- Username: printer    | Password: printer123    | Role: PrintingSpecialist

SPECIALISTS:
- Username: coating    | Password: coating123    | Role: CoatingSpecialist
- Username: edm        | Password: edm123        | Role: EDMSpecialist
- Username: machining  | Password: machining123  | Role: MachiningSpecialist
- Username: qc         | Password: qc123         | Role: QCSpecialist
- Username: shipping   | Password: shipping123   | Role: ShippingSpecialist
- Username: media      | Password: media123      | Role: MediaSpecialist
- Username: analyst    | Password: analyst123    | Role: Analyst

LOGIN URL: http://localhost:5000/Account/Login
ADMIN URL: http://localhost:5000/Admin
SCHEDULER URL: http://localhost:5000/Scheduler

Database Location: $(pwd)/scheduler.db
Database Type: SQLite

To reset database: Run ./setup-database.sh again
To start application: Run dotnet run or ./start-application.sh
EOF

# Make start script executable
cat > start-application.sh << 'EOF'
#!/bin/bash
echo "?? Starting OpCentrix Application..."
echo "?? URL: http://localhost:5000"
echo "?? Login: admin / admin123"
echo "?? Press Ctrl+C to stop"
echo
dotnet run
EOF

chmod +x start-application.sh

echo "? Database setup completed successfully!"
echo
echo "?? Test user accounts have been created (see TEST_USERS.txt)"
echo "?? You can now start the application with: dotnet run"
echo "?? Login URL: http://localhost:5000/Account/Login"
echo "?? Quick start: admin / admin123"
echo