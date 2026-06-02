using System;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Pages
{
    public partial class Login : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (AuthGuard.IsAuthenticated && !IsPostBack)
            {
                Response.Redirect("~/Default.aspx");
            }

            if (Request.HttpMethod == "POST" && Request.Form["loginAction"] == "login")
            {
                UserModel user;
                _result = UserService.Login(Request.Form["username"], Request.Form["password"], out user);
                if (_result.Success)
                {
                    AuthGuard.SignIn(user);
                    var returnUrl = Request.QueryString["returnUrl"];
                    Response.Redirect(string.IsNullOrWhiteSpace(returnUrl) ? "~/Admin/Default.aspx" : returnUrl);
                }
            }
        }

        protected string RenderPage()
        {
            return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"form-panel auth-card\">" +
                   "<h1>Đăng nhập</h1>" + UiHelper.Alert(_result) +
                   "<input type=\"hidden\" name=\"loginAction\" value=\"login\" />" +
                   "<div class=\"field\"><label>Tên đăng nhập</label><input name=\"username\" autocomplete=\"username\" required /></div>" +
                   "<div class=\"field\" style=\"margin-top:14px\"><label>Mật khẩu</label><input name=\"password\" type=\"password\" autocomplete=\"current-password\" required /></div>" +
                   "<div class=\"btn-row\" style=\"margin-top:18px\"><button class=\"btn-main\" type=\"submit\">Đăng nhập</button><a class=\"btn-soft\" href=\"" + ResolveUrl("~/Register.aspx") + "\">Tạo tài khoản</a></div>" +
                   "<p class=\"muted\" style=\"margin-top:16px\">Tài khoản mẫu: admin/Admin@123, editor01/Editor@123, reader01/Reader@123.</p>" +
                   "</div></div></div>";
        }
    }
}
