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
                        var newsId = Convert.ToInt32(Request.Form["approve"]);
                        NewsService.ApproveNews(newsId, AuthGuard.CurrentUserId, true, null);
                        _result = OperationResult.Ok("Đã duyệt bài.");
                    }
                    else if (!string.IsNullOrWhiteSpace(Request.Form["reject"]))
                    {
                        var newsId = Convert.ToInt32(Request.Form["reject"]);
                        var reason = (Request.Form["reason_" + newsId] ?? string.Empty).Trim();
                        if (string.IsNullOrWhiteSpace(reason))
                        {
                            reason = "Không đạt yêu cầu biên tập.";
                        }

                        NewsService.ApproveNews(newsId, AuthGuard.CurrentUserId, false, reason);
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
                var page = Math.Max(1, Convert.ToInt32(Request.QueryString["page"] ?? "1"));
                const int pageSize = 15;
                int total;
                var items = NewsService.GetAdminNews(1, null, null, null, page, pageSize, out total);
                if (items.Count == 0)
                {
                    builder.Append("<div class=\"empty-state\">🎉 Không có bài viết nào đang chờ duyệt.</div>");
                }
                else
                {
                    builder.Append("<p class=\"muted\" style=\"margin-bottom:12px\">Có tổng cộng <strong>" + total + "</strong> bài đang chờ duyệt.</p>");
                    builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Bài viết</th><th>Tác giả</th><th>Chuyên mục</th><th>Thao tác</th></tr></thead><tbody>");
                    foreach (var item in items)
                    {
                        builder.Append("<tr><td><strong>" + UiHelper.E(item.Title) + "</strong><br/><span class=\"muted\">" + UiHelper.E(UiHelper.Excerpt(item.Summary, 110)) + "</span></td>");
                        builder.Append("<td>" + UiHelper.E(item.AuthorName) + "</td><td>" + UiHelper.E(item.CatName) + "</td>");
                        builder.Append("<td>");
                        builder.Append("<a class=\"btn-soft\" href=\"AddEditNews.aspx?id=" + item.NewsID + "\">Xem/Sửa</a> ");
                        builder.Append("<button class=\"btn-main\" type=\"submit\" name=\"approve\" value=\"" + item.NewsID + "\" onclick=\"return confirm('Duyệt bài này?')\">✅ Duyệt</button> ");
                        builder.Append("<button class=\"btn-danger\" type=\"button\" onclick=\"showRejectBox(" + item.NewsID + ")\">❌ Từ chối</button>");
                        
                        // Ô nhập lý do từ chối ẩn
                        builder.Append("<div id=\"rejectBox_" + item.NewsID + "\" style=\"display:none;margin-top:8px;background:#fff7ed;border:1px solid #fdba74;border-radius:6px;padding:10px;\">");
                        builder.Append("<label style=\"font-weight:700;font-size:0.87rem;color:#92400e\">Lý do từ chối:</label>");
                        builder.Append("<textarea name=\"reason_" + item.NewsID + "\" id=\"reason_" + item.NewsID + "\" rows=\"2\" style=\"width:100%;margin-top:4px;border-radius:4px;border:1px solid #fdba74;padding:6px;font-size:0.9rem\" placeholder=\"Nhập lý do... (để trống = dùng lý do mặc định)\"></textarea>");
                        builder.Append("<div style=\"margin-top:6px;display:flex;gap:6px\">");
                        builder.Append("<button class=\"btn-danger\" type=\"submit\" name=\"reject\" value=\"" + item.NewsID + "\">Xác nhận từ chối</button>");
                        builder.Append("<button class=\"btn-soft\" type=\"button\" onclick=\"hideRejectBox(" + item.NewsID + ")\">Hủy</button>");
                        builder.Append("</div></div>");
                        builder.Append("</td></tr>");
                    }
                    builder.Append("</tbody></table></div>");
                    builder.Append(UiHelper.Pagination(page, pageSize, total, p => ResolveUrl("~/Admin/PendingNews.aspx?page=" + p)));
                }
            }
            catch (Exception ex)
            {
                builder.Append("<div class=\"app-alert danger\">Không thể tải danh sách chờ duyệt: " + UiHelper.E(ex.Message) + "</div>");
            }

            builder.Append(@"<script>
function showRejectBox(id) {
    document.getElementById('rejectBox_' + id).style.display = 'block';
}
function hideRejectBox(id) {
    document.getElementById('rejectBox_' + id).style.display = 'none';
}
</script>");

            return AdminUiHelper.Layout("Duyệt bài", builder.ToString());
        }
    }
}
