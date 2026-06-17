using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.User
{
    public partial class Profile : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireLogin(this);

            if (Request.HttpMethod == "POST" && Request.Form["saveProfile"] == "1")
            {
                HandleSave();
            }
        }

        private void HandleSave()
        {
            var fullName = (Request.Form["fullName"] ?? string.Empty).Trim();
            var phone    = (Request.Form["phone"]    ?? string.Empty).Trim();
            var avatar   = (Request.Form["avatar"]   ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                _result = OperationResult.Fail("Họ tên không được để trống.");
                return;
            }

            // Xử lý upload avatar nếu có file
            var avatarFile = Request.Files["avatarFile"];
            if (avatarFile != null && avatarFile.ContentLength > 0)
            {
                string uploadError;
                if (!FileUploadHelper.IsValidImage(avatarFile, 3, out uploadError))
                {
                    _result = OperationResult.Fail(uploadError);
                    return;
                }

                var serverFolder = Server.MapPath("~/Static/images/avatars/");
                avatar = FileUploadHelper.SaveImage(avatarFile, serverFolder, "~/Static/images/avatars");
            }

            _result = UserBLL.UpdateProfile(AuthGuard.CurrentUserId, fullName, phone, avatar);

            // Cập nhật lại tên trong session
            if (_result != null && _result.Success)
            {
                HttpContext.Current.Session["CurrentFullName"] = fullName;
            }
        }

        protected string RenderPage()
        {
            var user = UserBLL.GetById(AuthGuard.CurrentUserId);
            if (user == null)
            {
                Response.Redirect("~/Login.aspx", true);
                return string.Empty;
            }

            var avatarUrl = string.IsNullOrWhiteSpace(user.Avatar)
                ? string.Empty
                : UiHelper.ResolveImageUrl(user.Avatar);

            var initials = GetInitials(user.FullName ?? user.Username);

            var sb = new StringBuilder();

            sb.Append(@"<div class=""page-shell""><div class=""container-xl""><div class=""profile-page-layout"">");

            // ── Sidebar / avatar card
            sb.Append(@"<aside class=""profile-sidebar"">");
            sb.Append(@"<div class=""profile-avatar-card"">");

            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                sb.Append("<img id=\"profileAvatarImg\" class=\"profile-avatar-img\" src=\"" + UiHelper.Attr(avatarUrl) + "\" alt=\"Ảnh đại diện\" onerror=\"this.style.display='none';document.getElementById('avatarInitials').style.display='grid';\" />");
                sb.Append("<div id=\"avatarInitials\" class=\"profile-avatar-initials\" style=\"display:none;\">" + UiHelper.E(initials) + "</div>");
            }
            else
            {
                sb.Append("<img id=\"profileAvatarImg\" class=\"profile-avatar-img\" src=\"\" alt=\"\" style=\"display:none;\" />");
                sb.Append("<div id=\"avatarInitials\" class=\"profile-avatar-initials\">" + UiHelper.E(initials) + "</div>");
            }

            sb.Append("<div class=\"profile-avatar-name\">" + UiHelper.E(user.FullName ?? user.Username) + "</div>");
            sb.Append("<div class=\"profile-avatar-role\">" + UiHelper.E(user.RoleName) + "</div>");

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                sb.Append("<div class=\"profile-avatar-email\">" + UiHelper.E(user.Email) + "</div>");
            }

            sb.Append("</div>"); // avatar-card

            // Thông tin thêm
            sb.Append("<div class=\"profile-sidebar-info\">");
            sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Ngày tham gia</span><span class=\"profile-info-val\">" + UiHelper.E(user.CreatedAt.ToString("dd/MM/yyyy")) + "</span></div>");
            if (user.LastLogin.HasValue)
            {
                sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Đăng nhập gần nhất</span><span class=\"profile-info-val\">" + UiHelper.E(user.LastLogin.Value.ToString("dd/MM/yyyy HH:mm")) + "</span></div>");
            }
            sb.Append("</div>"); // sidebar-info

            sb.Append(@"<div class=""profile-sidebar-links"">");
            sb.Append("<a class=\"profile-nav-link profile-nav-active\" href=\"Profile.aspx\">🧑 Hồ sơ cá nhân</a>");
            sb.Append("<a class=\"profile-nav-link\" href=\"ChangePassword.aspx\">🔒 Đổi mật khẩu</a>");
            sb.Append("</div>");

            sb.Append("</aside>"); // sidebar

            // ── Nội dung chính (form)
            sb.Append(@"<div class=""profile-main"">");
            sb.Append(@"<div class=""profile-card"">");
            sb.Append("<h1 class=\"profile-card-title\">Cập nhật hồ sơ</h1>");
            sb.Append("<p class=\"profile-card-subtitle\">Thay đổi thông tin cá nhân của bạn bên dưới.</p>");

            sb.Append(UiHelper.Alert(_result));

            sb.Append("<input type=\"hidden\" name=\"saveProfile\" value=\"1\" />");

            // Họ tên
            sb.Append(@"<div class=""field"" style=""margin-bottom:18px"">");
            sb.Append("<label for=\"inp_fullName\">Họ và tên <span class=\"req\">*</span></label>");
            sb.Append("<input id=\"inp_fullName\" name=\"fullName\" value=\"" + UiHelper.Attr(user.FullName) + "\" required placeholder=\"Nhập họ và tên...\" />");
            sb.Append("</div>");

            // Số điện thoại
            sb.Append(@"<div class=""field"" style=""margin-bottom:18px"">");
            sb.Append("<label for=\"inp_phone\">Số điện thoại</label>");
            sb.Append("<input id=\"inp_phone\" name=\"phone\" type=\"tel\" value=\"" + UiHelper.Attr(user.Phone) + "\" placeholder=\"Nhập số điện thoại...\" />");
            sb.Append("</div>");

            // Avatar URL
            sb.Append(@"<div class=""field"" style=""margin-bottom:18px"">");
            sb.Append("<label for=\"inp_avatar\">Avatar – URL ảnh (hoặc upload bên dưới)</label>");
            sb.Append("<input id=\"inp_avatar\" name=\"avatar\" value=\"" + UiHelper.Attr(user.Avatar) + "\" placeholder=\"https://...\" oninput=\"previewAvatarUrl(this.value)\" />");
            sb.Append("</div>");

            // Upload file
            sb.Append(@"<div class=""profile-upload-zone"" id=""uploadZone"">");
            sb.Append(@"<div class=""profile-upload-inner"">");
            sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"28\" height=\"28\" fill=\"none\" viewBox=\"0 0 24 24\"><path stroke=\"currentColor\" stroke-linecap=\"round\" stroke-linejoin=\"round\" stroke-width=\"1.8\" d=\"M4 16v2a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-2M12 12V4m0 0-3 3m3-3 3 3\"/></svg>");
            sb.Append("<p>Kéo thả hoặc <label for=\"inp_avatarFile\" class=\"upload-link\">chọn ảnh</label> (JPG, PNG, WEBP, tối đa 3 MB)</p>");
            sb.Append("<input id=\"inp_avatarFile\" name=\"avatarFile\" type=\"file\" accept=\"image/jpeg,image/png,image/gif,image/webp\" style=\"display:none;\" onchange=\"handleFileChange(this)\" />");
            sb.Append("<span id=\"uploadFileName\" class=\"upload-filename\"></span>");
            sb.Append("</div>");
            sb.Append("</div>"); // upload-zone

            sb.Append(@"<div class=""btn-row"" style=""margin-top:24px"">");
            sb.Append("<button class=\"btn-main\" type=\"submit\" id=\"btnSaveProfile\">💾 Lưu hồ sơ</button>");
            sb.Append("</div>");

            sb.Append("</div>"); // profile-card
            sb.Append("</div>"); // profile-main
            sb.Append("</div>"); // layout
            sb.Append("</div></div>"); // page-shell + container

            // Script cho preview avatar và upload zone
            sb.Append(@"<script>
function previewAvatarUrl(url) {
    var img = document.getElementById('profileAvatarImg');
    var init = document.getElementById('avatarInitials');
    if (!img || !init) return;
    if (url && url.trim().length > 4) {
        img.src = url.trim();
        img.style.display = '';
        init.style.display = 'none';
        img.onerror = function() { img.style.display='none'; init.style.display='grid'; };
    } else {
        img.style.display = 'none';
        init.style.display = 'grid';
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
            var img  = document.getElementById('profileAvatarImg');
            var init = document.getElementById('avatarInitials');
            if (!img || !init) return;
            img.src = e.target.result;
            img.style.display = '';
            init.style.display = 'none';
            img.onerror = null;
        };
        reader.readAsDataURL(file);
    } else {
        label.textContent = '';
        zone.classList.remove('has-file');
    }
}

// Drag-and-drop cho upload zone
(function() {
    var zone = document.getElementById('uploadZone');
    if (!zone) return;
    zone.addEventListener('dragover', function(e) { e.preventDefault(); zone.classList.add('drag-over'); });
    zone.addEventListener('dragleave', function() { zone.classList.remove('drag-over'); });
    zone.addEventListener('drop', function(e) {
        e.preventDefault();
        zone.classList.remove('drag-over');
        var input = document.getElementById('inp_avatarFile');
        if (e.dataTransfer.files.length) {
            input.files = e.dataTransfer.files;
            handleFileChange(input);
        }
    });
    zone.addEventListener('click', function(e) {
        if (e.target.tagName !== 'LABEL' && e.target.tagName !== 'INPUT') {
            document.getElementById('inp_avatarFile').click();
        }
    });
})();
</script>");

            return sb.ToString();
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }
    }
}
