using System;
using System.Web;
using System.Web.Script.Serialization;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Handlers
{
    public class ShareNews : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            int newsId;
            if (!int.TryParse(context.Request["newsId"], out newsId) || newsId <= 0)
            {
                Write(context, new { success = false, message = "NewsID không hợp lệ." });
                return;
            }

            var news = NewsBLL.GetById(newsId);
            if (news == null)
            {
                Write(context, new { success = false, message = "Bài viết không tồn tại." });
                return;
            }

            var newsUrl = EmailBLL.AbsoluteUrl("~/NewsDetail.aspx?slug=" + HttpUtility.UrlEncode(news.Slug));
            var result = ShareLogBLL.ShareNews(
                newsId,
                context.Request["senderName"],
                context.Request["senderEmail"],
                context.Request["receiverEmail"],
                context.Request["message"],
                newsUrl);
            Write(context, new { success = result.Success, message = result.Message });
        }

        public bool IsReusable { get { return false; } }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
