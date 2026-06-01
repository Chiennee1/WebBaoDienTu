using System;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using baoDienTu.Models;

namespace baoDienTu.Helpers
{
    public static class PasswordHasher
    {
        public static string CreateSalt()
        {
            var bytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        public static string Hash(string salt, string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes((salt ?? string.Empty) + (password ?? string.Empty)));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public static class SlugHelper
    {
        public static string Generate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "bai-viet";
            }

            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(ch == 'đ' ? 'd' : ch);
                }
            }

            var slug = Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"[^a-z0-9\s-]", string.Empty);
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
            return string.IsNullOrEmpty(slug) ? "bai-viet" : slug;
        }
    }

    public static class AuthGuard
    {
        public static bool IsAuthenticated
        {
            get { return HttpContext.Current != null && HttpContext.Current.Session["CurrentUserId"] != null; }
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
        }

        public static void SignOut()
        {
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

    public static class UiHelper
    {
        public static string E(object value)
        {
            return HttpUtility.HtmlEncode(value == null ? string.Empty : Convert.ToString(value));
        }

        public static string Attr(object value)
        {
            return HttpUtility.HtmlAttributeEncode(value == null ? string.Empty : Convert.ToString(value));
        }

        public static string NewsUrl(string slug)
        {
            return VirtualPathUtility.ToAbsolute("~/NewsDetail.aspx?slug=" + HttpUtility.UrlEncode(slug ?? string.Empty));
        }

        public static string CategoryUrl(CategoryModel category)
        {
            return VirtualPathUtility.ToAbsolute("~/NewsList.aspx?cat=" + HttpUtility.UrlEncode(category.Slug));
        }

        public static string Date(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) : "Chưa xuất bản";
        }

        public static string ShortDate(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty;
        }

        public static string Excerpt(string value, int length)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var clean = Regex.Replace(HttpUtility.HtmlDecode(value), "<.*?>", string.Empty).Trim();
            return clean.Length <= length ? clean : clean.Substring(0, length).Trim() + "...";
        }

        public static string ImageBlock(NewsModel news, string className)
        {
            var label = string.IsNullOrWhiteSpace(news.CatName) ? "Tin mới" : news.CatName;
            var img = string.IsNullOrWhiteSpace(news.Thumbnail)
                ? string.Empty
                : "<img src=\"" + Attr(VirtualPathUtility.ToAbsolute(news.Thumbnail)) + "\" alt=\"" + Attr(news.Title) + "\" onerror=\"this.remove();this.parentElement.classList.add('is-placeholder');\" />";
            return "<div class=\"news-image " + Attr(className) + "\" data-label=\"" + Attr(label) + "\">" + img + "</div>";
        }

        public static string ArticleCard(NewsModel news, string className)
        {
            var builder = new StringBuilder();
            builder.Append("<article class=\"article-card " + Attr(className) + "\">");
            builder.Append("<a class=\"article-image-link\" href=\"" + Attr(NewsUrl(news.Slug)) + "\">" + ImageBlock(news, string.Empty) + "</a>");
            builder.Append("<div class=\"article-card-body\">");
            builder.Append("<div class=\"article-meta\"><span>" + E(news.CatName) + "</span><span>" + E(ShortDate(news.PublishedAt)) + "</span></div>");
            builder.Append("<h3><a href=\"" + Attr(NewsUrl(news.Slug)) + "\">" + E(news.Title) + "</a></h3>");
            builder.Append("<p>" + E(Excerpt(news.Summary, 145)) + "</p>");
            builder.Append("<div class=\"article-foot\"><span>" + E(news.AuthorName) + "</span><span>" + news.ViewCount + " lượt xem</span></div>");
            builder.Append("</div></article>");
            return builder.ToString();
        }

        public static string SmallArticle(NewsModel news)
        {
            return "<article class=\"small-article\"><a href=\"" + Attr(NewsUrl(news.Slug)) + "\">" +
                   ImageBlock(news, "mini") + "</a><div><div class=\"article-meta\"><span>" + E(news.CatName) +
                   "</span></div><h4><a href=\"" + Attr(NewsUrl(news.Slug)) + "\">" + E(news.Title) +
                   "</a></h4><span class=\"muted\">" + news.ViewCount + " lượt xem</span></div></article>";
        }

        public static string StatusBadge(byte status)
        {
            if (status == 2) return "<span class=\"status-badge status-approved\">Đã duyệt</span>";
            if (status == 1) return "<span class=\"status-badge status-pending\">Chờ duyệt</span>";
            if (status == 3) return "<span class=\"status-badge status-rejected\">Từ chối</span>";
            return "<span class=\"status-badge status-draft\">Nháp</span>";
        }

        public static string Alert(OperationResult result)
        {
            if (result == null || string.IsNullOrWhiteSpace(result.Message))
            {
                return string.Empty;
            }
            return "<div class=\"app-alert " + (result.Success ? "success" : "danger") + "\">" + E(result.Message) + "</div>";
        }

        public static string Pagination(int page, int pageSize, int total, Func<int, string> urlBuilder)
        {
            var pages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
            if (pages <= 1)
            {
                return string.Empty;
            }

            var builder = new StringBuilder("<nav class=\"pagination-wrap\" aria-label=\"Phân trang\">");
            for (var i = 1; i <= pages; i++)
            {
                builder.Append("<a class=\"" + (i == page ? "active" : string.Empty) + "\" href=\"" + Attr(urlBuilder(i)) + "\">" + i + "</a>");
            }
            builder.Append("</nav>");
            return builder.ToString();
        }
    }
}
