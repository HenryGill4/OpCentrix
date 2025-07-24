# ?? OpCentrix SLS Print Job Scheduler

## ?? Project Overview

OpCentrix is a comprehensive **Selective Laser Sintering (SLS) Print Job Scheduler** designed for manufacturing environments. It provides a complete solution for managing SLS print jobs, parts, materials, and operations across multiple machines with role-based access control and real-time monitoring.

## ?? Key Features

### ??? **Advanced Scheduling**
- **Visual Grid-Based Scheduler** with timeline views (Day, Hour, 30min, 15min)
- **Machine-Specific Scheduling** for TI1, TI2, and INC machines
- **Conflict Detection** prevents overlapping jobs
- **Drag-and-Drop Interface** for easy job management
- **Real-Time Updates** with HTMX integration

### ?? **Role-Based Access Control**
- **Admin**: Full system access and configuration
- **Manager**: Scheduling and oversight capabilities
- **Scheduler**: Job management and coordination
- **Operator**: Machine operation and job execution
- **Specialists**: Department-specific access (Printing, Coating, EDM, etc.)

### ?? **Comprehensive Admin System**
- **Real-Time Dashboard** with KPIs and system status
- **Complete CRUD Operations** for jobs, parts, and logs
- **Advanced Filtering** and search capabilities
- **Audit Trail** for all operations
- **Database Management** tools

### ?? **Professional Design System**
- **Modern UI/UX** with consistent branding
- **Responsive Design** for desktop, tablet, and mobile
- **Accessibility Compliant** (WCAG AA standards)
- **Professional Color Palette** with gradient effects
- **Smooth Animations** and micro-interactions

### ?? **Manufacturing Integration**
- **SLS Material Management** with powder specifications
- **OPC UA Integration** for machine connectivity
- **Print Tracking System** with build job management
- **Quality Control** checkpoints and inspections
- **Cost Tracking** for materials, labor, and overhead

## ??? Technology Stack

### **Backend**
- **Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: Entity Framework Core with SQLite
- **Authentication**: Cookie-based authentication with role management
- **API Integration**: OPC UA for machine communication
- **Real-Time**: HTMX for seamless partial updates

### **Frontend**
- **Styling**: Custom CSS with OpCentrix design system
- **JavaScript**: Modern ES6+ with enhanced error logging
- **UI Library**: Custom components with Bootstrap foundation
- **Icons**: Heroicons for consistent iconography
- **Responsive**: Mobile-first responsive design

### **Database**
- **Primary**: SQLite for development and small deployments
- **Migrations**: EF Core migrations for schema management
- **Optimization**: Indexed queries and efficient relationships
- **Analytics**: Enhanced job analytics and reporting

## ?? Quick Start

### **Prerequisites**
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git

### **Installation**
```bash
# Clone the repository
git clone [repository-url]
cd OpCentrix

# Restore packages
dotnet restore

# Run database migrations
dotnet ef database update

# Start the application
dotnet run
```

### **Default Access**
- **URL**: `http://localhost:5000`
- **Admin User**: `admin` / `admin123`
- **Manager User**: `manager` / `manager123`
- **Test Users**: See `TEST_USERS.txt` for complete list

## ?? Project Structure

```
OpCentrix/
??? ?? Data/                    # Database context and migrations
??? ?? Models/                  # Entity models and ViewModels
?   ??? ?? ViewModels/         # Page-specific ViewModels
?   ??? Job.cs                 # Core job entity
?   ??? Part.cs                # Manufacturing part entity
?   ??? User.cs                # User and authentication
??? ?? Pages/                   # Razor Pages
?   ??? ?? Admin/              # Administrative interface
?   ??? ?? Scheduler/          # Main scheduling interface
?   ??? ?? Account/            # Authentication pages
?   ??? ?? Shared/             # Shared layouts and components
??? ?? Services/               # Business logic services
?   ??? SchedulerService.cs    # Core scheduling logic
?   ??? AuthenticationService.cs
?   ??? PrintTrackingService.cs
??? ?? wwwroot/                # Static files
?   ??? ?? css/               # Stylesheets
?   ??? ?? js/                # JavaScript files
?   ??? ?? lib/               # Third-party libraries
??? ?? ProjectNotes/          # Comprehensive documentation
??? ?? Migrations/            # Database migrations
??? Program.cs                # Application startup
??? appsettings.json         # Configuration
```

## ?? Core Modules

### **1. Scheduler Module**
- **Main Interface**: Visual timeline with machine rows
- **Job Management**: Add, edit, delete, and move jobs
- **Conflict Detection**: Automatic overlap prevention
- **Real-Time Updates**: Live grid updates without page refresh

### **2. Admin Module**
- **Dashboard**: Real-time statistics and system health
- **Jobs Management**: Complete CRUD with advanced filtering
- **Parts Management**: Material costs and specifications
- **Logs Viewer**: Comprehensive audit trail

### **3. Print Tracking Module**
- **Build Jobs**: Track actual print operations
- **Part Tracking**: Monitor individual parts through production
- **Delay Logging**: Record and analyze production delays
- **Quality Control**: Integration with inspection processes

### **4. Department Modules**
- **Printing**: SLS machine operation and monitoring
- **Coating**: Post-processing coating operations
- **EDM**: Electrical discharge machining
- **Machining**: Traditional machining operations
- **Quality Control**: Inspection and certification
- **Shipping**: Order fulfillment and logistics

## ?? Configuration

