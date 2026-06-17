using System;
using System.Text;
using System.Web;

namespace baoDienTu.Helpers
{
    public static class AdminUiHelper
    {
        public static string Layout(string title, string content)
        {
            var builder = new StringBuilder();
            builder.Append("<div class=\"page-shell\"><div class=\"container-xl admin-layout\">");
            builder.Append("<aside class=\"admin-nav\">");
            AppendNav(builder, "~/Admin/Default.aspx", "Tổng quan");
            AppendNav(builder, "~/Admin/ManageNews.aspx", "Bài viết");
            AppendNav(builder, "~/Admin/AddEditNews.aspx", "Viết bài");
            if (AuthGuard.IsAdmin)
            {
                AppendNav(builder, "~/Admin/PendingNews.aspx", "Duyệt bài");
                AppendNav(builder, "~/Admin/ManageCategory.aspx", "Chuyên mục");
                AppendNav(builder, "~/Admin/ManageUser.aspx", "Người dùng");
                AppendNav(builder, "~/Admin/ManageComment.aspx", "Bình luận");
                AppendNav(builder, "~/Admin/ManageNewsletter.aspx", "Newsletter");
                AppendNav(builder, "~/Admin/SendNewsletter.aspx", "Gửi bản tin");
                AppendNav(builder, "~/Admin/ManageSettings.aspx", "Cấu hình");
            }
            else if (AuthGuard.IsEditor)
            {
                AppendNav(builder, "~/Editor/MyNews.aspx", "Bài của tôi");
            }
            builder.Append("</aside><section>");
            builder.Append("<div class=\"section-title\"><h1>" + UiHelper.E(title) + "</h1><a href=\"" + UiHelper.Attr(Url("~/Default.aspx")) + "\">Về trang chủ</a></div>");
            builder.Append(content);
            builder.Append("</section></div></div>");
            return builder.ToString();
        }

        private static void AppendNav(StringBuilder builder, string path, string label)
        {
            var absPath = Url(path);
            var currentPath = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath ?? string.Empty;
            // So sánh không phân biệt hoa thường, bỏ query string
            var isActive = string.Equals(
                VirtualPathUtility.ToAbsolute(currentPath.Split('?')[0]),
                absPath.Split('?')[0],
                StringComparison.OrdinalIgnoreCase);
            builder.Append("<a href=\"");
            builder.Append(UiHelper.Attr(absPath));
            builder.Append("\"" + (isActive ? " class=\"admin-nav-active\"" : string.Empty) + ">");
            builder.Append(UiHelper.E(label));
            builder.Append("</a>");
        }


        private static string Url(string path)
        {
            return VirtualPathUtility.ToAbsolute(path);
        }
    }
}
