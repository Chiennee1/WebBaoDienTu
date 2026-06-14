using System.Web.Routing;
using Microsoft.AspNet.FriendlyUrls;

namespace baoDienTu
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            MapPublicPage(routes, "Home", string.Empty, "Default");
            MapPublicPage(routes, "Default", "Default");
            MapPublicPage(routes, "NewsList", "NewsList");
            MapPublicPage(routes, "NewsDetail", "NewsDetail");
            MapPublicPage(routes, "Search", "Search");
            MapPublicPage(routes, "Login", "Login");
            MapPublicPage(routes, "Logout", "Logout");
            MapPublicPage(routes, "Register", "Register");
            MapPublicPage(routes, "NewsletterSubscribe", "NewsletterSubscribe");
            MapPublicPage(routes, "NewsletterConfirm", "NewsletterConfirm");
            MapPublicPage(routes, "Unsubscribe", "Unsubscribe");
            MapPublicPage(routes, "NewsTag", "NewsTag");
            MapPublicPage(routes, "Print", "Print");
            MapPublicPage(routes, "About", "About");
            MapPublicPage(routes, "Contact", "Contact");
            MapPublicPage(routes, "Error", "Error");
            MapPublicPage(routes, "NotFound", "404");

            var settings = new FriendlyUrlSettings();
            settings.AutoRedirectMode = RedirectMode.Off;
            routes.EnableFriendlyUrls(settings);
        }

        private static void MapPublicPage(RouteCollection routes, string routeName, string urlName, string pageName = null)
        {
            pageName = pageName ?? urlName;
            routes.MapPageRoute(routeName, urlName, "~/Pages/" + pageName + ".aspx");

            if (!string.IsNullOrEmpty(urlName))
            {
                routes.MapPageRoute(routeName + "Aspx", urlName + ".aspx", "~/Pages/" + pageName + ".aspx");
            }
        }
    }
}
