using System;
using System.Net.Mail;
using System.Text;
using System.Web;
using baoDienTu.DAL;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class ShareLogBLL
    {
        public static OperationResult ShareNews(int newsId, string senderName, string senderEmail, string receiverEmail, string message, string newsUrl)
        {
            senderName = string.IsNullOrWhiteSpace(senderName) ? "Một độc giả" : senderName.Trim();
            senderEmail = string.IsNullOrWhiteSpace(senderEmail) ? string.Empty : senderEmail.Trim();
            receiverEmail = string.IsNullOrWhiteSpace(receiverEmail) ? string.Empty : receiverEmail.Trim();
            message = string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim();

            if (!IsValidEmail(receiverEmail))
            {
                return OperationResult.Fail("Vui lòng nhập email người nhận hợp lệ.");
            }

            if (!string.IsNullOrWhiteSpace(senderEmail) && !IsValidEmail(senderEmail))
            {
                return OperationResult.Fail("Email của bạn không hợp lệ.");
            }

            if (message.Length > 500)
            {
                return OperationResult.Fail("Lời nhắn không được vượt quá 500 ký tự.");
            }

            var body = new StringBuilder();
            body.Append("<p>");
            body.Append(HttpUtility.HtmlEncode(senderName));
            body.Append(" muốn chia sẻ một bài viết với bạn.</p>");
            body.Append("<p><a href=\"");
            body.Append(HttpUtility.HtmlAttributeEncode(newsUrl));
            body.Append("\">Xem bài viết</a></p>");
            if (!string.IsNullOrWhiteSpace(message))
            {
                body.Append("<p>");
                body.Append(HttpUtility.HtmlEncode(message));
                body.Append("</p>");
            }

            var mail = EmailBLL.Send(receiverEmail, "Tin hay được chia sẻ cho bạn", body.ToString());
            ShareLogDAL.Add(newsId, senderName, senderEmail, receiverEmail, message, mail.Success);
            return mail.Success ? OperationResult.Ok("Đã gửi tin cho bạn.") : mail;
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
