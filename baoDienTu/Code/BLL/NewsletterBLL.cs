using System.Collections.Generic;
using System;
using System.Net.Mail;
using System.Text;
using baoDienTu.DAL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class NewsletterBLL
    {
        public static OperationResult Subscribe(string email, string fullName)
        {
            email = string.IsNullOrWhiteSpace(email) ? string.Empty : email.Trim();
            fullName = string.IsNullOrWhiteSpace(fullName) ? string.Empty : fullName.Trim();

            if (!IsValidEmail(email))
            {
                return OperationResult.Fail("Vui lòng nhập email hợp lệ.");
            }

            var existing = NewsletterDAL.GetByEmail(email);
            if (existing != null && existing.IsConfirmed)
            {
                return OperationResult.Ok(existing.IsActive
                    ? "Email này đã xác nhận đăng ký newsletter."
                    : "Email này đã hủy newsletter trước đó. Vui lòng liên hệ quản trị viên để kích hoạt lại.");
            }

            var token = SecurityHelper.GenerateToken();
            var unsubToken = SecurityHelper.GenerateToken();
            var code = NewsletterDAL.Subscribe(email, fullName, token, unsubToken);
            var confirmUrl = EmailBLL.AbsoluteUrl("~/NewsletterConfirm.aspx?token=" + token);
            var mail = EmailBLL.Send(email, "Xác nhận đăng ký bản tin", "<p>Vui lòng xác nhận đăng ký bản tin tại liên kết sau:</p><p><a href=\"" + confirmUrl + "\">Xác nhận email</a></p>");

            if (!mail.Success)
            {
                return OperationResult.Ok("Đã lưu đăng ký. " + mail.Message);
            }

            return code == 1
                ? OperationResult.Ok("Đăng ký thành công. Vui lòng kiểm tra email để xác nhận.")
                : OperationResult.Ok("Email đã tồn tại. Chúng tôi đã gửi lại email xác nhận nếu cần.");
        }

        public static OperationResult Confirm(string token)
        {
            return NewsletterDAL.Confirm(token) == 1
                ? OperationResult.Ok("Đã xác nhận đăng ký newsletter.")
                : OperationResult.Fail("Token xác nhận không hợp lệ hoặc đã được dùng.");
        }

        public static OperationResult Unsubscribe(string token)
        {
            return NewsletterDAL.Unsubscribe(token) == 1
                ? OperationResult.Ok("Đã hủy đăng ký newsletter.")
                : OperationResult.Fail("Token hủy đăng ký không hợp lệ.");
        }

        public static List<NewsletterModel> GetAll()
        {
            return NewsletterDAL.GetAll();
        }

        public static OperationResult SendNewsletter(string subject, string htmlContent, int sentBy)
        {
            var subscribers = NewsletterDAL.GetActiveSubscribers();
            var sent = 0;
            foreach (var sub in subscribers)
            {
                var content = htmlContent + "<hr/><p><a href=\"" + EmailBLL.AbsoluteUrl("~/Unsubscribe.aspx?token=" + sub.UnsubscribeToken) + "\">Hủy đăng ký</a></p>";
                var result = EmailBLL.Send(sub.Email, subject, content);
                if (result.Success)
                {
                    sent++;
                }
            }

            NewsletterDAL.AddSendHistory(subject, htmlContent, sentBy, sent);
            return OperationResult.Ok("Đã gửi bản tin đến " + sent + "/" + subscribers.Count + " subscriber.");
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var address = new MailAddress(email);
                return string.Equals(address.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
