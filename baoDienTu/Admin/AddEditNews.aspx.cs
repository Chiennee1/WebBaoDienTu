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

            if (_news != null)
            {
                var statusHtml = UiHelper.StatusBadge(_news.Status);
                var rejectHtml = (_news.Status == 3 && !string.IsNullOrWhiteSpace(_news.RejectReason))
                    ? "<div style=\"margin-top: 8px; color: var(--danger); font-weight: 700;\">Lý do từ chối: <span style=\"font-weight: 500; font-style: italic;\">" + UiHelper.E(_news.RejectReason) + "</span></div>"
                    : string.Empty;

                builder.Append("<div style=\"margin-bottom: 20px; padding: 14px; background: var(--surface); border: 1px solid var(--line); border-radius: var(--radius);\">");
                builder.Append("<div><strong>Trạng thái hiện tại:</strong> " + statusHtml + "</div>");
                builder.Append(rejectHtml);
                builder.Append("</div>");

                if (_news.Status == 2)
                {
                    builder.Append("<div class=\"app-alert warning\">⚠️ <strong>Lưu ý:</strong> Bài viết này đã được xuất bản (Published). Nếu bạn sửa đổi và lưu bài viết, trạng thái sẽ tự động chuyển về <strong>Chờ duyệt</strong> và tạm thời ẩn khỏi trang chủ cho đến khi được kiểm duyệt lại.</div>");
                }
            }

            builder.Append("<div class=\"form-grid\">");
            Field(builder, "Tiêu đề", "title", _news == null ? string.Empty : _news.Title, true);
            builder.Append("<div class=\"field\"><label>Chuyên mục</label><select name=\"catId\" required>");
            foreach (var cat in categories)
            {
                var selected = _news != null && _news.CatID == cat.CatID ? " selected" : string.Empty;
                builder.Append("<option value=\"" + cat.CatID + "\"" + selected + ">" + UiHelper.E(cat.Breadcrumb == string.Empty ? cat.CatName : cat.Breadcrumb) + "</option>");
            }
            builder.Append("</select></div>");
            // Thumbnail URL & Upload
            builder.Append("<div class=\"field full\"><label>Ảnh đại diện (Thumbnail)</label>");
            builder.Append("<div style=\"display: grid; grid-template-columns: 1.2fr 1fr; gap: 16px; align-items: start;\">");
            // Cột trái: Nhập URL hoặc xem trước
            builder.Append("<div>");
            builder.Append("<input id=\"inp_thumbnail\" name=\"thumbnail\" value=\"" + UiHelper.Attr(_news == null ? string.Empty : _news.Thumbnail) + "\" placeholder=\"Nhập URL ảnh hoặc upload bên phải...\" style=\"margin-bottom: 8px;\" oninput=\"previewThumbnail(this.value)\" />");
            builder.Append("<div id=\"thumbPreviewContainer\" style=\"width: 100%; aspect-ratio: 16/9; background: #f3f4f6; border: 1px solid var(--line); border-radius: var(--radius); display: grid; place-items: center; overflow: hidden;\">");
            var thumbUrl = (_news != null && !string.IsNullOrWhiteSpace(_news.Thumbnail)) ? UiHelper.ResolveImageUrl(_news.Thumbnail) : string.Empty;
            if (!string.IsNullOrWhiteSpace(thumbUrl))
            {
                builder.Append("<img id=\"thumbPreviewImg\" src=\"" + UiHelper.Attr(thumbUrl) + "\" style=\"width:100%; height:100%; object-fit:cover;\" />");
                builder.Append("<span id=\"thumbPlaceholderText\" style=\"display:none; color:var(--muted); font-size:0.9rem;\">Xem trước ảnh</span>");
            }
            else
            {
                builder.Append("<img id=\"thumbPreviewImg\" src=\"\" style=\"width:100%; height:100%; object-fit:cover; display:none;\" />");
                builder.Append("<span id=\"thumbPlaceholderText\" style=\"color:var(--muted); font-size:0.9rem;\">Xem trước ảnh</span>");
            }
            builder.Append("</div></div>");
            
            // Cột phải: Upload kéo thả
            builder.Append("<div class=\"profile-upload-zone\" id=\"uploadZone\" style=\"height: 100%; min-height: 145px; display: flex; align-items: center; justify-content: center;\">");
            builder.Append("<div class=\"profile-upload-inner\">");
            builder.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"28\" height=\"28\" fill=\"none\" viewBox=\"0 0 24 24\"><path stroke=\"currentColor\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"1.8\" d=\"M4 16v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2M12 12V4m0 0-3 3m3-3 3 3\"/></svg>");
            builder.Append("<p style=\"margin: 4px 0;\">Kéo thả hoặc <label for=\"inp_thumbFile\" class=\"upload-link\">chọn ảnh</label></p>");
            builder.Append("<input id=\"inp_thumbFile\" name=\"thumbFile\" type=\"file\" accept=\"image/jpeg,image/png,image/gif,image/webp\" style=\"display:none;\" onchange=\"handleFileChange(this)\" />");
            builder.Append("<span id=\"uploadFileName\" class=\"upload-filename\"></span>");
            builder.Append("</div></div></div></div>");

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
            builder.Append("<script src=\"https://cdn.ckeditor.com/ckeditor5/41.4.2/classic/ckeditor.js\"></script>");
            builder.Append(@"<script>
ClassicEditor.create(document.querySelector('#newsContent'),{ckfinder:{uploadUrl:'../Handlers/UploadImage.ashx'}}).catch(function(e){console.error(e);});

function previewThumbnail(url) {
    var img = document.getElementById('thumbPreviewImg');
    var txt = document.getElementById('thumbPlaceholderText');
    if (!img || !txt) return;
    if (url && url.trim().length > 4) {
        img.src = url.trim();
        img.style.display = '';
        txt.style.display = 'none';
        img.onerror = function() { img.style.display='none'; txt.style.display='grid'; };
    } else {
        img.style.display = 'none';
        txt.style.display = '';
    }
}

function handleFileChange(input) {
    var label = document.getElementById('uploadFileName');
    var zone  = document.getElementById('uploadZone');
    if (input.files && input.files[0]) {
        var file = input.files[0];
        label.textContent = file.name;
        zone.classList.add('has-file');
        var reader = new FileReader();
        reader.onload = function(e) {
            var img = document.getElementById('thumbPreviewImg');
            var txt = document.getElementById('thumbPlaceholderText');
            if (!img || !txt) return;
            img.src = e.target.result;
            img.style.display = '';
            txt.style.display = 'none';
            img.onerror = null;
        };
        reader.readAsDataURL(file);
    } else {
        label.textContent = '';
        zone.classList.remove('has-file');
    }
}

(function() {
    var zone = document.getElementById('uploadZone');
    if (!zone) return;
    zone.addEventListener('dragover', function(e) { e.preventDefault(); zone.classList.add('drag-over'); });
    zone.addEventListener('dragleave', function() { zone.classList.remove('drag-over'); });
    zone.addEventListener('drop', function(e) {
        e.preventDefault();
        zone.classList.remove('drag-over');
        var input = document.getElementById('inp_thumbFile');
        if (e.dataTransfer.files.length) {
            input.files = e.dataTransfer.files;
            handleFileChange(input);
        }
    });
    zone.addEventListener('click', function(e) {
        if (e.target.tagName !== 'LABEL' && e.target.tagName !== 'INPUT') {
            document.getElementById('inp_thumbFile').click();
        }
    });
})();
</script>");
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

                // Xử lý upload thumbnail file nếu có
                var thumbFile = Request.Files["thumbFile"];
                if (thumbFile != null && thumbFile.ContentLength > 0)
                {
                    string uploadError;
                    if (!FileUploadHelper.IsValidImage(thumbFile, 5, out uploadError))
                    {
                        _result = OperationResult.Fail(uploadError);
                        return;
                    }

                    var serverFolder = Server.MapPath("~/Static/uploads/news/");
                    thumbnail = FileUploadHelper.SaveImage(thumbFile, serverFolder, "~/Static/uploads/news");
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
