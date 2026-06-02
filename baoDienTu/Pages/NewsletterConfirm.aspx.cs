using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;

namespace baoDienTu.Pages
{
    public partial class NewsletterConfirm : Page
    {
        protected string RenderPage()
        {
            var result = NewsletterBLL.Confirm(Request.QueryString["token"]);
            return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"form-panel auth-card\"><h1>Xác nhận newsletter</h1>" + UiHelper.Alert(result) + "<a class=\"btn-main\" href=\"" + ResolveUrl("~/Default.aspx") + "\">Về trang chủ</a></div></div></div>";
        }
    }
}
