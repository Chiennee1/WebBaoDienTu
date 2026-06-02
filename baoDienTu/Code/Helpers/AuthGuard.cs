using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using baoDienTu.Models;

namespace baoDienTu.Helpers
{
    public static class AuthGuard
    {
        public static bool IsAuthenticated
        {
            get { return HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["CurrentUserId"] != null; }
        }

        public static int CurrentUserId
        {
            get { return IsAuthenticated ? Convert.ToInt32(HttpContext.Current.Session["CurrentUserId"]) : 0; }
        }

        public static string CurrentFullName
        {
            get { return IsAuthenticated ? Convert.ToString(HttpContext.Current.Session["CurrentFullName"]) : string.Empty; }
        }

        public static string CurrentRoleName
        {
            get { return IsAuthenticated ? Convert.ToString(HttpContext.Current.Session["CurrentRoleName"]) : string.Empty; }
        }

        public static bool IsAdmin
        {
            get { return string.Equals(CurrentRoleName, "Admin", StringComparison.OrdinalIgnoreCase); }
        }

        public static bool IsEditor
        {
            get { return string.Equals(CurrentRoleName, "Editor", StringComparison.OrdinalIgnoreCase); }
        }

        public static void SignIn(UserModel user)
        {
            HttpContext.Current.Session["CurrentUserId"] = user.UserID;
            HttpContext.Current.Session["CurrentUsername"] = user.Username;
            HttpContext.Current.Session["CurrentFullName"] = user.FullName;
            HttpContext.Current.Session["CurrentRoleId"] = user.RoleID;
            HttpContext.Current.Session["CurrentRoleName"] = user.RoleName;
            FormsAuthentication.SetAuthCookie(user.Username, false);
        }

        public static void SignOut()
        {
            FormsAuthentication.SignOut();
            HttpContext.Current.Session.Clear();
            HttpContext.Current.Session.Abandon();
        }

        public static void RequireLogin(Page page)
        {
            if (!IsAuthenticated)
            {
                page.Response.Redirect("~/Login.aspx?returnUrl=" + HttpUtility.UrlEncode(page.Request.RawUrl), true);
            }
        }

        public static void RequireRole(Page page, params string[] roles)
        {
            RequireLogin(page);
            if (!roles.Any(r => string.Equals(r, CurrentRoleName, StringComparison.OrdinalIgnoreCase)))
            {
                page.Response.Redirect("~/Default.aspx?denied=1", true);
            }
        }
    }
}
