using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string RenderPage()
        {
            try
            {
                var featured = NewsService.GetFeatured(6);
                var latest = NewsService.GetLatest(9);
                var mostViewed = NewsService.GetMostViewed(6);
                var categories = CategoryService.GetCategories(true).Where(c => !c.ParentID.HasValue).Take(10).ToList();
                var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl\">");

                if (Request.QueryString["denied"] == "1")
                {
                    builder.Append("<div class=\"app-alert danger\">Bạn không có quyền truy cập khu vực vừa yêu cầu.</div>");
                }

                if (featured.Count == 0)
                {
                    builder.Append("<div class=\"empty-state\">Chưa có dữ liệu bài viết. Hãy chạy Database.sql trên SQL Express để nạp seed data.</div>");
                    builder.Append("</div></div>");
                    return builder.ToString();
                }

                var lead = featured[0];
                builder.Append("<section class=\"hero-grid\">");
                builder.Append("<article class=\"lead-story\"><a href=\"" + UiHelper.Attr(UiHelper.NewsUrl(lead.Slug)) + "\">" + UiHelper.ImageBlock(lead, string.Empty) + "</a>");
                builder.Append("<div class=\"lead-copy\"><div class=\"article-meta\"><span>" + UiHelper.E(lead.CatName) + "</span><span>" + UiHelper.E(UiHelper.Date(lead.PublishedAt)) + "</span></div>");
                builder.Append("<h1><a href=\"" + UiHelper.Attr(UiHelper.NewsUrl(lead.Slug)) + "\">" + UiHelper.E(lead.Title) + "</a></h1>");
                builder.Append("<p class=\"article-summary\">" + UiHelper.E(UiHelper.Excerpt(lead.Summary, 230)) + "</p>");
                builder.Append("<div class=\"article-foot\"><span>" + UiHelper.E(lead.AuthorName) + "</span><span>" + lead.ViewCount + " lượt xem</span></div></div></article>");

                builder.Append("<aside class=\"panel\"><div class=\"section-title\"><h2>Đọc nhiều</h2></div><div class=\"side-stack\">");
                foreach (var item in mostViewed)
                {
                    builder.Append(UiHelper.SmallArticle(item));
                }
                builder.Append("</div></aside></section>");

                builder.Append("<section><div class=\"section-title\"><h2>Tin mới nhất</h2><a href=\"" + ResolveUrl("~/NewsList.aspx") + "\">Xem tất cả</a></div><div class=\"article-grid\">");
                foreach (var item in latest)
                {
                    builder.Append(UiHelper.ArticleCard(item, string.Empty));
                }
                builder.Append("</div></section>");

                builder.Append("<section class=\"page-shell\"><div class=\"section-title\"><h2>Chuyên mục</h2></div><div class=\"article-grid\">");
                foreach (var category in categories)
                {
                    builder.Append("<a class=\"panel\" href=\"" + UiHelper.Attr(UiHelper.CategoryUrl(category)) + "\"><h3>" + UiHelper.E(category.CatName) + "</h3><p class=\"muted\">" + category.NewsCount + " bài viết đã xuất bản</p></a>");
                }
                builder.Append("</div></section>");

                builder.Append("</div></div>");
                return builder.ToString();
            }
            catch (Exception ex)
            {
                return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"app-alert danger\">Không thể tải dữ liệu trang chủ: " + UiHelper.E(ex.Message) + "</div></div></div>";
            }
        }
    }
}
