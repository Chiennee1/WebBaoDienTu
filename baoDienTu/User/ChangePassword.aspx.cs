using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.User
{
    public partial class ChangePassword : Page
    {
        private OperationResult _result;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireLogin(this);

            if (Request.HttpMethod == "POST" && Request.Form["changePassword"] == "1")
            {
                HandleChange();
            }
        }

        private void HandleChange()
        {
            var oldPassword = Request.Form["oldPassword"] ?? string.Empty;
            var newPassword = Request.Form["newPassword"] ?? string.Empty;
            var confirmPassword = Request.Form["confirmPassword"] ?? string.Empty;

            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                _result = OperationResult.Fail("Vui lòng nhập mật khẩu hiện tại.");
                return;
            }

            if (newPassword.Length < 6)
            {
                _result = OperationResult.Fail("Mật khẩu mới phải có ít nhất 6 ký tự.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                _result = OperationResult.Fail("Mật khẩu xác nhận không khớp.");
                return;
            }

            _result = UserBLL.ChangePassword(AuthGuard.CurrentUserId, oldPassword, newPassword);
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
            sb.Append(@"<section class=""profile-hero"" aria-labelledby=""passwordHeroTitle"">");
            sb.Append(@"<div class=""profile-hero-copy"">");
            sb.Append(@"<span class=""profile-kicker"">Bảo mật tài khoản</span>");
            sb.Append(@"<h1 id=""passwordHeroTitle"">Đổi mật khẩu</h1>");
            sb.Append(@"<p>Cập nhật mật khẩu định kỳ để bảo vệ tài khoản đọc tin, bình luận và nhận newsletter.</p>");
            sb.Append("</div>");
            sb.Append(@"<div class=""profile-hero-actions"">");
            sb.Append(@"<a class=""btn-soft profile-hero-link"" href=""Profile.aspx"">Quay lại hồ sơ</a>");
            sb.Append("</div>");
            sb.Append("</section>");

            sb.Append(@"<div class=""profile-page-layout"">");
            sb.Append(RenderSidebar(user, avatarUrl, initials));

            sb.Append(@"<section class=""profile-main"" aria-labelledby=""passwordFormTitle"">");
            sb.Append(@"<section class=""profile-card profile-form-card"">");
            sb.Append(@"<div class=""profile-card-head"">");
            sb.Append(@"<div>");
            sb.Append(@"<span class=""profile-section-label"">Mật khẩu</span>");
            sb.Append(@"<h2 id=""passwordFormTitle"" class=""profile-card-title"">Thiết lập mật khẩu mới</h2>");
            sb.Append(@"<p class=""profile-card-subtitle"">Nhập mật khẩu hiện tại, sau đó chọn mật khẩu mới đủ mạnh và dễ nhớ với bạn.</p>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append(UiHelper.Alert(_result));
            sb.Append(@"<input type=""hidden"" name=""changePassword"" value=""1"" />");

            sb.Append(@"<div class=""profile-password-grid"">");
            sb.Append(@"<div class=""profile-form-stack"">");

            sb.Append(@"<div class=""field profile-field"">");
            sb.Append(@"<label for=""inp_oldPw"">Mật khẩu hiện tại <span class=""req"">*</span></label>");
            sb.Append(@"<div class=""pw-input-wrap"">");
            sb.Append(@"<input id=""inp_oldPw"" name=""oldPassword"" type=""password"" required placeholder=""Nhập mật khẩu hiện tại"" autocomplete=""current-password"" />");
            sb.Append(@"<button type=""button"" class=""pw-toggle"" onclick=""togglePw('inp_oldPw',this)"" aria-label=""Hiện mật khẩu"" aria-pressed=""false"">" + IconEye() + "</button>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append(@"<div class=""field profile-field"">");
            sb.Append(@"<label for=""inp_newPw"">Mật khẩu mới <span class=""req"">*</span></label>");
            sb.Append(@"<div class=""pw-input-wrap"">");
            sb.Append(@"<input id=""inp_newPw"" name=""newPassword"" type=""password"" required minlength=""6"" placeholder=""Tối thiểu 6 ký tự"" autocomplete=""new-password"" oninput=""checkStrength(this.value);checkMatch();"" />");
            sb.Append(@"<button type=""button"" class=""pw-toggle"" onclick=""togglePw('inp_newPw',this)"" aria-label=""Hiện mật khẩu"" aria-pressed=""false"">" + IconEye() + "</button>");
            sb.Append("</div>");
            sb.Append(@"<div class=""pw-strength"" aria-live=""polite"">");
            sb.Append(@"<div class=""pw-strength-bar""><div class=""pw-strength-fill"" id=""pwStrengthFill""></div></div>");
            sb.Append(@"<span class=""pw-strength-label"" id=""pwStrengthLabel""></span>");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append(@"<div class=""field profile-field"">");
            sb.Append(@"<label for=""inp_confirmPw"">Xác nhận mật khẩu mới <span class=""req"">*</span></label>");
            sb.Append(@"<div class=""pw-input-wrap"">");
            sb.Append(@"<input id=""inp_confirmPw"" name=""confirmPassword"" type=""password"" required minlength=""6"" placeholder=""Nhập lại mật khẩu mới"" autocomplete=""new-password"" oninput=""checkMatch()"" />");
            sb.Append(@"<button type=""button"" class=""pw-toggle"" onclick=""togglePw('inp_confirmPw',this)"" aria-label=""Hiện mật khẩu"" aria-pressed=""false"">" + IconEye() + "</button>");
            sb.Append("</div>");
            sb.Append(@"<span class=""pw-match-label"" id=""pwMatchLabel"" aria-live=""polite""></span>");
            sb.Append("</div>");

            sb.Append("</div>");

            sb.Append(@"<aside class=""security-panel"" aria-label=""Gợi ý bảo mật"">");
            sb.Append(@"<span class=""security-panel-icon"" aria-hidden=""true"">" + IconShield() + "</span>");
            sb.Append(@"<h3>Gợi ý mật khẩu mạnh</h3>");
            sb.Append(@"<p>Ưu tiên mật khẩu riêng cho tài khoản này, không trùng email hoặc mạng xã hội.</p>");
            sb.Append(@"<div class=""security-checklist"">");
            sb.Append(@"<span>Ít nhất 8 ký tự</span>");
            sb.Append(@"<span>Có chữ hoa và chữ thường</span>");
            sb.Append(@"<span>Có số hoặc ký tự đặc biệt</span>");
            sb.Append("</div>");
            sb.Append("</aside>");
            sb.Append("</div>");

            sb.Append(@"<div class=""profile-actions"">");
            sb.Append(@"<button class=""btn-main profile-primary"" type=""submit"" id=""btnChangePw"">" + IconLock() + "<span>Đổi mật khẩu</span></button>");
            sb.Append(@"<a class=""btn-soft"" href=""Profile.aspx"">Hồ sơ cá nhân</a>");
            sb.Append("</div>");

            sb.Append("</section>");
            sb.Append("</section>");
            sb.Append("</div>");
            sb.Append("</div></div>");

            sb.Append(@"<script>
function togglePw(id, btn) {
    var inp = document.getElementById(id);
    if (!inp) return;
    var show = inp.type === 'password';
    inp.type = show ? 'text' : 'password';
    btn.classList.toggle('is-visible', show);
    btn.setAttribute('aria-label', show ? 'Ẩn mật khẩu' : 'Hiện mật khẩu');
    btn.setAttribute('aria-pressed', show ? 'true' : 'false');
}

function checkStrength(val) {
    var fill = document.getElementById('pwStrengthFill');
    var label = document.getElementById('pwStrengthLabel');
    if (!fill || !label) return;

    var score = 0;
    if (val.length >= 6) score++;
    if (val.length >= 8) score++;
    if (/[A-Z]/.test(val) && /[a-z]/.test(val)) score++;
    if (/\d/.test(val)) score++;
    if (/[^A-Za-z0-9]/.test(val)) score++;

    var labels = ['', 'Rất yếu', 'Yếu', 'Trung bình', 'Mạnh', 'Rất mạnh'];
    fill.style.width = score === 0 ? '0' : ((score / 5) * 100) + '%';
    fill.className = 'pw-strength-fill strength-' + score;
    label.className = 'pw-strength-label strength-' + score;
    label.textContent = labels[score] || '';
}

function checkMatch() {
    var pw1 = document.getElementById('inp_newPw');
    var pw2 = document.getElementById('inp_confirmPw');
    var label = document.getElementById('pwMatchLabel');
    if (!pw1 || !pw2 || !label) return;
    label.className = 'pw-match-label';
    if (pw2.value.length === 0) {
        label.textContent = '';
        return;
    }
    if (pw1.value === pw2.value) {
        label.textContent = 'Mật khẩu xác nhận đã khớp';
        label.classList.add('is-match');
    } else {
        label.textContent = 'Mật khẩu xác nhận chưa khớp';
        label.classList.add('is-mismatch');
    }
}
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
                sb.Append("<img class=\"profile-avatar-img\" src=\"" + UiHelper.Attr(avatarUrl) + "\" alt=\"Ảnh đại diện\" onerror=\"this.style.display='none';document.getElementById('profileSidebarInitials').style.display='grid';\" />");
                sb.Append("<div id=\"profileSidebarInitials\" class=\"profile-avatar-initials\" style=\"display:none;\">" + UiHelper.E(initials) + "</div>");
            }
            else
            {
                sb.Append("<div id=\"profileSidebarInitials\" class=\"profile-avatar-initials\">" + UiHelper.E(initials) + "</div>");
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
            sb.Append(@"<a class=""profile-nav-link"" href=""Profile.aspx"">" + IconUser() + "<span>Hồ sơ cá nhân</span></a>");
            sb.Append(@"<a class=""profile-nav-link profile-nav-active"" href=""ChangePassword.aspx"">" + IconLock() + "<span>Đổi mật khẩu</span></a>");
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

        private static string IconEye()
        {
            return Svg(@"<path d=""M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z"" /><circle cx=""12"" cy=""12"" r=""3"" />");
        }

        private static string IconShield()
        {
            return Svg(@"<path d=""M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z"" /><path d=""m9 12 2 2 4-5"" />");
        }

        private static string Svg(string body)
        {
            return @"<svg class=""ui-icon"" xmlns=""http://www.w3.org/2000/svg"" width=""20"" height=""20"" viewBox=""0 0 24 24"" fill=""none"" stroke=""currentColor"" stroke-width=""1.9"" stroke-linecap=""round"" stroke-linejoin=""round"" aria-hidden=""true"">" + body + "</svg>";
        }
    }
}
