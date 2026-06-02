using System;
using System.Web;
using System.Web.Script.Serialization;
using baoDienTu.BLL;

namespace baoDienTu.Handlers
{
    public class IncreaseView : IHttpHandler
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

            NewsBLL.IncreaseViewCount(newsId);
            Write(context, new { success = true });
        }

        public bool IsReusable { get { return false; } }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
