using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Pages
{
    public partial class Print : Page
    {
        protected string RenderPage()
        {
            var news = NewsBLL.GetDetail(Request.QueryString["slug"]);
            if (news == null)
            {
                return "<div class=\"empty-state\">Bài viết không tồn tại hoặc chưa được duyệt.</div>";
            }

            Page.Title = "In: " + news.Title;
            var builder = new StringBuilder();
            builder.Append("<article class=\"print-article\">");
            builder.Append("<h1>" + UiHelper.E(news.Title) + "</h1>");
            builder.Append("<p class=\"muted\">" + UiHelper.E(news.CatName) + " · " + UiHelper.E(UiHelper.Date(news.PublishedAt)) + " · " + UiHelper.E(news.AuthorName) + "</p>");
            builder.Append("<p class=\"article-summary\">" + UiHelper.E(news.Summary) + "</p>");
            builder.Append("<div class=\"article-body\">" + news.Content + "</div>");
            builder.Append("<script>window.addEventListener('load',function(){window.print();});</script>");
            builder.Append("</article>");
            return builder.ToString();
        }
    }
}
