using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Pages
{
    public partial class NewsList : Page
    {
        private const int PageSize = 9;

        protected void Page_Load(object sender, EventArgs e)
        {
            UiHelper.DisableBrowserCache();
        }

        protected string RenderPage()
        {
            try
            {
                var page = Math.Max(1, Convert.ToInt32(Request.QueryString["page"] ?? "1"));
                var catSlug = Request.QueryString["cat"];
                CategoryModel category = string.IsNullOrWhiteSpace(catSlug) ? null : CategoryService.GetBySlug(catSlug);
                int total;
                var items = NewsService.GetNewsList(category == null ? (int?)null : category.CatID, page, PageSize, out total);
                var title = category == null ? "Tất cả tin tức" : category.CatName;

                var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl\">");
                builder.Append("<div class=\"section-title\"><h1>" + UiHelper.E(title) + "</h1><span class=\"muted\">" + total + " bài viết</span></div>");

                if (items.Count == 0)
                {
                    builder.Append("<div class=\"empty-state\">Chưa có bài viết phù hợp.</div>");
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

                builder.Append(UiHelper.Pagination(page, PageSize, total, p => ResolveUrl("~/NewsList.aspx?cat=" + HttpUtility.UrlEncode(catSlug ?? string.Empty) + "&page=" + p)));
                builder.Append("</div></div>");
                return builder.ToString();
            }
            catch (Exception ex)
            {
                return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"app-alert danger\">Không thể tải danh sách tin: " + UiHelper.E(ex.Message) + "</div></div></div>";
            }
        }
    }
}
