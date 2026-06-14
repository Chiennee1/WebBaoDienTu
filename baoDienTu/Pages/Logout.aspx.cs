using System;
using System.Web.UI;
using baoDienTu.Helpers;

namespace baoDienTu.Pages
{
    public partial class Logout : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.SignOut();
            Response.Redirect("~/Default.aspx");
        }
    }
}
