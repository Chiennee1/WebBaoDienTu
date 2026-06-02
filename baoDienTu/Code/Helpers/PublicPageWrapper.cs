using System;
using System.IO;
using System.Web.UI;

namespace baoDienTu
{
    public class PublicPageWrapper : Page
    {
        protected override void OnLoad(EventArgs e)
        {
            var fileName = Path.GetFileName(Request.AppRelativeCurrentExecutionFilePath);
            if (!fileName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".aspx";
            }
            Server.Transfer("~/Pages/" + fileName, true);
        }
    }
}
