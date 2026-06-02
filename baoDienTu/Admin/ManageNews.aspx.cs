using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class ManageNews : Page
    {
        private const int PageSize = 12;
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin", "Editor");
            HandleActions();
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder();
            builder.Append(UiHelper.Alert(_result));
            builder.Append("<div class=\"btn-row\" style=\"margin-bottom:16px\"><a class=\"btn-main\" href=\"AddEditNews.aspx\">Viết bài mới</a>");
            builder.Append("<a class=\"btn-soft\" href=\"ManageNews.aspx\">Tất cả</a><a class=\"btn-soft\" href=\"ManageNews.aspx?status=1\">Chờ duyệt</a><a class=\"btn-soft\" href=\"ManageNews.aspx?status=2\">Đã duyệt</a><a class=\"btn-soft\" href=\"ManageNews.aspx?status=3\">Từ chối</a></div>");

            try
            {
                var page = Math.Max(1, Convert.ToInt32(Request.QueryString["page"] ?? "1"));
                byte? status = string.IsNullOrWhiteSpace(Request.QueryString["status"]) ? (byte?)null : Convert.ToByte(Request.QueryString["status"]);
                int total;
                var items = NewsService.GetAdminNews(status, AuthGuard.IsAdmin ? (int?)null : AuthGuard.CurrentUserId, null, Request.QueryString["q"], page, PageSize, out total);
                builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Tiêu đề</th><th>Chuyên mục</th><th>Tác giả</th><th>Trạng thái</th><th>Lượt xem</th><th>Thao tác</th></tr></thead><tbody>");
                foreach (var item in items)
                {
                    builder.Append("<tr><td><strong>" + UiHelper.E(item.Title) + "</strong><br/><span class=\"muted\">" + UiHelper.E(item.Slug) + "</span></td>");
                    builder.Append("<td>" + UiHelper.E(item.CatName) + "</td><td>" + UiHelper.E(item.AuthorName) + "</td><td>" + UiHelper.StatusBadge(item.Status) + "</td><td>" + item.ViewCount + "</td><td>");
                    builder.Append("<a class=\"btn-soft\" href=\"AddEditNews.aspx?id=" + item.NewsID + "\">Sửa</a> ");
                    if (AuthGuard.IsAdmin)
                    {
                        builder.Append("<button class=\"btn-soft\" name=\"approve\" value=\"" + item.NewsID + "\" type=\"submit\">Duyệt</button> ");
                        builder.Append("<button class=\"btn-danger\" name=\"reject\" value=\"" + item.NewsID + "\" type=\"submit\">Từ chối</button> ");
                    }
                    builder.Append("<button class=\"btn-danger\" name=\"delete\" value=\"" + item.NewsID + "\" type=\"submit\" onclick=\"return confirm('Xóa bài viết này?')\">Xóa</button>");
                    builder.Append("</td></tr>");
                }
                builder.Append("</tbody></table></div>");
                builder.Append(UiHelper.Pagination(page, PageSize, total, p => ResolveUrl("~/Admin/ManageNews.aspx?status=" + HttpUtility.UrlEncode(Request.QueryString["status"]) + "&page=" + p)));
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải bài viết: " + UiHelper.E(ex.Message) + "</div>");
            }

            return AdminUiHelper.Layout("Quản lý bài viết", builder.ToString());
        }

        private void HandleActions()
        {
            if (Request.HttpMethod != "POST")
            {
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["approve"]) && AuthGuard.IsAdmin)
                {
                    NewsService.ApproveNews(Convert.ToInt32(Request.Form["approve"]), AuthGuard.CurrentUserId, true, null);
                    _result = OperationResult.Ok("Đã duyệt bài viết.");
                }
                else if (!string.IsNullOrWhiteSpace(Request.Form["reject"]) && AuthGuard.IsAdmin)
                {
                    NewsService.ApproveNews(Convert.ToInt32(Request.Form["reject"]), AuthGuard.CurrentUserId, false, "Bài viết cần chỉnh sửa trước khi xuất bản.");
                    _result = OperationResult.Ok("Đã từ chối bài viết.");
                }
                else if (!string.IsNullOrWhiteSpace(Request.Form["delete"]))
                {
                    var id = Convert.ToInt32(Request.Form["delete"]);
                    var news = NewsService.GetById(id);
                    if (news != null && (AuthGuard.IsAdmin || news.AuthorID == AuthGuard.CurrentUserId))
                    {
                        NewsService.DeleteNews(id);
                        _result = OperationResult.Ok("Đã xóa bài viết.");
                    }
                }
            }
            catch (Exception ex)
            {
                _result = OperationResult.Fail(ex.Message);
            }
        }
    }
}
