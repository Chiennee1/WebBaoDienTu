using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.User
{
    public partial class ChangePassword : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireLogin(this);
            if (Request.HttpMethod == "POST" && Request.Form["changePassword"] == "1")
            {
                var next = Request.Form["newPassword"];
                if (next != Request.Form["confirmPassword"])
                {
                    _result = OperationResult.Fail("Mật khẩu xác nhận không khớp.");
                    return;
                }
                _result = UserBLL.ChangePassword(AuthGuard.CurrentUserId, Request.Form["oldPassword"], next);
            }
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"form-panel auth-card\"><h1>Đổi mật khẩu</h1>");
            builder.Append(UiHelper.Alert(_result));
            builder.Append("<input type=\"hidden\" name=\"changePassword\" value=\"1\" />");
            builder.Append("<div class=\"field\"><label>Mật khẩu hiện tại</label><input type=\"password\" name=\"oldPassword\" required /></div>");
            builder.Append("<div class=\"field\" style=\"margin-top:14px\"><label>Mật khẩu mới</label><input type=\"password\" name=\"newPassword\" required /></div>");
            builder.Append("<div class=\"field\" style=\"margin-top:14px\"><label>Xác nhận mật khẩu mới</label><input type=\"password\" name=\"confirmPassword\" required /></div>");
            builder.Append("<div class=\"btn-row\" style=\"margin-top:18px\"><button class=\"btn-main\" type=\"submit\">Đổi mật khẩu</button><a class=\"btn-soft\" href=\"Profile.aspx\">Hồ sơ</a></div>");
            builder.Append("</div></div></div>");
            return builder.ToString();
        }
    }
}
