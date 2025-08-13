using WebApplication1.ViewModel;
using WebApplication1.Helpers;

namespace WebApplication1.Helpers
{
    public static class CustomAuthentication
    {
        public const string SessionKey = "CURRENT_USER";

        public static void SignIn(HttpContext http, UserSessionVm user, bool isPersistent = false)
        {
            // isPersistent: nếu muốn tự làm cookie "nhớ tôi" riêng thì xử lý thêm ở đây
            http.Session.Set(SessionKey, user);
        }

        public static UserSessionVm? GetUser(HttpContext http)
        {
            return http.Session.Get<UserSessionVm>(SessionKey);
        }

        public static bool IsAuthenticated(HttpContext http)
        {
            return GetUser(http) != null;
        }

        public static bool IsInRole(HttpContext http, params string[] roles)
        {
            var me = GetUser(http);
            if (me == null || roles == null || roles.Length == 0) return false;

            foreach (var r in roles)
            {
                if (!string.IsNullOrWhiteSpace(r) &&
                    string.Equals(me.Role ?? "", r, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public static void SignOut(HttpContext http)
        {
            http.Session.Remove(SessionKey);
            // hoặc http.Session.Clear();
        }

        public static string BuildReturnUrl(HttpContext http)
        {
            return http.Request.Path + http.Request.QueryString;
        }
    }
}
