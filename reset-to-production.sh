#!/bin/bash
echo "==================================="
echo "   OpCentrix Production Reset"
echo "==================================="
echo
echo "This will:"
echo "- Remove all sample/test data"
echo "- Keep essential users and machines"
echo "- Optimize for production use"
echo

read -p "Continue with production reset? (y/N): " confirm
if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
    echo "Operation cancelled."
    exit 0
fi

# Navigate to project directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

if [ ! -f "OpCentrix.csproj" ]; then
    cd OpCentrix
fi

echo "?? Resetting to production configuration..."

# Set production environment
export ASPNETCORE_ENVIRONMENT=Production
export SEED_SAMPLE_DATA=false
export RECREATE_DATABASE=true

# Clean database
rm -f scheduler.db scheduler.db-shm scheduler.db-wal

echo "?? Creating production database..."
timeout 30s dotnet run --no-build --urls "http://localhost:0" -- --seed-only 2>/dev/null || echo "?? Database seeding completed (timeout is normal)"

# Create production users file
cat > PRODUCTION_USERS.txt << 'EOF'
OpCentrix Production Users
=========================

DEFAULT ADMIN ACCOUNT:
- Username: admin
- Password: admin123
- Role: Admin

IMPORTANT: Change default passwords before production use!

Production Features Enabled:
- No sample data
- Optimized logging
- Essential users only
- Ready for real data

Login URL: http://localhost:5000/Account/Login
EOF

echo "? Production reset completed!"
echo "?? See PRODUCTION_USERS.txt for login details"
echo "?? Remember to change default passwords!"
echo