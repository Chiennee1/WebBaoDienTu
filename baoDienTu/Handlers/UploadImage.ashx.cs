using System;
using System.Configuration;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.SessionState;
using baoDienTu.Helpers;

namespace baoDienTu.Handlers
{
    public class UploadImage : IHttpHandler, IRequiresSessionState
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            if (!AuthGuard.IsAuthenticated || (!AuthGuard.IsAdmin && !AuthGuard.IsEditor))
            {
                Write(context, new { uploaded = 0, error = new { message = "Bạn cần đăng nhập Editor/Admin để upload ảnh." } });
                return;
            }

            var file = context.Request.Files["upload"];
            int maxMb;
            if (!int.TryParse(ConfigurationManager.AppSettings["MaxFileSizeMB"], out maxMb))
            {
                maxMb = 5;
            }

            string error;
            if (!FileUploadHelper.IsValidImage(file, maxMb, out error))
            {
                Write(context, new { uploaded = 0, error = new { message = error } });
                return;
            }

            var publicFolder = ConfigurationManager.AppSettings["UploadPath"] ?? "~/Static/uploads/news/";
            var serverFolder = context.Server.MapPath(publicFolder);
            var url = FileUploadHelper.SaveImage(file, serverFolder, publicFolder);
            Write(context, new { uploaded = 1, url = url });
        }

        public bool IsReusable { get { return false; } }

        private static void Write(HttpContext context, object payload)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(payload));
        }
    }
}
