using System.Collections.Generic;
using baoDienTu.DAL;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class CommentBLL
    {
        public static List<CommentModel> GetApprovedByNews(int newsId)
        {
            return CommentDAL.GetApprovedByNews(newsId);
        }

        public static OperationResult Add(int newsId, int? userId, string guestName, string guestEmail, string content)
        {
            var result = CommentDAL.Add(newsId, userId, guestName, guestEmail, content);
            return result == -1
                ? OperationResult.Fail("Bài viết hiện không cho phép bình luận.")
                : OperationResult.Ok("Bình luận đã được gửi và đang chờ duyệt.");
        }

        public static List<CommentModel> GetAdminComments(bool? approved, int page, int pageSize, out int total)
        {
            return CommentDAL.GetAdminComments(approved, page, pageSize, out total);
        }

        public static void Approve(int commentId, bool approved)
        {
            CommentDAL.Approve(commentId, approved);
        }

        public static void Delete(int commentId)
        {
            CommentDAL.Delete(commentId);
        }
    }
}
