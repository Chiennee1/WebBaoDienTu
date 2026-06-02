using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Admin
{
    public partial class ManageNewsletter : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder();
            try
            {
                var subscribers = NewsletterBLL.GetAll();
                builder.Append("<div class=\"btn-row\" style=\"margin-bottom:16px\"><a class=\"btn-main\" href=\"SendNewsletter.aspx\">Soạn bản tin</a></div>");
                builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Email</th><th>Họ tên</th><th>Trạng thái</th><th>Xác nhận</th><th>Ngày đăng ký</th></tr></thead><tbody>");
                foreach (var item in subscribers)
                {
                    builder.Append("<tr><td>" + UiHelper.E(item.Email) + "</td><td>" + UiHelper.E(item.FullName) + "</td><td>" + (item.IsActive ? "Đang nhận" : "Tạm dừng") + "</td><td>" + (item.IsConfirmed ? "Đã xác nhận" : "Chờ xác nhận") + "</td><td>" + UiHelper.E(UiHelper.Date(item.SubscribedAt)) + "</td></tr>");
                }
                builder.Append("</tbody></table></div>");
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải newsletter: " + UiHelper.E(ex.Message) + "</div>");
            }
            return AdminUiHelper.Layout("Quản lý newsletter", builder.ToString());
        }
    }
}
