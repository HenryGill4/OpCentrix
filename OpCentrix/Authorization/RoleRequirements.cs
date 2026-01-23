using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpCentrix.Models;

namespace OpCentrix.Authorization
{
    public class RoleRequirementAttribute : TypeFilterAttribute
    {
        public RoleRequirementAttribute(params string[] roles) : base(typeof(RoleRequirementFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class RoleRequirementFilter : IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleRequirementFilter(string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToPageResult("/Account/Login", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            // FIXED: Check for consistent role claim - try both claim types
            var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                          user.FindFirst("Role")?.Value;
                          
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    // Specific role attributes for easy use
    public class AdminOnlyAttribute : RoleRequirementAttribute
    {
        public AdminOnlyAttribute() : base(UserRoles.Admin) { }
    }

    public class ManagerOrAdminAttribute : RoleRequirementAttribute
    {
        public ManagerOrAdminAttribute() : base(UserRoles.Admin, UserRoles.Manager) { }
    }

    public class SchedulerAccessAttribute : RoleRequirementAttribute
    {
        public SchedulerAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.Scheduler, UserRoles.Operator, UserRoles.PrintingSpecialist) { }
    }

    public class CoatingAccessAttribute : RoleRequirementAttribute
    {
        public CoatingAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.CoatingSpecialist) { }
    }

    public class ShippingAccessAttribute : RoleRequirementAttribute
    {
        public ShippingAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.ShippingSpecialist) { }
    }

    public class EDMAccessAttribute : RoleRequirementAttribute
    {
        public EDMAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.EDMSpecialist) { }
    }

    public class MachiningAccessAttribute : RoleRequirementAttribute
    {
        public MachiningAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.MachiningSpecialist) { }
    }