### **Database Configuration**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=scheduler.db"
  }
}
```

### **Authentication Settings**
- **Session Timeout**: 2 hours (configurable)
- **Auto-Extension**: On user activity
- **Security**: HTTP-only cookies with CSRF protection

### **Machine Configuration**
- **TI1**: TruPrint 3000 (250x250x300mm build volume)
- **TI2**: TruPrint 3000 (250x250x300mm build volume)  
- **INC**: Inconel specialist machine
- **Materials**: Ti-6Al-4V Grade 5, Ti-6Al-4V ELI, Inconel 718

## ?? Features by Role

### **?? Admin**
- Full system configuration and user management
- Database administration and backup tools
- System monitoring and health checks
- Complete audit trail access

### **?? Manager**
- Scheduling oversight and approval workflows
- Resource allocation and capacity planning
- Performance analytics and reporting
- Quality metrics and KPI monitoring

### **?? Scheduler**
- Job scheduling and timeline management
- Material planning and allocation
- Conflict resolution and optimization
- Customer order coordination

### **?? Operator**
- Machine operation and job execution
- Real-time status updates
- Quality checkpoints and inspections
- Production delay reporting

### **?? Specialists**
- Department-specific operation management
- Process parameter optimization
- Quality control and certification
- Specialized equipment operation

## ??? Development Tools

### **Error Logging System**
- **Client-Side**: Comprehensive JavaScript error tracking
- **Server-Side**: Detailed operation logging with IDs
- **Error Reports**: Structured debugging information
- **Issue Tracking**: Systematic bug documentation

### **Testing Framework**
- **Unit Tests**: Business logic validation
- **Integration Tests**: End-to-end workflows
- **Performance Tests**: Load and stress testing
- **Browser Testing**: Cross-browser compatibility

### **Design System**
- **CSS Variables**: Consistent color and spacing
- **Component Library**: Reusable UI components
- **Responsive Grid**: Mobile-first design approach
- **Accessibility**: WCAG AA compliance

## ?? Analytics & Reporting

### **Key Performance Indicators**
- **Machine Utilization**: Real-time and historical usage
- **Job Completion Rates**: Success metrics and trends
- **Material Usage**: Cost tracking and waste reduction
- **Quality Metrics**: Defect rates and improvement tracking

### **Real-Time Monitoring**
- **Machine Status**: Live operational status
- **Job Progress**: Current build status and timing
- **Material Levels**: Powder inventory tracking
- **System Health**: Database and service monitoring

## ?? Security Features

### **Authentication & Authorization**
- **Role-Based Access**: Granular permission system
- **Session Management**: Secure session handling
- **CSRF Protection**: Anti-forgery token validation
- **Input Validation**: Comprehensive data sanitization

### **Data Protection**
- **Audit Logging**: Complete change tracking
- **Database Security**: Parameterized queries
- **Error Handling**: Secure error reporting
- **Backup System**: Automated database backups

## ?? Browser Support

### **Supported Browsers**
- **Chrome**: Full feature support
- **Firefox**: Complete compatibility
- **Edge**: Microsoft Edge support
- **Safari**: WebKit compatibility
- **Mobile**: iOS and Android browsers

### **Responsive Design**
- **Desktop**: Full-featured interface (?1024px)
- **Tablet**: Touch-optimized layout (768-1023px)
- **Mobile**: Streamlined interface (?767px)

## ?? Documentation

### **Available Documentation**
- **[Site Structure Guide](docs/SITE_STRUCTURE.md)**: Complete application architecture
- **[Database Structure Guide](docs/DATABASE_STRUCTURE.md)**: Data model and relationships
- **[CSS Structure Guide](docs/CSS_STRUCTURE.md)**: Design system and styling
- **[Modification Guide](docs/MODIFICATION_GUIDE.md)**: Developer onboarding and customization

### **Project Notes**
- Comprehensive implementation documentation in `ProjectNotes/`
- Design system specifications and guidelines
- Testing reports and issue tracking
- Performance optimization records

## ?? Contributing

### **Development Workflow**
1. **Fork the repository** and create a feature branch
2. **Follow coding standards** and naming conventions
3. **Write tests** for new functionality
4. **Update documentation** for changes
5. **Submit pull request** with detailed description

### **Code Standards**
- **C#**: Follow Microsoft C# coding conventions
- **CSS**: Use BEM methodology and design tokens
- **JavaScript**: Modern ES6+ with error handling
- **Database**: Proper indexing and relationships

## ?? Support

### **Getting Help**
- **Documentation**: Check the comprehensive guides in `docs/`
- **Project Notes**: Review implementation details in `ProjectNotes/`
- **Error Logging**: Use built-in debugging tools
- **Issue Tracking**: Submit detailed bug reports

### **System Requirements**
- **Server**: Windows/Linux with .NET 8.0
- **Database**: SQLite (upgradeable to SQL Server)
- **Memory**: 4GB RAM minimum, 8GB recommended
- **Storage**: 10GB for application and database

---

## ?? Status: Production Ready

The OpCentrix SLS Print Job Scheduler is a **complete, professional-grade application** ready for manufacturing environments. It features enterprise-level design, comprehensive functionality, and robust error handling suitable for industrial use.

**Key Achievements:**
- ? **Complete Scheduler Implementation** with visual timeline
- ? **Professional Admin System** with full CRUD operations
- ? **Role-Based Security** with comprehensive authentication
- ? **Responsive Design** working on all devices
- ? **Enhanced Error Logging** for production debugging
- ? **Manufacturing Integration** with SLS-specific features

---

*Last Updated: December 2024*  
*Version: 2.0.0*  
*License: [Your License]*