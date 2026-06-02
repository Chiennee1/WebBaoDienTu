using System;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Pages
{
    public partial class NewsletterSubscribe : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.HttpMethod == "POST" && Request.Form["newsletterAction"] == "subscribe")
            {
                _result = NewsletterService.Subscribe(Request.Form["email"], Request.Form["fullName"]);
            }
        }

        protected string RenderPage()
        {
            return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"form-panel auth-card\">" +
                   "<h1>Nhận bản tin</h1><p class=\"muted\">Theo dõi các bài viết nổi bật và tin nóng trong ngày.</p>" +
                   UiHelper.Alert(_result) +
                   "<input type=\"hidden\" name=\"newsletterAction\" value=\"subscribe\" />" +
                   "<div class=\"field\"><label>Họ tên</label><input name=\"fullName\" /></div>" +
                   "<div class=\"field\" style=\"margin-top:14px\"><label>Email</label><input name=\"email\" type=\"email\" required /></div>" +
                   "<div class=\"btn-row\" style=\"margin-top:18px\"><button class=\"btn-main\" type=\"submit\">Đăng ký</button></div>" +
                   "</div></div></div>";
        }
    }
}
