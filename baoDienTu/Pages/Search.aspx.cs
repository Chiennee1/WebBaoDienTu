using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Pages
{
    public partial class Search : Page
    {
        private const int PageSize = 9;

        protected string RenderPage()
        {
            var keyword = Request.QueryString["q"] ?? string.Empty;
            var page = Math.Max(1, Convert.ToInt32(Request.QueryString["page"] ?? "1"));
            var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl\">");
            builder.Append("<div class=\"section-title\"><h1>Tìm kiếm</h1></div>");
            builder.Append("<div class=\"form-panel\" style=\"margin-bottom:20px\"><div class=\"global-search\" style=\"width:100%\"><input type=\"search\" id=\"searchPageInput\" value=\"" + UiHelper.Attr(keyword) + "\" placeholder=\"Nhập từ khóa...\" /><button type=\"button\" onclick=\"location.href='Search.aspx?q='+encodeURIComponent(document.getElementById('searchPageInput').value)\">Tìm</button></div></div>");

            if (string.IsNullOrWhiteSpace(keyword))
            {
                builder.Append("<div class=\"empty-state\">Nhập từ khóa để tìm bài viết.</div></div></div>");
                return builder.ToString();
            }

            try
            {
                int total;
                var items = NewsService.Search(keyword, page, PageSize, out total);
                builder.Append("<p class=\"muted\">Tìm thấy " + total + " kết quả cho <strong>" + UiHelper.E(keyword) + "</strong>.</p>");
                if (items.Count == 0)
                {
                    builder.Append("<div class=\"empty-state\">Không có bài viết phù hợp.</div>");
                }
                else
                {
                    builder.Append("<div class=\"article-grid\">");
                    foreach (var item in items)
                    {
                        builder.Append(UiHelper.ArticleCard(item, string.Empty));
                    }
                    builder.Append("</div>");
                }
                builder.Append(UiHelper.Pagination(page, PageSize, total, p => ResolveUrl("~/Search.aspx?q=" + HttpUtility.UrlEncode(keyword) + "&page=" + p)));
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tìm kiếm: " + UiHelper.E(ex.Message) + "</div>");
            }

            builder.Append("</div></div>");
            return builder.ToString();
        }
    }
}
