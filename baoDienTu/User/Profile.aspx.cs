using System;
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
            var phone = (Request.Form["phone"] ?? string.Empty).Trim();
            var avatar = (Request.Form["avatar"] ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fullName))
            {
                _result = OperationResult.Fail("Họ tên không được để trống.");
                return;
            }

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

            sb.Append(@"<div class=""page-shell profile-shell""><div class=""container-xl"">");
            sb.Append(@"<section class=""profile-hero"" aria-labelledby=""profileHeroTitle"">");
            sb.Append(@"<div class=""profile-hero-copy"">");
            sb.Append(@"<span class=""profile-kicker"">Tài khoản độc giả</span>");
            sb.Append(@"<h1 id=""profileHeroTitle"">Hồ sơ cá nhân</h1>");
            sb.Append(@"<p>Quản lý thông tin hiển thị, ảnh đại diện và dữ liệu liên hệ của bạn trên Báo Điện Tử.</p>");
            sb.Append("</div>");
            sb.Append(@"<div class=""profile-hero-actions"">");
            sb.Append("<a class=\"btn-soft profile-hero-link\" href=\"" + UiHelper.Attr(ResolveUrl("~/Default.aspx")) + "\">Về trang chủ</a>");
            sb.Append("</div>");
            sb.Append("</section>");

            sb.Append(@"<div class=""profile-page-layout"">");
            sb.Append(RenderSidebar(user, avatarUrl, initials));

            sb.Append(@"<section class=""profile-main"" aria-labelledby=""profileFormTitle"">");
            sb.Append(@"<section class=""profile-card profile-form-card"">");
            sb.Append(@"<div class=""profile-card-head"">");
            sb.Append(@"<div>");
            sb.Append(@"<span class=""profile-section-label"">Thông tin</span>");
            sb.Append(@"<h2 id=""profileFormTitle"" class=""profile-card-title"">Cập nhật hồ sơ</h2>");
            sb.Append(@"<p class=""profile-card-subtitle"">Thay đổi họ tên, số điện thoại hoặc ảnh đại diện đang dùng cho tài khoản.</p>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append(UiHelper.Alert(_result));
            sb.Append(@"<input type=""hidden"" name=""saveProfile"" value=""1"" />");

            sb.Append(@"<div class=""profile-form-grid"">");
            sb.Append(@"<div class=""field profile-field"">");
            sb.Append(@"<label for=""inp_fullName"">Họ và tên <span class=""req"">*</span></label>");
            sb.Append("<input id=\"inp_fullName\" name=\"fullName\" value=\"" + UiHelper.Attr(user.FullName) + "\" required placeholder=\"Nhập họ và tên\" autocomplete=\"name\" />");
            sb.Append(@"<small class=""field-hint"">Tên này sẽ hiển thị ở thanh chào và khu vực tài khoản.</small>");
            sb.Append("</div>");

            sb.Append(@"<div class=""field profile-field"">");
            sb.Append(@"<label for=""inp_phone"">Số điện thoại</label>");
            sb.Append("<input id=\"inp_phone\" name=\"phone\" type=\"tel\" value=\"" + UiHelper.Attr(user.Phone) + "\" placeholder=\"Ví dụ: 0901234567\" autocomplete=\"tel\" />");
            sb.Append(@"<small class=""field-hint"">Chỉ dùng cho thông tin liên hệ nội bộ.</small>");
            sb.Append("</div>");

            sb.Append(@"<div class=""field profile-field profile-field-wide"">");
            sb.Append(@"<label for=""inp_avatar"">URL ảnh đại diện</label>");
            sb.Append("<input id=\"inp_avatar\" name=\"avatar\" value=\"" + UiHelper.Attr(user.Avatar) + "\" placeholder=\"https://...\" oninput=\"previewAvatarUrl(this.value)\" />");
            sb.Append(@"<small class=""field-hint"">Bạn có thể nhập đường dẫn ảnh hoặc tải ảnh mới ở bên dưới.</small>");
            sb.Append("</div>");

            sb.Append(@"<div class=""profile-field-wide"">");
            sb.Append(@"<div class=""profile-upload-zone"" id=""uploadZone"" role=""button"" tabindex=""0"" aria-describedby=""uploadHint"">");
            sb.Append(@"<div class=""profile-upload-inner"">");
            sb.Append(@"<span class=""profile-upload-icon"" aria-hidden=""true"">" + IconUpload() + "</span>");
            sb.Append(@"<span class=""profile-upload-title"">Kéo thả ảnh vào đây</span>");
            sb.Append(@"<span id=""uploadHint"" class=""profile-upload-note"">JPG, PNG, GIF hoặc WEBP, tối đa 3 MB.</span>");
            sb.Append(@"<label for=""inp_avatarFile"" class=""upload-link"">Chọn ảnh từ máy</label>");
            sb.Append(@"<input id=""inp_avatarFile"" name=""avatarFile"" type=""file"" accept=""image/jpeg,image/png,image/gif,image/webp"" onchange=""handleFileChange(this)"" />");
            sb.Append(@"<span id=""uploadFileName"" class=""upload-filename"" aria-live=""polite""></span>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append(@"<div class=""profile-actions"">");
            sb.Append(@"<button class=""btn-main profile-primary"" type=""submit"" id=""btnSaveProfile"">" + IconSave() + "<span>Lưu hồ sơ</span></button>");
            sb.Append(@"<a class=""btn-soft"" href=""ChangePassword.aspx"">Đổi mật khẩu</a>");
            sb.Append("</div>");

            sb.Append("</section>");
            sb.Append("</section>");
            sb.Append("</div>");
            sb.Append("</div></div>");

            sb.Append(@"<script>
function previewAvatarUrl(url) {
    var img = document.getElementById('profileAvatarImg');
    var init = document.getElementById('avatarInitials');
    if (!img || !init) return;
    if (url && url.trim().length > 4) {
        img.src = url.trim();
        img.style.display = '';
        init.style.display = 'none';
        img.onerror = function() { img.style.display = 'none'; init.style.display = 'grid'; };
    } else {
        img.style.display = 'none';
        init.style.display = 'grid';
    }
}

function handleFileChange(input) {
    var label = document.getElementById('uploadFileName');
    var zone = document.getElementById('uploadZone');
    if (input.files && input.files[0]) {
        var file = input.files[0];
        label.textContent = file.name;
        zone.classList.add('has-file');
        var reader = new FileReader();
        reader.onload = function(e) {
            var img = document.getElementById('profileAvatarImg');
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
    zone.addEventListener('keydown', function(e) {
        if (e.key === 'Enter' || e.key === ' ') {
            e.preventDefault();
            document.getElementById('inp_avatarFile').click();
        }
    });
})();
</script>");

            return sb.ToString();
        }

        private static string RenderSidebar(UserModel user, string avatarUrl, string initials)
        {
            var sb = new StringBuilder();
            sb.Append(@"<aside class=""profile-sidebar"" aria-label=""Tài khoản"">");
            sb.Append(@"<section class=""profile-account-card"">");
            sb.Append(@"<div class=""profile-avatar-frame"">");

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

            sb.Append("</div>");
            sb.Append(@"<div class=""profile-person"">");
            sb.Append("<h2 class=\"profile-avatar-name\">" + UiHelper.E(user.FullName ?? user.Username) + "</h2>");
            sb.Append("<span class=\"profile-avatar-role\">" + UiHelper.E(user.RoleName) + "</span>");

            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                sb.Append("<span class=\"profile-avatar-email\">" + UiHelper.E(user.Email) + "</span>");
            }

            sb.Append("</div>");
            sb.Append("</section>");

            sb.Append(@"<section class=""profile-sidebar-info"" aria-label=""Thông tin tài khoản"">");
            sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Tên đăng nhập</span><span class=\"profile-info-val\">" + UiHelper.E(user.Username) + "</span></div>");
            sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Ngày tham gia</span><span class=\"profile-info-val\">" + UiHelper.E(user.CreatedAt.ToString("dd/MM/yyyy")) + "</span></div>");
            sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Đăng nhập gần nhất</span><span class=\"profile-info-val\">" + UiHelper.E(user.LastLogin.HasValue ? user.LastLogin.Value.ToString("dd/MM/yyyy HH:mm") : "Chưa ghi nhận") + "</span></div>");
            sb.Append("</section>");

            sb.Append(@"<nav class=""profile-sidebar-links"" aria-label=""Thiết lập tài khoản"">");
            sb.Append(@"<a class=""profile-nav-link profile-nav-active"" href=""Profile.aspx"">" + IconUser() + "<span>Hồ sơ cá nhân</span></a>");
            sb.Append(@"<a class=""profile-nav-link"" href=""ChangePassword.aspx"">" + IconLock() + "<span>Đổi mật khẩu</span></a>");
            sb.Append("</nav>");
            sb.Append("</aside>");
            return sb.ToString();
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
            return (parts[0][0].ToString() + parts[parts.Length - 1][0].ToString()).ToUpper();
        }

        private static string IconUser()
        {
            return Svg(@"<path d=""M20 21a8 8 0 0 0-16 0"" /><path d=""M12 11a4 4 0 1 0 0-8 4 4 0 0 0 0 8"" />");
        }

        private static string IconLock()
        {
            return Svg(@"<rect x=""5"" y=""11"" width=""14"" height=""10"" rx=""2"" /><path d=""M8 11V8a4 4 0 0 1 8 0v3"" />");
        }

        private static string IconSave()
        {
            return Svg(@"<path d=""M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2Z"" /><path d=""M17 21v-8H7v8"" /><path d=""M7 3v5h8"" />");
        }

        private static string IconUpload()
        {
            return Svg(@"<path d=""M12 16V4"" /><path d=""m7 9 5-5 5 5"" /><path d=""M4 16v3a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2v-3"" />");
        }

        private static string Svg(string body)
        {
            return @"<svg class=""ui-icon"" xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""1.9"" stroke-linecap=""round"" stroke-linejoin=""round"" aria-hidden=""true"">" + body + "</svg>";
        }
    }
}
