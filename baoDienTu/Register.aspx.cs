using System;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu
{
    public partial class Register : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.HttpMethod == "POST" && Request.Form["registerAction"] == "register")
            {
                var password = Request.Form["password"];
                if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                {
                    _result = OperationResult.Fail("Mật khẩu cần ít nhất 6 ký tự.");
                    return;
                }

                _result = UserService.Register(Request.Form["username"], password, Request.Form["email"], Request.Form["fullName"]);
            }
        }

        protected string RenderPage()
        {
            return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"form-panel auth-card\">" +
                   "<h1>Đăng ký độc giả</h1>" + UiHelper.Alert(_result) +
                   "<input type=\"hidden\" name=\"registerAction\" value=\"register\" />" +
                   "<div class=\"field\"><label>Họ tên</label><input name=\"fullName\" required /></div>" +
                   "<div class=\"field\" style=\"margin-top:14px\"><label>Email</label><input name=\"email\" type=\"email\" required /></div>" +
                   "<div class=\"field\" style=\"margin-top:14px\"><label>Tên đăng nhập</label><input name=\"username\" required /></div>" +
                   "<div class=\"field\" style=\"margin-top:14px\"><label>Mật khẩu</label><input name=\"password\" type=\"password\" required /></div>" +
                   "<div class=\"btn-row\" style=\"margin-top:18px\"><button class=\"btn-main\" type=\"submit\">Đăng ký</button><a class=\"btn-soft\" href=\"" + ResolveUrl("~/Login.aspx") + "\">Đăng nhập</a></div>" +
                   "</div></div></div>";
        }
    }
}
