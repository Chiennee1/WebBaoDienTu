using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected string RenderCategoryLinks()
        {
            try
            {
                var builder = new StringBuilder();
                foreach (var category in CategoryService.GetCategories(true))
                {
                    if (category.ParentID.HasValue)
                    {
                        continue;
                    }
                    builder.Append("<a href=\"");
                    builder.Append(UiHelper.Attr(UiHelper.CategoryUrl(category)));
                    builder.Append("\">");
                    builder.Append(UiHelper.E(category.CatName));
                    builder.Append("</a>");
                }
                return builder.ToString();
            }
            catch
            {
                return "<a href=\"" + ResolveUrl("~/NewsList.aspx") + "\">Tin mới</a>";
            }
        }

        protected string RenderAuthLinks()
        {
            if (!AuthGuard.IsAuthenticated)
            {
                return "<a href=\"" + ResolveUrl("~/Login.aspx") + "\">Đăng nhập</a><a href=\"" + ResolveUrl("~/Register.aspx") + "\">Đăng ký</a>";
            }

            var builder = new StringBuilder();
            builder.Append("<span>Xin chào, ");
            builder.Append(UiHelper.E(AuthGuard.CurrentFullName));
            builder.Append("</span>");

            if (AuthGuard.IsAdmin || AuthGuard.IsEditor)
            {
                builder.Append("<a href=\"");
                builder.Append(ResolveUrl("~/Admin/Default.aspx"));
                builder.Append("\">Quản trị</a>");
            }

            builder.Append("<a href=\"");
            builder.Append(ResolveUrl("~/Logout.aspx"));
            builder.Append("\">Đăng xuất</a>");
            return builder.ToString();
        }
    }
}
