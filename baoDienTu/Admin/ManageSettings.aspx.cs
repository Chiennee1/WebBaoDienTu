using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class ManageSettings : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
            if (Request.HttpMethod == "POST" && Request.Form["saveSettings"] == "1")
            {
                var values = new Dictionary<string, string>
                {
                    { "SiteName", Request.Form["SiteName"] },
                    { "SiteDescription", Request.Form["SiteDescription"] },
                    { "SiteUrl", Request.Form["SiteUrl"] },
                    { "ContactEmail", Request.Form["ContactEmail"] },
                    { "SMTP_Host", Request.Form["SMTP_Host"] },
                    { "SMTP_Port", Request.Form["SMTP_Port"] },
                    { "SMTP_User", Request.Form["SMTP_User"] },
                    { "SMTP_Pass", Request.Form["SMTP_Pass"] }
                };
                _result = SettingBLL.Save(values);
            }
        }

        protected string RenderPage()
        {
            var settings = SettingBLL.GetAll();
            var builder = new StringBuilder(UiHelper.Alert(_result));
            builder.Append("<div class=\"form-panel\"><input type=\"hidden\" name=\"saveSettings\" value=\"1\" /><div class=\"form-grid\">");
            Field(builder, settings, "SiteName", "Tên website");
            Field(builder, settings, "SiteDescription", "Mô tả");
            Field(builder, settings, "SiteUrl", "URL website");
            Field(builder, settings, "ContactEmail", "Email liên hệ");
            Field(builder, settings, "SMTP_Host", "SMTP host");
            Field(builder, settings, "SMTP_Port", "SMTP port");
            Field(builder, settings, "SMTP_User", "SMTP user");
            Field(builder, settings, "SMTP_Pass", "SMTP password");
            builder.Append("</div><div class=\"btn-row\" style=\"margin-top:16px\"><button class=\"btn-main\" type=\"submit\">Lưu cấu hình</button></div></div>");
            return AdminUiHelper.Layout("Cấu hình hệ thống", builder.ToString());
        }

        private static void Field(StringBuilder builder, Dictionary<string, string> settings, string key, string label)
        {
            var value = settings.ContainsKey(key) ? settings[key] : string.Empty;
            builder.Append("<div class=\"field\"><label>" + UiHelper.E(label) + "</label><input name=\"" + UiHelper.Attr(key) + "\" value=\"" + UiHelper.Attr(value) + "\" /></div>");
        }
    }
}
