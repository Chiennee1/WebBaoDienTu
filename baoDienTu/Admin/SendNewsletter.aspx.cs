using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class SendNewsletter : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
            if (Request.HttpMethod == "POST" && Request.Form["sendNewsletter"] == "1")
            {
                _result = NewsletterBLL.SendNewsletter(Request.Form["subject"], Request.Unvalidated.Form["htmlContent"], AuthGuard.CurrentUserId);
            }
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder(UiHelper.Alert(_result));
            builder.Append("<div class=\"form-panel\"><input type=\"hidden\" name=\"sendNewsletter\" value=\"1\" />");
            builder.Append("<div class=\"field\"><label>Tiêu đề email</label><input name=\"subject\" required /></div>");
            builder.Append("<div class=\"field\" style=\"margin-top:14px\"><label>Nội dung HTML</label><textarea id=\"newsletterContent\" name=\"htmlContent\" required style=\"min-height:280px\"></textarea></div>");
            builder.Append("<div class=\"btn-row\" style=\"margin-top:16px\"><button class=\"btn-main\" type=\"submit\">Gửi newsletter</button><a class=\"btn-soft\" href=\"ManageNewsletter.aspx\">Danh sách đăng ký</a></div>");
            builder.Append("</div><script src=\"https://cdn.ckeditor.com/ckeditor5/41.4.2/classic/ckeditor.js\"></script><script>ClassicEditor.create(document.querySelector('#newsletterContent')).catch(function(e){console.error(e);});</script>");
            return AdminUiHelper.Layout("Gửi newsletter", builder.ToString());
        }
    }
}
