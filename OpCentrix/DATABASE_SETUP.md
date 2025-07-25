# OpCentrix Database Setup Guide

This guide will help you set up the OpCentrix SLS Scheduler database correctly on any computer where the git repository is cloned.

## ?? Quick Start (Automated Setup)

### Windows
```batch
# Run the automated setup script
setup-database.bat

# Start the application
start-application.bat
```

### Linux/Mac
```bash
# Make scripts executable
chmod +x setup-database.sh
chmod +x start-application.sh

# Run the automated setup script
./setup-database.sh

# Start the application
./start-application.sh
```

## ?? Prerequisites

Before setting up the database, ensure you have:

- **.NET 8.0 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
- **Git** - For cloning the repository
- **Visual Studio 2022** or **VS Code** (optional, for development)

### Verify Prerequisites
```bash
# Check .NET version
dotnet --version

# Should show 8.0.x or higher
```

## ??? Database Architecture

OpCentrix uses **SQLite** as the default database engine, which provides:

- ? **Zero Configuration** - No database server required
- ? **Portable** - Single file database
- ? **Reliable** - ACID compliant
- ? **Fast** - Optimized for read-heavy workloads
- ? **Cross-Platform** - Works on Windows, Linux, Mac

### Database File Location
- **File**: `scheduler.db` (in the application root)
- **Type**: SQLite 3
- **Encoding**: UTF-8
- **Journal Mode**: WAL (Write-Ahead Logging)

## ?? Manual Setup (Step by Step)

If you prefer to set up manually or need to troubleshoot:

### 1. Clone and Navigate
```bash
git clone [repository-url]
cd OpCentrix
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build the Project
```bash
dotnet build
```

### 4. Initialize Database
```bash
# For development with sample data
export ASPNETCORE_ENVIRONMENT=Development
export SEED_SAMPLE_DATA=true

# For production (clean start)
export ASPNETCORE_ENVIRONMENT=Production
export SEED_SAMPLE_DATA=false

# Create and seed database
dotnet run
```

## ?? Default User Accounts

After setup, the following test accounts are available:

### Administrative Users
| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `admin` | `admin123` | Admin | Full system access |
| `manager` | `manager123` | Manager | Production oversight |

### Production Staff
| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `scheduler` | `scheduler123` | Scheduler | Job scheduling |
| `operator` | `operator123` | Operator | Machine operation |
| `printer` | `printer123` | PrintingSpecialist | 3D printing expert |

### Department Specialists
| Username | Password | Role | Description |
|----------|----------|------|-------------|
| `coating` | `coating123` | CoatingSpecialist | Coating operations |
| `edm` | `edm123` | EDMSpecialist | EDM operations |
| `machining` | `machining123` | MachiningSpecialist | Machining operations |
| `qc` | `qc123` | QCSpecialist | Quality control |
| `shipping` | `shipping123` | ShippingSpecialist | Shipping operations |
| `media` | `media123` | MediaSpecialist | Media blasting |
| `analyst` | `analyst123` | Analyst | Data analysis |

> ?? **Security Notice**: Change default passwords before production deployment!

## ?? Sample Data

The setup includes realistic sample data:

### SLS Machines (3 machines)
- **TI1** - TruPrint 3000 (Titanium Line 1)
- **TI2** - TruPrint 3000 (Titanium Line 2) 
- **INC** - TruPrint 3000 (Inconel Line)

### Parts Library (8+ parts)
- Aerospace turbine blades
- Medical implants
- Automotive prototypes
- Industrial components

### Scheduled Jobs (14 days)
- Realistic job schedules
- Multiple materials (Ti-6Al-4V, Inconel 718)
- Varying priorities and durations
- Machine-specific assignments

## ?? Environment Configuration

### Development Environment
```bash
# Enable sample data and detailed logging
export ASPNETCORE_ENVIRONMENT=Development
export SEED_SAMPLE_DATA=true
export ENABLE_DETAILED_LOGGING=true
```

### Production Environment
```bash
# Disable sample data, enable optimizations
export ASPNETCORE_ENVIRONMENT=Production
export SEED_SAMPLE_DATA=false
export ENABLE_DETAILED_LOGGING=false
```

### Custom Configuration
Create a `.env` file (copy from `.env.example`):
```ini
ASPNETCORE_ENVIRONMENT=Development
SEED_SAMPLE_DATA=true
RECREATE_DATABASE=false
DATABASE_TYPE=SQLite
DEFAULT_SESSION_TIMEOUT=120
```

## ?? Database Management

### Reset Database (Development)
```bash
# Windows
setup-database.bat

