using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.User
{
    public partial class Profile : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireLogin(this);
            if (Request.HttpMethod == "POST" && Request.Form["saveProfile"] == "1")
            {
                _result = UserBLL.UpdateProfile(AuthGuard.CurrentUserId, Request.Form["fullName"], Request.Form["phone"], Request.Form["avatar"]);
            }
        }

        protected string RenderPage()
        {
            var user = UserBLL.GetById(AuthGuard.CurrentUserId);
            var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"form-panel auth-card\"><h1>Hồ sơ cá nhân</h1>");
            builder.Append(UiHelper.Alert(_result));
            builder.Append("<input type=\"hidden\" name=\"saveProfile\" value=\"1\" />");
            builder.Append("<div class=\"field\"><label>Họ tên</label><input name=\"fullName\" value=\"" + UiHelper.Attr(user.FullName) + "\" required /></div>");
            builder.Append("<div class=\"field\" style=\"margin-top:14px\"><label>Điện thoại</label><input name=\"phone\" value=\"" + UiHelper.Attr(user.Phone) + "\" /></div>");
            builder.Append("<div class=\"field\" style=\"margin-top:14px\"><label>Avatar URL/path</label><input name=\"avatar\" value=\"" + UiHelper.Attr(user.Avatar) + "\" /></div>");
            builder.Append("<div class=\"btn-row\" style=\"margin-top:18px\"><button class=\"btn-main\" type=\"submit\">Lưu hồ sơ</button><a class=\"btn-soft\" href=\"ChangePassword.aspx\">Đổi mật khẩu</a></div>");
            builder.Append("</div></div></div>");
            return builder.ToString();
        }
    }
}
