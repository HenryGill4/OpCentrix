# ??? System Architecture Documentation

This section contains foundational system architecture documentation including database design, authentication systems, and core configuration.

## ?? **CONTENTS**

### **Database Architecture**
- [`Database/`](Database/) - Complete database documentation
  - [`Database_Schema_Analysis_Complete.md`](Database/Database_Schema_Analysis_Complete.md) - Comprehensive schema analysis
  - [`Database_Quick_Reference.md`](Database/Database_Quick_Reference.md) - Quick reference guide
  - [`DATABASE_REFACTORING_SUCCESS.md`](Database/DATABASE_REFACTORING_SUCCESS.md) - Database refactoring completion
  - [`OpCentrix_Database_Schema_Complete.md`](Database/OpCentrix_Database_Schema_Complete.md) - Complete schema documentation

### **Authentication & Authorization**
- [`Authentication/`](Authentication/) - Security system documentation
  - [`AUTHENTICATION_SYSTEM_FIX_COMPLETE.md`](Authentication/AUTHENTICATION_SYSTEM_FIX_COMPLETE.md) - Authentication fixes
  - [`authorization-session-timeout-complete.md`](Authentication/authorization-session-timeout-complete.md) - Session management
  - [`Authorization-Policy-Fixes-and-System-Restoration-Complete.md`](Authentication/Authorization-Policy-Fixes-and-System-Restoration-Complete.md) - Policy fixes

### **System Configuration**
- [`System-Configuration/`](System-Configuration/) - Core system settings
  - [`Port-Configuration-Fix.md`](System-Configuration/Port-Configuration-Fix.md) - Port setup guide
  - [`Package-Installation-and-Fixes-Complete.md`](System-Configuration/Package-Installation-and-Fixes-Complete.md) - Package management

## ?? **ARCHITECTURE OVERVIEW**

### **??? Database Layer**
```
OpCentrix Database (SQLite)
??? Core Tables
?   ??? Users (Authentication)
?   ??? Jobs (Production scheduling)
?   ??? Parts (Manufacturing components)
?   ??? Machines (Equipment management)
??? Admin Tables
?   ??? BugReports (Quality tracking)
?   ??? AuditLogs (Change tracking)
?   ??? SystemSettings (Configuration)
??? Operational Tables
    ??? ProductionStages (Workflow)
    ??? EDMLogs (EDM operations)
    ??? MachineCapabilities (Equipment specs)
```

### **?? Authentication Architecture**
```
Authentication Flow
??? Cookie-Based Authentication
??? Role-Based Authorization
?   ??? Admin (Full access)
?   ??? Manager (Management functions)
?   ??? Supervisor (Department oversight)
?   ??? Operator (Production operations)
??? Session Management
?   ??? Timeout: 2 hours
?   ??? Warning: 5 minutes before timeout
?   ??? Extension: Available on user activity
??? Authorization Policies
    ??? AdminOnly (Admin exclusive)
    ??? SchedulerAccess (Production users)
    ??? SupervisorAccess (Management level)
```

### **?? Application Architecture**
```
ASP.NET Core 8.0 (Razor Pages)
??? Presentation Layer
?   ??? Razor Pages (/Pages/)
?   ??? Shared Components (/Pages/Shared/)
?   ??? Static Assets (/wwwroot/)
??? Business Logic Layer
?   ??? Services (/Services/)
?   ??? Domain Models (/Models/)
?   ??? ViewModels (/ViewModels/)
??? Data Access Layer
?   ??? Entity Framework Core
?   ??? Database Context (/Data/)
?   ??? Migrations (/Migrations/)
??? Infrastructure
    ??? Authentication & Authorization
    ??? Logging (Serilog)
    ??? Configuration Management
```

## ?? **KEY ARCHITECTURAL DECISIONS**

### **? Technology Choices**
- **Framework**: ASP.NET Core 8.0 Razor Pages
- **Database**: SQLite with Entity Framework Core
- **Authentication**: Cookie-based with role authorization
- **Frontend**: HTMX + Tailwind CSS + Bootstrap
- **Logging**: Serilog with structured logging
- **Testing**: xUnit with comprehensive integration tests

