using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApplication1.Helpers
{
    /// <summary>
    /// Kiểm tra đăng nhập dựa trên Session + (tuỳ chọn) role.
    /// Dùng: [CustomAuthorize] hoặc [CustomAuthorize(Roles="Admin")] hoặc [CustomAuthorize(Roles="Admin,Manager")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        /// <summary>
        /// Danh sách role phân tách bởi dấu phẩy: "Admin,Manager"
        /// </summary>
        public string? Roles { get; set; }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            // 1) Chưa đăng nhập
            var me = CustomAuthentication.GetUser(http);
            if (me == null)
            {
                HandleUnauthenticated(context);
                return;
            }

            // 2) Có yêu cầu role → kiểm tra
            var roles = ParseRoles(Roles);
            if (roles.Length > 0)
            {
                var ok = roles.Any(r => string.Equals(me.Role ?? "", r, StringComparison.OrdinalIgnoreCase));
                if (!ok)
                {
                    HandleForbidden(context);
                    return;
                }
            }
        }

        private static string[] ParseRoles(string? roles)
        {
            if (string.IsNullOrWhiteSpace(roles)) return Array.Empty<string>();
            return roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }

        private static void HandleUnauthenticated(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            // AJAX/API → trả 401 thay vì redirect
            if (IsAjax(http.Request) || WantsJson(http.Request))
            {
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
                return;
            }

            var returnUrl = CustomAuthentication.BuildReturnUrl(http);
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
        }

        private static void HandleForbidden(AuthorizationFilterContext context)
        {
            var http = context.HttpContext;

            if (IsAjax(http.Request) || WantsJson(http.Request))
            {
                context.Result = new JsonResult(new { message = "Forbidden" }) { StatusCode = StatusCodes.Status403Forbidden };
                return;
            }

            context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
        }

        private static bool IsAjax(HttpRequest req)
        {
            return string.Equals(req.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private static bool WantsJson(HttpRequest req)
        {
            return req.Headers.Accept.Any(a => a?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
