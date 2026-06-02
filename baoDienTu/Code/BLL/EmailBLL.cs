using System;
using System.Net;
using System.Net.Mail;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class EmailBLL
    {
        public static OperationResult Send(string to, string subject, string htmlContent)
        {
            var host = SettingBLL.Get("SMTP_Host");
            var portValue = SettingBLL.Get("SMTP_Port");
            var user = SettingBLL.Get("SMTP_User");
            var pass = SettingBLL.Get("SMTP_Pass");
            var from = string.IsNullOrWhiteSpace(user) ? SettingBLL.Get("ContactEmail") : user;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                return OperationResult.Fail("Chưa cấu hình SMTP_User/SMTP_Pass trong bảng Settings.");
            }

            int port;
            if (!int.TryParse(portValue, out port))
            {
                port = 587;
            }

            try
            {
                using (var message = new MailMessage())
                using (var client = new SmtpClient(host, port))
                {
                    message.From = new MailAddress(from);
                    message.To.Add(to);
                    message.Subject = subject;
                    message.Body = htmlContent;
                    message.IsBodyHtml = true;

                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(user, pass);
                    client.Send(message);
                }
            }
            catch (Exception ex)
            {
                return OperationResult.Fail("Không gửi được email: " + ex.Message);
            }

            return OperationResult.Ok("Đã gửi email.");
        }

        public static string AbsoluteUrl(string relativeOrAbsolute)
        {
            if (string.IsNullOrWhiteSpace(relativeOrAbsolute))
            {
                return SettingBLL.Get("SiteUrl");
            }

            if (relativeOrAbsolute.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                relativeOrAbsolute.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return relativeOrAbsolute;
            }

            return SettingBLL.Get("SiteUrl").TrimEnd('/') + "/" + relativeOrAbsolute.TrimStart('~', '/');
        }
    }
}
