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
    /// Requires Print Tracking access (Admin, Manager, Operator, PrintingSpecialist) - FIXED for print tracking pages
    /// </summary>
    public class PrintTrackingAccessAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated == true)
            {
                context.Result = new RedirectToPageResult("/Account/Login");
                return;
            }

            // FIXED: Check both claim types consistently and ensure Admin has access
            var userRole = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ??
                          user.FindFirst("Role")?.Value ?? "";
            var allowedRoles = new[] { "Admin", "Manager", "Operator", "PrintingSpecialist" };
            
            if (!allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToPageResult("/Account/AccessDenied");
            }
        }
    }
}