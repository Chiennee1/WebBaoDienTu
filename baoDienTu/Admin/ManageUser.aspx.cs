using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class ManageUser : Page
    {
        private const int PageSize = 15;
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
            if (Request.HttpMethod == "POST" && !string.IsNullOrWhiteSpace(Request.Form["toggleUser"]))
            {
                try
                {
                    var userId = Convert.ToInt32(Request.Form["toggleUser"]);
                    int total;
                    var user = UserService.GetUsers(1, 1000, null, out total).Find(u => u.UserID == userId);
                    if (user != null && user.UserID != AuthGuard.CurrentUserId)
                    {
                        UserService.SetActive(user.UserID, !user.IsActive);
                        _result = OperationResult.Ok("Đã cập nhật trạng thái người dùng.");
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
                var page = Math.Max(1, Convert.ToInt32(Request.QueryString["page"] ?? "1"));
                var keyword = Request.QueryString["q"];
                int total;
                var users = UserService.GetUsers(page, PageSize, keyword, out total);
                builder.Append("<div class=\"form-panel\" style=\"margin-bottom:16px\"><div class=\"global-search\" style=\"width:100%\"><input id=\"userSearch\" value=\"" + UiHelper.Attr(keyword) + "\" placeholder=\"Tìm theo tên hoặc username\" /><button type=\"button\" onclick=\"location.href='ManageUser.aspx?q='+encodeURIComponent(document.getElementById('userSearch').value)\">Tìm</button></div></div>");
                builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Tài khoản</th><th>Email</th><th>Vai trò</th><th>Trạng thái</th><th>Lần đăng nhập</th><th>Thao tác</th></tr></thead><tbody>");
                foreach (var user in users)
                {
                    builder.Append("<tr><td><strong>" + UiHelper.E(user.FullName) + "</strong><br/><span class=\"muted\">" + UiHelper.E(user.Username) + "</span></td><td>" + UiHelper.E(user.Email) + "</td><td>" + UiHelper.E(user.RoleName) + "</td><td>" + (user.IsActive ? "Hoạt động" : "Khóa") + "</td><td>" + UiHelper.E(UiHelper.Date(user.LastLogin)) + "</td><td>");
                    if (user.UserID != AuthGuard.CurrentUserId)
                    {
                        builder.Append("<button class=\"btn-danger\" name=\"toggleUser\" value=\"" + user.UserID + "\" type=\"submit\">" + (user.IsActive ? "Khóa" : "Mở") + "</button>");
                    }
                    builder.Append("</td></tr>");
                }
                builder.Append("</tbody></table></div>");
                builder.Append(UiHelper.Pagination(page, PageSize, total, p => ResolveUrl("~/Admin/ManageUser.aspx?q=" + HttpUtility.UrlEncode(keyword) + "&page=" + p)));
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải người dùng: " + UiHelper.E(ex.Message) + "</div>");
            }
            return AdminUiHelper.Layout("Quản lý người dùng", builder.ToString());
        }
    }
}
