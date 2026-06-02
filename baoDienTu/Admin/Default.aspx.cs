using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Admin
{
    public partial class Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin", "Editor");
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder();
            try
            {
                if (AuthGuard.IsAdmin)
                {
                    var stats = DashboardService.GetStats();
                    builder.Append("<div class=\"stat-grid\">");
                    builder.Append(Stat("Đã xuất bản", stats.TotalApprovedNews));
                    builder.Append(Stat("Chờ duyệt", stats.TotalPendingNews));
                    builder.Append(Stat("Người dùng", stats.TotalActiveUsers));
                    builder.Append(Stat("Subscribers", stats.TotalSubscribers));
                    builder.Append(Stat("Bình luận chờ", stats.TotalPendingComments));
                    builder.Append(Stat("Tổng lượt xem", stats.TotalViews));
                    builder.Append("</div>");
                }
                else
                {
                    builder.Append("<div class=\"form-panel\"><h2>Không gian biên tập</h2><p class=\"muted\">Bạn có thể viết bài mới và quản lý các bài do mình tạo.</p><a class=\"btn-main\" href=\"AddEditNews.aspx\">Viết bài mới</a></div>");
                }
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải dashboard: " + UiHelper.E(ex.Message) + "</div>");
            }
            return AdminUiHelper.Layout("Bảng điều khiển", builder.ToString());
        }

        private static string Stat(string label, int value)
        {
            return "<article class=\"stat-card\"><span class=\"muted\">" + UiHelper.E(label) + "</span><strong>" + value + "</strong></article>";
        }
    }
}
