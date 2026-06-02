using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Pages
{
    public partial class NewsTag : Page
    {
        private const int PageSize = 9;

        protected string RenderPage()
        {
            var tagSlug = Request.QueryString["tag"];
            var tag = TagBLL.GetBySlug(tagSlug);
            var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl\">");
            if (tag == null)
            {
                builder.Append("<div class=\"empty-state\">Tag không tồn tại.</div></div></div>");
                return builder.ToString();
            }

            Page.Title = "Tag: " + tag.TagName;
            var page = PagingHelper.NormalizePage(Request.QueryString["page"]);
            int total;
            var items = NewsBLL.GetByTag(tagSlug, page, PageSize, out total);
            builder.Append("<div class=\"section-title\"><h1>Tag: " + UiHelper.E(tag.TagName) + "</h1><a href=\"" + ResolveUrl("~/Search.aspx") + "\">Tìm kiếm</a></div>");
            builder.Append("<div class=\"article-grid\">");
            foreach (var item in items)
            {
                builder.Append(UiHelper.ArticleCard(item, string.Empty));
            }
            builder.Append("</div>");
            builder.Append(UiHelper.Pagination(page, PageSize, total, p => ResolveUrl("~/NewsTag.aspx?tag=" + HttpUtility.UrlEncode(tagSlug) + "&page=" + p)));
            builder.Append("</div></div>");
            return builder.ToString();
        }
    }
}
