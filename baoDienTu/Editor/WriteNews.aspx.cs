using System;
using System.Web.UI;
using baoDienTu.Helpers;

namespace baoDienTu.Editor
{
    public partial class WriteNews : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin", "Editor");
            Response.Redirect("~/Admin/AddEditNews.aspx", true);
        }
    }
}
