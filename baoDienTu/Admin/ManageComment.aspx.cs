using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class ManageComment : Page
    {
        private const int PageSize = 15;
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
            HandlePost();
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder(UiHelper.Alert(_result));
            builder.Append("<div class=\"btn-row\" style=\"margin-bottom:16px\"><a class=\"btn-soft\" href=\"ManageComment.aspx\">Tất cả</a><a class=\"btn-soft\" href=\"ManageComment.aspx?approved=0\">Chờ duyệt</a><a class=\"btn-soft\" href=\"ManageComment.aspx?approved=1\">Đã duyệt</a></div>");
            try
            {
                var page = Math.Max(1, Convert.ToInt32(Request.QueryString["page"] ?? "1"));
                bool? approved = null;
                if (Request.QueryString["approved"] == "0") approved = false;
                if (Request.QueryString["approved"] == "1") approved = true;
                int total;
                var comments = CommentService.GetAdminComments(approved, page, PageSize, out total);
                builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Bình luận</th><th>Bài viết</th><th>Người gửi</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody>");
                foreach (var comment in comments)
                {
                    builder.Append("<tr><td>" + UiHelper.E(comment.Content) + "<br/><span class=\"muted\">" + UiHelper.E(UiHelper.Date(comment.CreatedAt)) + "</span></td><td><a href=\"../NewsDetail.aspx?slug=" + HttpUtility.UrlEncode(comment.NewsSlug) + "\">" + UiHelper.E(comment.NewsTitle) + "</a></td><td>" + UiHelper.E(comment.DisplayName) + "<br/><span class=\"muted\">" + UiHelper.E(comment.DisplayEmail) + "</span></td><td>" + (comment.IsApproved ? "Đã duyệt" : "Chờ duyệt") + "</td><td>");
                    if (!comment.IsApproved)
                    {
                        builder.Append("<button class=\"btn-main\" name=\"approveComment\" value=\"" + comment.CmtID + "\" type=\"submit\">Duyệt</button> ");
                    }
                    builder.Append("<button class=\"btn-soft\" name=\"hideComment\" value=\"" + comment.CmtID + "\" type=\"submit\">Ẩn</button> <button class=\"btn-danger\" name=\"deleteComment\" value=\"" + comment.CmtID + "\" type=\"submit\" onclick=\"return confirm('Xóa bình luận này?')\">Xóa</button></td></tr>");
                }
                builder.Append("</tbody></table></div>");
                builder.Append(UiHelper.Pagination(page, PageSize, total, p => ResolveUrl("~/Admin/ManageComment.aspx?approved=" + HttpUtility.UrlEncode(Request.QueryString["approved"]) + "&page=" + p)));
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải bình luận: " + UiHelper.E(ex.Message) + "</div>");
            }
            return AdminUiHelper.Layout("Quản lý bình luận", builder.ToString());
        }

        private void HandlePost()
        {
            if (Request.HttpMethod != "POST")
            {
                return;
            }
            try
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["approveComment"]))
                {
                    CommentService.Approve(Convert.ToInt32(Request.Form["approveComment"]), true);
                    _result = OperationResult.Ok("Đã duyệt bình luận.");
                }
                else if (!string.IsNullOrWhiteSpace(Request.Form["hideComment"]))
                {
                    CommentService.Approve(Convert.ToInt32(Request.Form["hideComment"]), false);
                    _result = OperationResult.Ok("Đã ẩn bình luận.");
                }
                else if (!string.IsNullOrWhiteSpace(Request.Form["deleteComment"]))
                {
                    CommentService.Delete(Convert.ToInt32(Request.Form["deleteComment"]));
                    _result = OperationResult.Ok("Đã xóa bình luận.");
                }
            }
            catch (Exception ex)
            {
                _result = OperationResult.Fail(ex.Message);
            }
        }
    }
}
