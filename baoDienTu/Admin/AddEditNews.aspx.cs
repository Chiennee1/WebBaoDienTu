using System;
using System.Linq;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class AddEditNews : Page
    {
        private OperationResult _result;
        private NewsModel _news;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin", "Editor");
            var id = GetId();
            if (id > 0)
            {
                _news = NewsService.GetById(id);
                if (_news == null || (!AuthGuard.IsAdmin && _news.AuthorID != AuthGuard.CurrentUserId))
                {
                    Response.Redirect("~/Admin/ManageNews.aspx");
                }
            }

            if (Request.HttpMethod == "POST" && Request.Form["saveNews"] == "1")
            {
                Save();
            }
        }

        protected string RenderPage()
        {
            var categories = CategoryService.GetCategories(true);
            var builder = new StringBuilder();
            builder.Append(UiHelper.Alert(_result));
            builder.Append("<div class=\"form-panel\"><input type=\"hidden\" name=\"saveNews\" value=\"1\" />");
            builder.Append("<div class=\"form-grid\">");
            Field(builder, "Tiêu đề", "title", _news == null ? string.Empty : _news.Title, true);
            builder.Append("<div class=\"field\"><label>Chuyên mục</label><select name=\"catId\" required>");
            foreach (var cat in categories)
            {
                var selected = _news != null && _news.CatID == cat.CatID ? " selected" : string.Empty;
                builder.Append("<option value=\"" + cat.CatID + "\"" + selected + ">" + UiHelper.E(cat.Breadcrumb == string.Empty ? cat.CatName : cat.Breadcrumb) + "</option>");
            }
            builder.Append("</select></div>");
            Field(builder, "Ảnh đại diện URL/path", "thumbnail", _news == null ? string.Empty : _news.Thumbnail, false);
            Field(builder, "Tags (phân cách bằng dấu phẩy)", "tags", _news == null ? string.Empty : TagBLL.GetTagNameCsv(_news.NewsID), false);
            builder.Append("<div class=\"field\"><label>Cho bình luận</label><select name=\"allowComment\"><option value=\"1\"" + (_news == null || _news.AllowComment ? " selected" : string.Empty) + ">Có</option><option value=\"0\"" + (_news != null && !_news.AllowComment ? " selected" : string.Empty) + ">Không</option></select></div>");
            builder.Append("<div class=\"field full\"><label>Tóm tắt</label><textarea name=\"summary\" required>" + UiHelper.E(_news == null ? string.Empty : _news.Summary) + "</textarea></div>");
            builder.Append("<div class=\"field full\"><label>Nội dung HTML</label><textarea id=\"newsContent\" name=\"content\" style=\"min-height:320px\" required>" + UiHelper.E(_news == null ? string.Empty : _news.Content) + "</textarea></div>");
            if (AuthGuard.IsAdmin)
            {
                builder.Append("<div class=\"field\"><label>Nổi bật</label><select name=\"isFeatured\"><option value=\"1\"" + (_news != null && _news.IsFeatured ? " selected" : string.Empty) + ">Có</option><option value=\"0\"" + (_news == null || !_news.IsFeatured ? " selected" : string.Empty) + ">Không</option></select></div>");
                builder.Append("<div class=\"field\"><label>Tin nóng</label><select name=\"isHot\"><option value=\"1\"" + (_news != null && _news.IsHot ? " selected" : string.Empty) + ">Có</option><option value=\"0\"" + (_news == null || !_news.IsHot ? " selected" : string.Empty) + ">Không</option></select></div>");
            }
            builder.Append("</div><div class=\"btn-row\" style=\"margin-top:18px\"><button class=\"btn-main\" type=\"submit\">Lưu và gửi duyệt</button><a class=\"btn-soft\" href=\"ManageNews.aspx\">Quay lại</a></div></div>");
            builder.Append("<script src=\"https://cdn.ckeditor.com/ckeditor5/41.4.2/classic/ckeditor.js\"></script><script>ClassicEditor.create(document.querySelector('#newsContent'),{ckfinder:{uploadUrl:'../Handlers/UploadImage.ashx'}}).catch(function(e){console.error(e);});</script>");
            return AdminUiHelper.Layout(_news == null ? "Viết bài mới" : "Sửa bài viết", builder.ToString());
        }

        private void Save()
        {
            try
            {
                var title = Request.Form["title"];
                var summary = Request.Form["summary"];
                var content = Request.Unvalidated.Form["content"];
                var thumbnail = Request.Form["thumbnail"];
                var tags = Request.Form["tags"];
                var catId = Convert.ToInt32(Request.Form["catId"]);
                var allowComment = Request.Form["allowComment"] == "1";
                if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                {
                    _result = OperationResult.Fail("Vui lòng nhập tiêu đề và nội dung.");
                    return;
                }

                if (_news == null)
                {
                    var id = NewsBLL.AddNews(title, summary, content, thumbnail, AuthGuard.CurrentUserId, catId, allowComment, tags);
                    _news = NewsService.GetById(id);
                    _result = OperationResult.Ok("Đã tạo bài viết và gửi chờ duyệt.");
                }
                else
                {
                    var isFeatured = AuthGuard.IsAdmin ? Request.Form["isFeatured"] == "1" : _news.IsFeatured;
                    var isHot = AuthGuard.IsAdmin ? Request.Form["isHot"] == "1" : _news.IsHot;
                    NewsBLL.UpdateNews(_news.NewsID, title, summary, content, thumbnail, catId, allowComment, isFeatured, isHot, tags);
                    _news = NewsService.GetById(_news.NewsID);
                    _result = OperationResult.Ok("Đã cập nhật bài viết và gửi lại chờ duyệt.");
                }
            }
            catch (Exception ex)
            {
                _result = OperationResult.Fail(ex.Message);
            }
        }

        private int GetId()
        {
            int id;
            return int.TryParse(Request.QueryString["id"], out id) ? id : 0;
        }

        private static void Field(StringBuilder builder, string label, string name, string value, bool required)
        {
            builder.Append("<div class=\"field\"><label>" + UiHelper.E(label) + "</label><input name=\"" + UiHelper.Attr(name) + "\" value=\"" + UiHelper.Attr(value) + "\"" + (required ? " required" : string.Empty) + " /></div>");
        }
    }
}
