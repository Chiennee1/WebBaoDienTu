using baoDienTu.DAL;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class ShareLogBLL
    {
        public static OperationResult ShareNews(int newsId, string senderName, string senderEmail, string receiverEmail, string message, string newsUrl)
        {
            var mail = EmailBLL.Send(receiverEmail, "Tin hay được chia sẻ cho bạn", "<p>" + senderName + " muốn chia sẻ một bài viết:</p><p><a href=\"" + newsUrl + "\">Xem bài viết</a></p><p>" + message + "</p>");
            ShareLogDAL.Add(newsId, senderName, senderEmail, receiverEmail, message, mail.Success);
            return mail.Success ? OperationResult.Ok("Đã gửi tin cho bạn.") : mail;
        }
    }
}
