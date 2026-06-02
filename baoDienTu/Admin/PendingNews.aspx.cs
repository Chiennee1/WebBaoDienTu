using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class PendingNews : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
            if (Request.HttpMethod == "POST")
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(Request.Form["approve"]))
                    {
                        NewsService.ApproveNews(Convert.ToInt32(Request.Form["approve"]), AuthGuard.CurrentUserId, true, null);
                        _result = OperationResult.Ok("Đã duyệt bài.");
                    }
                    if (!string.IsNullOrWhiteSpace(Request.Form["reject"]))
                    {
                        NewsService.ApproveNews(Convert.ToInt32(Request.Form["reject"]), AuthGuard.CurrentUserId, false, "Không đạt yêu cầu biên tập.");
                        _result = OperationResult.Ok("Đã từ chối bài.");
                    }
                }
                catch (Exception ex)
                {
                    _result = OperationResult.Fail(ex.Message);
                }
            }
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder(UiHelper.Alert(_result));
            try
            {
                int total;
                var items = NewsService.GetAdminNews(1, null, null, null, 1, 100, out total);
                if (items.Count == 0)
                {
                    builder.Append("<div class=\"empty-state\">Không có bài viết chờ duyệt.</div>");
                }
                else
                {
                    builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Bài viết</th><th>Tác giả</th><th>Chuyên mục</th><th>Thao tác</th></tr></thead><tbody>");
                    foreach (var item in items)
                    {
                        builder.Append("<tr><td><strong>" + UiHelper.E(item.Title) + "</strong><br/><span class=\"muted\">" + UiHelper.E(UiHelper.Excerpt(item.Summary, 110)) + "</span></td><td>" + UiHelper.E(item.AuthorName) + "</td><td>" + UiHelper.E(item.CatName) + "</td><td><a class=\"btn-soft\" href=\"AddEditNews.aspx?id=" + item.NewsID + "\">Xem/Sửa</a> <button class=\"btn-main\" name=\"approve\" value=\"" + item.NewsID + "\" type=\"submit\">Duyệt</button> <button class=\"btn-danger\" name=\"reject\" value=\"" + item.NewsID + "\" type=\"submit\">Từ chối</button></td></tr>");
                    }
                    builder.Append("</tbody></table></div>");
                }
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải danh sách chờ duyệt: " + UiHelper.E(ex.Message) + "</div>");
            }
            return AdminUiHelper.Layout("Duyệt bài", builder.ToString());
        }
    }
}