### **? Design Patterns**
- **Repository Pattern**: Service layer abstraction
- **Dependency Injection**: Built-in ASP.NET Core DI
- **MVC Pattern**: Razor Pages model
- **Domain-Driven Design**: Clear domain models
- **Command Query Separation**: Read/write operations separated

### **? Security Measures**
- **Authentication**: Secure cookie-based sessions
- **Authorization**: Role-based access control
- **CSRF Protection**: Anti-forgery tokens
- **Input Validation**: Comprehensive validation
- **SQL Injection Prevention**: Entity Framework parameterization
- **XSS Protection**: Razor encoding and CSP headers

## ?? **SYSTEM SPECIFICATIONS**

### **?? Technical Requirements**
- **.NET Version**: 8.0
- **C# Version**: 12.0
- **Database**: SQLite 5.0+
- **Web Server**: Kestrel (Development) / IIS (Production)
- **Browsers**: Modern browsers (Chrome, Firefox, Safari, Edge)

### **?? Performance Characteristics**
- **Startup Time**: < 5 seconds
- **Page Load Time**: < 2 seconds
- **Database Queries**: Optimized with indexes
- **Memory Usage**: < 100MB typical usage
- **Concurrent Users**: 50+ supported

### **?? Security Features**
- **Session Timeout**: 2 hours with warning
- **Password Requirements**: Configurable complexity
- **Account Lockout**: After failed attempts
- **Audit Logging**: All admin actions logged
- **Data Protection**: Sensitive data encrypted

## ?? **ARCHITECTURAL PRINCIPLES**

### **?? Design Principles**
1. **Single Responsibility**: Each class has one purpose
2. **Open/Closed**: Open for extension, closed for modification
3. **Dependency Inversion**: Depend on abstractions, not concretions
4. **DRY (Don't Repeat Yourself)**: Minimize code duplication
5. **YAGNI (You Aren't Gonna Need It)**: Build what's needed now

### **?? UI/UX Principles**
1. **Progressive Enhancement**: Works without JavaScript
2. **Mobile First**: Responsive design from start
3. **Accessibility**: WCAG compliant where possible
4. **Consistency**: Uniform design patterns
5. **Performance**: Fast loading and responsive

### **?? Development Principles**
1. **Test-Driven**: Comprehensive test coverage
2. **Documentation**: Self-documenting code with comments
3. **Version Control**: Clear commit messages and branching
4. **Code Review**: All changes reviewed
5. **Continuous Integration**: Automated build and test

## ?? **KNOWN LIMITATIONS & TRADE-OFFS**

### **?? Current Limitations**
- **Database**: SQLite suitable for small-medium deployments
- **Scalability**: Single-server deployment initially
- **Real-time**: Limited real-time features (polling-based)
- **Mobile**: Responsive but not native mobile app

### **?? Architectural Trade-offs**
- **SQLite vs SQL Server**: Chose simplicity over enterprise features
- **Razor Pages vs MVC**: Chose page-focused over controller-centric
- **Cookie Auth vs JWT**: Chose server-side sessions over stateless tokens
- **HTMX vs SPA**: Chose progressive enhancement over full SPA

## ?? **FUTURE ARCHITECTURE PLANS**

### **?? Near-term Enhancements (3-6 months)**
- **Caching Layer**: Redis for improved performance
- **Background Jobs**: Hangfire for async processing
- **API Enhancement**: RESTful API expansion
- **Real-time Features**: SignalR for live updates

### **?? Long-term Evolution (6+ months)**
- **Microservices**: Service decomposition for scaling
- **Cloud Deployment**: Azure/AWS deployment options
- **Database Scaling**: SQL Server/PostgreSQL migration path
- **Mobile Apps**: Native mobile applications

---

**?? Last Updated:** January 2025  
**?? Documents in Category:** 12 files  
**?? Current Status:** Stable and Production-Ready  

*The OpCentrix architecture provides a solid foundation for a manufacturing execution system with room for future growth.* ???