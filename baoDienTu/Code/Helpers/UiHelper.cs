using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using baoDienTu.Models;

namespace baoDienTu.Helpers
{
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
            return VirtualPathUtility.ToAbsolute("~/NewsList.aspx?cat=" + HttpUtility.UrlEncode(category == null ? string.Empty : category.Slug));
        }

        public static string TagUrl(TagModel tag)
        {
            return VirtualPathUtility.ToAbsolute("~/NewsTag.aspx?tag=" + HttpUtility.UrlEncode(tag == null ? string.Empty : tag.Slug));
        }

        public static string PrintUrl(NewsModel news)
        {
            return VirtualPathUtility.ToAbsolute("~/Print.aspx?slug=" + HttpUtility.UrlEncode(news == null ? string.Empty : news.Slug));
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

        public static string ResolveImageUrl(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return string.Empty;
            }

            Uri uri;
            if (Uri.TryCreate(imagePath, UriKind.Absolute, out uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return imagePath;
            }

            if (imagePath.StartsWith("~/", StringComparison.Ordinal))
            {
                return VirtualPathUtility.ToAbsolute(imagePath);
            }

            if (imagePath.StartsWith("/", StringComparison.Ordinal))
            {
                return imagePath;
            }

            return VirtualPathUtility.ToAbsolute("~/" + imagePath.TrimStart('/'));
        }

        public static string ImageBlock(NewsModel news, string className)
        {
            var label = news == null || string.IsNullOrWhiteSpace(news.CatName) ? "Tin mới" : news.CatName;
            var title = news == null ? label : news.Title;
            var imageUrl = news == null ? string.Empty : ResolveImageUrl(news.Thumbnail);
            var img = string.IsNullOrWhiteSpace(imageUrl)
                ? string.Empty
                : "<img src=\"" + Attr(imageUrl) + "\" alt=\"" + Attr(title) + "\" onerror=\"this.remove();this.parentElement.classList.add('is-placeholder');\" />";
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
            var pages = PagingHelper.PageCount(total, pageSize);
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
