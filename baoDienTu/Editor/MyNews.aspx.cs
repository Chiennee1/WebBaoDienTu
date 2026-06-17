using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Editor
{
    public partial class MyNews : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin", "Editor");
        }

        protected string RenderPage()
        {
            int total;
            var items = NewsBLL.GetAdminNews(null, AuthGuard.IsAdmin ? (int?)null : AuthGuard.CurrentUserId, null, null, 1, 100, out total);
            var builder = new StringBuilder("<div class=\"btn-row\" style=\"margin-bottom:16px\"><a class=\"btn-main\" href=\"WriteNews.aspx\">Viết bài mới</a></div>");
            builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Tiêu đề</th><th>Chuyên mục</th><th>Trạng thái</th><th>Lượt xem</th><th>Thao tác</th></tr></thead><tbody>");
            foreach (var item in items)
            {
                builder.Append("<tr><td><strong>" + UiHelper.E(item.Title) + "</strong><br/><span class=\"muted\">" + UiHelper.E(item.Slug) + "</span></td><td>" + UiHelper.E(item.CatName) + "</td><td>" + UiHelper.StatusBadge(item.Status) + (item.Status == 3 && !string.IsNullOrWhiteSpace(item.RejectReason) ? "<br/><span style=\"color:#b45309;font-size:0.85rem;display:block;margin-top:4px\">📋 " + UiHelper.E(item.RejectReason) + "</span>" : string.Empty) + "</td><td>" + item.ViewCount + "</td><td><a class=\"btn-soft\" href=\"EditNews.aspx?id=" + item.NewsID + "\">Sửa</a></td></tr>");

            }
            builder.Append("</tbody></table></div>");
            return AdminUiHelper.Layout("Bài viết của tôi", builder.ToString());
        }
    }
}