    public class QCAccessAttribute : RoleRequirementAttribute
    {
        public QCAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.QCSpecialist) { }
    }

    public class MediaAccessAttribute : RoleRequirementAttribute
    {
        public MediaAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.MediaSpecialist) { }
    }

    // FIXED: Ensure PrintingAccess includes Admin and all appropriate roles
    public class PrintingAccessAttribute : RoleRequirementAttribute
    {
        public PrintingAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.PrintingSpecialist, UserRoles.Operator) { }
    }

    public class AnalyticsAccessAttribute : RoleRequirementAttribute
    {
        public AnalyticsAccessAttribute() : base(UserRoles.Admin, UserRoles.Manager, UserRoles.Analyst) { }
    }

    /// <summary>
    /// Requires Admin role access - FIXED to use consistent claim checking
    /// </summary>
    public class AdminAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToPageResult("/Account/Login");
                return;
            }

            // FIXED: Check both claim types consistently
            var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                          user.FindFirst("Role")?.Value ?? "";
                          
            if (userRole != "Admin")
            {
                context.Result = new RedirectToPageResult("/Account/AccessDenied");
            }
        }
    }

    /// <summary>
    /// Requires Operator role access (Admin, Manager, Scheduler, Operator, PrintingSpecialist) - UPDATED for print tracking
    /// </summary>
    public class OperatorAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToPageResult("/Account/Login");
                return;
            }

            // FIXED: Check both claim types consistently
            var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                          user.FindFirst("Role")?.Value ?? "";
            var allowedRoles = new[] { "Admin", "Manager", "Scheduler", "Operator", "PrintingSpecialist" };
            
            if (!allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToPageResult("/Account/AccessDenied");
            }
        }
    }

    /// <summary>
    /// Requires Manager level access (Admin, Manager) - FIXED with consistent claim checking
    /// </summary>
    public class ManagerAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToPageResult("/Account/Login");
                return;
            }

            // FIXED: Check both claim types consistently
            var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                          user.FindFirst("Role")?.Value ?? "";
            var allowedRoles = new[] { "Admin", "Manager" };
            
            if (!allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToPageResult("/Account/AccessDenied");
            }
        }
    }

    /// <summary>
    /// CRITICAL FIX: Enhanced Print Tracking access with comprehensive error handling
    /// Requires Print Tracking access (Admin, Manager, Operator, PrintingSpecialist)
    /// </summary>
    public class PrintTrackingAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var user = context.HttpContext.User;
                var logger = context.HttpContext.RequestServices
                    .GetService<Microsoft.Extensions.Logging.ILogger<PrintTrackingAccessAttribute>>();

                if (!user.Identity?.IsAuthenticated == true)
                {
                    logger?.LogWarning("PrintTracking access denied - user not authenticated");
                    context.Result = new RedirectToPageResult("/Account/Login", new { returnUrl = context.HttpContext.Request.Path });
                    return;
                }

                // ENHANCED: Get user details for debugging
                var userName = user.Identity?.Name ?? "Unknown";
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
                
                // FIXED: Check both claim types consistently with enhanced logging
                var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                              user.FindFirst("Role")?.Value ?? "";

                logger?.LogInformation("PrintTracking access check - User: {UserName} (ID: {UserId}), Role: {UserRole}", 
                    userName, userIdClaim, userRole);

                var allowedRoles = new[] { "Admin", "Manager", "Operator", "PrintingSpecialist" };
                
                if (string.IsNullOrEmpty(userRole))
                {
                    logger?.LogError("PrintTracking access denied - no role claim found for user {UserName}", userName);
                    context.Result = new RedirectToPageResult("/Account/AccessDenied");
                    return;
                }
                
                if (!allowedRoles.Contains(userRole))
                {
                    logger?.LogWarning("PrintTracking access denied - user {UserName} has role '{UserRole}' which is not in allowed roles: {AllowedRoles}", 
                        userName, userRole, string.Join(", ", allowedRoles));
                    context.Result = new RedirectToPageResult("/Account/AccessDenied");
                    return;
                }

                logger?.LogInformation("PrintTracking access granted for user {UserName} with role {UserRole}", userName, userRole);
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices
                    .GetService<Microsoft.Extensions.Logging.ILogger<PrintTrackingAccessAttribute>>();
                    
                logger?.LogError(ex, "Error in PrintTrackingAccessAttribute authorization");
                
                // Fail safely by denying access
                context.Result = new RedirectToPageResult("/Account/AccessDenied");
            }
        }
    }

    /// <summary>
    /// Laser Engraving access for firearms & suppressor serialization
    /// Requires specialized authorization due to ATF compliance requirements
    /// </summary>
    public class LaserEngravingAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            try
            {
                var user = context.HttpContext.User;
                var logger = context.HttpContext.RequestServices
                    .GetService<Microsoft.Extensions.Logging.ILogger<LaserEngravingAccessAttribute>>();

                if (!user.Identity?.IsAuthenticated == true)
                {
                    logger?.LogWarning("LaserEngraving access denied - user not authenticated");
                    context.Result = new RedirectToPageResult("/Account/Login", new { returnUrl = context.HttpContext.Request.Path });
                    return;
                }

                var userName = user.Identity?.Name ?? "Unknown";
                var userIdClaim = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
                
                var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                              user.FindFirst("Role")?.Value ?? "";

                logger?.LogInformation("LaserEngraving access check - User: {UserName} (ID: {UserId}), Role: {UserRole}", 
                    userName, userIdClaim, userRole);

                // Restricted access - only Admin, Manager, and specialized operators
                var allowedRoles = new[] { "Admin", "Manager", "LaserEngravingSpecialist", "BTSpecialist" };
                
                if (string.IsNullOrEmpty(userRole))
                {
                    logger?.LogError("LaserEngraving access denied - no role claim found for user {UserName}", userName);
                    context.Result = new RedirectToPageResult("/Account/AccessDenied");
                    return;
                }
                
                if (!allowedRoles.Contains(userRole))
                {
                    logger?.LogWarning("LaserEngraving access denied - user {UserName} has role '{UserRole}' which is not in allowed roles: {AllowedRoles}", 
                        userName, userRole, string.Join(", ", allowedRoles));
                    context.Result = new RedirectToPageResult("/Account/AccessDenied");
                    return;
                }

                logger?.LogInformation("LaserEngraving access granted for user {UserName} with role {UserRole} - ATF compliance required", userName, userRole);
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices
                    .GetService<Microsoft.Extensions.Logging.ILogger<LaserEngravingAccessAttribute>>();
                    
                logger?.LogError(ex, "Error in LaserEngravingAccessAttribute authorization");
                
                // Fail safely by denying access
                context.Result = new RedirectToPageResult("/Account/AccessDenied");
            }
        }
    }
}