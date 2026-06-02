using System.Text;

namespace baoDienTu.Helpers
{
    public static class AdminUiHelper
    {
        public static string Layout(string title, string content)
        {
            var builder = new StringBuilder();
            builder.Append("<div class=\"page-shell\"><div class=\"container-xl admin-layout\">");
            builder.Append("<aside class=\"admin-nav\">");
            builder.Append("<a href=\"/Admin/Default.aspx\">Tổng quan</a>");
            builder.Append("<a href=\"/Admin/ManageNews.aspx\">Bài viết</a>");
            builder.Append("<a href=\"/Admin/AddEditNews.aspx\">Viết bài</a>");
            if (AuthGuard.IsAdmin)
            {
                builder.Append("<a href=\"/Admin/PendingNews.aspx\">Duyệt bài</a>");
                builder.Append("<a href=\"/Admin/ManageCategory.aspx\">Chuyên mục</a>");
                builder.Append("<a href=\"/Admin/ManageUser.aspx\">Người dùng</a>");
                builder.Append("<a href=\"/Admin/ManageComment.aspx\">Bình luận</a>");
                builder.Append("<a href=\"/Admin/ManageNewsletter.aspx\">Newsletter</a>");
                builder.Append("<a href=\"/Admin/SendNewsletter.aspx\">Gửi bản tin</a>");
                builder.Append("<a href=\"/Admin/ManageSettings.aspx\">Cấu hình</a>");
            }
            else if (AuthGuard.IsEditor)
            {
                builder.Append("<a href=\"/Editor/MyNews.aspx\">Bài của tôi</a>");
            }
            builder.Append("</aside><section>");
            builder.Append("<div class=\"section-title\"><h1>" + UiHelper.E(title) + "</h1><a href=\"/Default.aspx\">Về trang chủ</a></div>");
            builder.Append(content);
            builder.Append("</section></div></div>");
            return builder.ToString();
        }
    }
}