# Linux/Mac
./setup-database.sh
```

### Reset to Production
```bash
# Windows
reset-to-production.bat

# Linux/Mac
./reset-to-production.sh
```

### Backup Database
```bash
# Manual backup
cp scheduler.db scheduler_backup_$(date +%Y%m%d).db

# Using the application (planned feature)
# Access /Admin/Database for backup tools
```

### Verify Setup
```bash
# Windows
verify-setup.bat

# Linux/Mac
./verify-setup.sh
```

## ?? Troubleshooting

### Common Issues

#### Database File Locked
```bash
# Stop all instances of the application
pkill -f "dotnet.*OpCentrix" # Linux/Mac
taskkill /F /IM dotnet.exe    # Windows

# Remove lock files
rm -f scheduler.db-wal scheduler.db-shm
```

#### Port Already in Use
```bash
# Find process using port 5000
lsof -i :5000          # Linux/Mac
netstat -ano | findstr :5000  # Windows

# Kill the process
kill -9 <PID>          # Linux/Mac
taskkill /PID <PID> /F # Windows
```

#### Build Errors
```bash
# Clean and restore
dotnet clean
dotnet restore
dotnet build
```

#### Database Corruption
```bash
# Reset database completely
rm scheduler.db*       # Linux/Mac
del scheduler.db*      # Windows

# Run setup again
./setup-database.sh    # Linux/Mac
setup-database.bat     # Windows
```

### Advanced Troubleshooting

#### Enable Detailed Logging
Add to `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

#### Database Connection Issues
Check connection string in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=scheduler.db"
  }
}
```

#### Missing Dependencies
```bash
# Restore all packages
dotnet restore

# Check for outdated packages
dotnet list package --outdated
```

## ?? Database Schema

### Core Tables
- **Users** - User accounts and authentication
- **UserSettings** - User preferences and configuration
- **Jobs** - Scheduled manufacturing jobs
- **Parts** - Part specifications and parameters
- **SlsMachines** - Machine configurations
- **JobLogEntries** - Audit trail for job operations

### Print Tracking Tables
- **BuildJobs** - Actual print operations
- **BuildJobParts** - Parts in each build
- **DelayLogs** - Production delay tracking

### Performance Optimization
- Indexed columns for fast queries
- Optimized relationships
- Efficient data types
- Query result caching

## ?? Security Considerations

### Development Security
- Default passwords for easy testing
- Session timeout: 2 hours
- HTTPS optional
- Detailed error messages

### Production Security
- **Change all default passwords**
- Session timeout: 8 hours
- HTTPS required
- Minimal error disclosure
- SQL injection protection
- CSRF protection enabled

## ?? Performance Optimization

### Database Performance
- **SQLite optimizations**: WAL mode, optimized pragmas
- **Indexing**: Strategic indexes on frequently queried columns
- **Query optimization**: Efficient LINQ queries
- **Connection pooling**: Managed by Entity Framework

### Application Performance
- **Lazy loading**: Disabled for predictable performance
- **AsNoTracking**: Used for read-only queries
- **Bulk operations**: Optimized for large datasets
- **Memory management**: Proper disposal patterns

## ?? Additional Resources

### Documentation
- See `ProjectNotes/` folder for detailed documentation
- Check `SCHEDULER-PRODUCTION-READY.md` for deployment guide
- Review `DATABASE-README.md` for technical details

### Support
- Check logs in the application output
- Use `/health` endpoint for system status
- Access admin panel at `/Admin` for diagnostics

### Development
- Use Visual Studio for debugging
- Entity Framework migrations for schema changes
- Unit tests for business logic validation

---

## ? Verification Checklist

After setup, verify these items work:

- [ ] Application starts without errors
- [ ] Can login with admin/admin123
- [ ] Scheduler page loads with machine rows
- [ ] Can create and edit jobs
- [ ] Admin panel accessible
- [ ] Database health check passes
- [ ] All test users can login
- [ ] Sample data appears correctly

## ?? Next Steps

1. **Start the application**: `dotnet run`
2. **Open browser**: http://localhost:5000
3. **Login**: admin / admin123
4. **Test scheduling**: Create a new job
5. **Explore features**: Check different user roles
6. **Customize**: Modify parts and machines as needed

---

**?? Congratulations! Your OpCentrix database is now fully configured and ready for use.**

For production deployment, run the production reset script and change all default passwords.