# OpCentrix SLS Scheduler - Production Ready

## Quick Start Guide

### Prerequisites
- .NET 8.0 SDK
- Windows, Linux, or macOS
- No additional database setup required (SQLite embedded)

### Installation
1. **Clone or download the repository**
2. **Run the setup script**:
   ```bash
   # Windows
   setup-clean-database.bat
   
   # Linux/Mac
   chmod +x setup-clean-database.sh
   ./setup-clean-database.sh
   ```

3. **Start the application**:
   ```bash
   cd OpCentrix
   dotnet run
   ```

4. **Open browser and login**:
   - URL: http://localhost:5000
   - Username: `admin`
   - Password: `admin123`

---

## System Testing

### Complete System Verification
Run the comprehensive test suite to verify everything works:

```bash
# Windows
test-complete-system.bat

# Linux/Mac
chmod +x test-complete-system.sh
./test-complete-system.sh
```

This tests:
- .NET SDK installation
- Package restoration
- Project build
- Database creation
- Data seeding
- Application startup
- HTTP endpoints

### Database Verification
Check if parts and data are saving correctly:

```bash
# Windows
verify-parts-database.bat

# Linux/Mac
chmod +x verify-parts-database.sh
./verify-parts-database.sh
```

---

## User Accounts

The system comes with pre-configured test users:

| Username | Password | Role | Access Level |
|----------|----------|------|--------------|
| admin | admin123 | Administrator | Full system access |
| manager | manager123 | Manager | Management functions |
| scheduler | scheduler123 | Scheduler | Job scheduling |
| operator | operator123 | Operator | Basic operations |
| printer | printer123 | PrintingSpecialist | Print operations |

---

## Features

### Scheduler
- Visual job scheduling interface
- Drag-and-drop job management
- Machine capacity planning
- Conflict detection
- Real-time updates

### Parts Management
- Complete CRUD operations
- Material specifications
- Cost tracking
- Usage history

### Print Tracking
- Build job monitoring
- Performance metrics
- Quality tracking
- Resource usage

### Administration
- User management
- System configuration
- Data backup/restore
- Analytics dashboard

---

## Architecture

### Technology Stack
- **Backend**: ASP.NET Core 8 (Razor Pages)
- **Database**: SQLite (embedded, no setup required)
- **Frontend**: HTML5, CSS3, JavaScript, HTMX
- **Authentication**: Cookie-based with role authorization

### Project Structure
```
OpCentrix/
├── Data/                  # Database context and models
├── Models/               # Data entities and view models
├── Pages/                # Razor Pages (UI)
├── Services/             # Business logic
├── wwwroot/              # Static files (CSS, JS, images)
├── Migrations/           # Database migrations
└── Program.cs            # Application startup
```

---

## Database

### Automatic Initialization
The database is created automatically on first run with:
- User accounts and roles
- Sample parts and materials
- Machine configurations
- Default settings

### Data Storage
- **Location**: `OpCentrix/Data/OpCentrix.db`
- **Type**: SQLite (single file, no server required)
- **Backup**: Copy the .db file to backup data

### Reset Database
To start fresh:
```bash
# Windows
setup-clean-database.bat

# Linux/Mac
./setup-clean-database.sh
```

---

## Troubleshooting

### Common Issues

#### Application Won't Start
1. Check .NET 8 SDK is installed: `dotnet --version`
2. Restore packages: `dotnet restore`
3. Build project: `dotnet build`
4. Check port 5000 is available

#### Database Issues
1. Ensure write permissions to `Data/` directory
2. Delete existing database and restart app
3. Run database verification script

#### Parts Not Saving
1. Check browser console for JavaScript errors
2. Verify all required fields are filled
3. Ensure unique part numbers
4. See PARTS-TROUBLESHOOTING-GUIDE.md

#### Login Problems
1. Use correct credentials (admin/admin123)
2. Clear browser cookies
3. Check application logs for errors

### Getting Help
1. Check application logs in console
2. Run diagnostic scripts
3. Review troubleshooting guides
4. Check browser developer tools

---

## Development

### Requirements
- Visual Studio 2022 or VS Code
- .NET 8.0 SDK
- Git for version control

### Building
```bash
dotnet restore
dotnet build
dotnet run
```

### Testing
```bash
dotnet test
```

### Adding Features
See the comprehensive guides in:
- `AI-INSTRUCTIONS-NO-UNICODE.md` - Important coding standards
- `PARTS-TROUBLESHOOTING-GUIDE.md` - Common issues and solutions

---

## Deployment

### Development
- Uses SQLite database (embedded)
- Runs on http://localhost:5000
- Debug logging enabled

### Production
- Can use SQL Server, PostgreSQL, or SQLite
- Configure connection strings in appsettings.json
- Enable HTTPS
- Set production logging levels

### Docker (Optional)
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
COPY . /app
WORKDIR /app
EXPOSE 80
ENTRYPOINT ["dotnet", "OpCentrix.dll"]
```

---

## Security

### Authentication
- Cookie-based authentication
- Role-based authorization
- Session management
- Password hashing (BCrypt)

### Data Protection
- Input validation
- SQL injection prevention
- XSS protection
- CSRF tokens

### Roles and Permissions
- Admin: Full system access
- Manager: Management functions
- Specialist: Department-specific access
- Operator: Basic operations only

---

## Performance

### Optimizations
- Efficient database queries
- Partial page updates (HTMX)
- Optimized CSS and JavaScript
- Database indexing

### Monitoring
- Application logging
- Performance counters
- Error tracking
- Usage analytics

---

## Support

### Documentation
- Complete API documentation
- User guides
- Developer documentation
- Troubleshooting guides

### Maintenance
- Regular database backups
- Log file rotation
- Performance monitoring
- Security updates

---

## License

This is a proprietary manufacturing scheduler system. All rights reserved.

---

## Version History

### v2.0.0 (Current)
- Complete rewrite for .NET 8
- Enhanced UI with HTMX
- Improved performance
- Comprehensive error handling
- Production-ready deployment

### v1.0.0
- Initial version
- Basic scheduling functionality
- SQLite database
- User authentication

---

## Contact

For technical support or questions about the OpCentrix SLS Scheduler system, please refer to the documentation or contact your system administrator.

**Last Updated**: December 2024
**System Status**: Production Ready