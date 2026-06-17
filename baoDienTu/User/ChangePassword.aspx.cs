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
            var oldPassword     = Request.Form["oldPassword"]     ?? string.Empty;
            var newPassword     = Request.Form["newPassword"]     ?? string.Empty;
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

            var sb = new StringBuilder();

            sb.Append(@"<div class=""page-shell""><div class=""container-xl""><div class=""profile-page-layout"">");

            // ── Sidebar
            sb.Append(RenderSidebar(user));

            // ── Nội dung chính
            sb.Append(@"<div class=""profile-main"">");
            sb.Append(@"<div class=""profile-card"">");
            sb.Append("<h1 class=\"profile-card-title\">Đổi mật khẩu</h1>");
            sb.Append("<p class=\"profile-card-subtitle\">Nhập mật khẩu hiện tại và chọn mật khẩu mới để bảo vệ tài khoản.</p>");

            sb.Append(UiHelper.Alert(_result));

            sb.Append("<input type=\"hidden\" name=\"changePassword\" value=\"1\" />");

            // Mật khẩu hiện tại
            sb.Append(@"<div class=""field"" style=""margin-bottom:18px"">");
            sb.Append("<label for=\"inp_oldPw\">Mật khẩu hiện tại <span class=\"req\">*</span></label>");
            sb.Append(@"<div class=""pw-input-wrap"">");
            sb.Append("<input id=\"inp_oldPw\" name=\"oldPassword\" type=\"password\" required placeholder=\"Nhập mật khẩu hiện tại...\" autocomplete=\"current-password\" />");
            sb.Append("<button type=\"button\" class=\"pw-toggle\" onclick=\"togglePw('inp_oldPw',this)\" title=\"Hiện/ẩn mật khẩu\">👁</button>");
            sb.Append("</div></div>");

            // Mật khẩu mới
            sb.Append(@"<div class=""field"" style=""margin-bottom:18px"">");
            sb.Append("<label for=\"inp_newPw\">Mật khẩu mới <span class=\"req\">*</span></label>");
            sb.Append(@"<div class=""pw-input-wrap"">");
            sb.Append("<input id=\"inp_newPw\" name=\"newPassword\" type=\"password\" required minlength=\"6\" placeholder=\"Tối thiểu 6 ký tự...\" autocomplete=\"new-password\" oninput=\"checkStrength(this.value)\" />");
            sb.Append("<button type=\"button\" class=\"pw-toggle\" onclick=\"togglePw('inp_newPw',this)\" title=\"Hiện/ẩn mật khẩu\">👁</button>");
            sb.Append("</div>");
            sb.Append(@"<div class=""pw-strength-bar"" id=""pwStrengthBar""><div class=""pw-strength-fill"" id=""pwStrengthFill""></div></div>");
            sb.Append("<span class=\"pw-strength-label\" id=\"pwStrengthLabel\"></span>");
            sb.Append("</div>");

            // Xác nhận mật khẩu mới
            sb.Append(@"<div class=""field"" style=""margin-bottom:18px"">");
            sb.Append("<label for=\"inp_confirmPw\">Xác nhận mật khẩu mới <span class=\"req\">*</span></label>");
            sb.Append(@"<div class=""pw-input-wrap"">");
            sb.Append("<input id=\"inp_confirmPw\" name=\"confirmPassword\" type=\"password\" required minlength=\"6\" placeholder=\"Nhập lại mật khẩu mới...\" autocomplete=\"new-password\" oninput=\"checkMatch()\" />");
            sb.Append("<button type=\"button\" class=\"pw-toggle\" onclick=\"togglePw('inp_confirmPw',this)\" title=\"Hiện/ẩn mật khẩu\">👁</button>");
            sb.Append("</div>");
            sb.Append("<span class=\"pw-match-label\" id=\"pwMatchLabel\"></span>");
            sb.Append("</div>");

            // Gợi ý bảo mật
            sb.Append(@"<div class=""pw-tips"">");
            sb.Append("<strong>Gợi ý mật khẩu mạnh:</strong>");
            sb.Append("<ul><li>Ít nhất 8 ký tự</li><li>Kết hợp chữ hoa, chữ thường</li><li>Bao gồm số và ký tự đặc biệt (!@#$...)</li></ul>");
            sb.Append("</div>");

            sb.Append(@"<div class=""btn-row"" style=""margin-top:24px"">");
            sb.Append("<button class=\"btn-main\" type=\"submit\" id=\"btnChangePw\">🔒 Đổi mật khẩu</button>");
            sb.Append("<a class=\"btn-soft\" href=\"Profile.aspx\">← Quay lại hồ sơ</a>");
            sb.Append("</div>");

            sb.Append("</div></div>"); // profile-card + profile-main
            sb.Append("</div></div></div>"); // layout + container + page-shell

            // Scripts
            sb.Append(@"<script>
function togglePw(id, btn) {
    var inp = document.getElementById(id);
    if (!inp) return;
    inp.type = inp.type === 'password' ? 'text' : 'password';
    btn.textContent = inp.type === 'password' ? '👁' : '🙈';
}

function checkStrength(val) {
    var fill  = document.getElementById('pwStrengthFill');
    var label = document.getElementById('pwStrengthLabel');
    if (!fill || !label) return;
    var score = 0;
    if (val.length >= 6)  score++;
    if (val.length >= 8)  score++;
    if (/[A-Z]/.test(val) && /[a-z]/.test(val)) score++;
    if (/\d/.test(val))   score++;
    if (/[^A-Za-z0-9]/.test(val)) score++;
    var pct = (score / 5) * 100;
    fill.style.width = pct + '%';
    var colors = ['#ef4444','#f97316','#eab308','#84cc16','#22c55e'];
    var labels = ['Rất yếu','Yếu','Trung bình','Mạnh','Rất mạnh'];
    fill.style.background = colors[score - 1] || '#ef4444';
    label.textContent = score > 0 ? labels[score - 1] : '';
    label.style.color = colors[score - 1] || '#ef4444';
}

function checkMatch() {
    var pw1   = document.getElementById('inp_newPw');
    var pw2   = document.getElementById('inp_confirmPw');
    var label = document.getElementById('pwMatchLabel');
    if (!pw1 || !pw2 || !label) return;
    if (pw2.value.length === 0) { label.textContent = ''; return; }
    if (pw1.value === pw2.value) {
        label.textContent = '✓ Mật khẩu khớp';
        label.style.color = '#16a34a';
    } else {
        label.textContent = '✗ Chưa khớp';
        label.style.color = '#dc2626';
    }
}
</script>");

            return sb.ToString();
        }

        private static string RenderSidebar(UserModel user)
        {
            var avatarUrl = string.IsNullOrWhiteSpace(user.Avatar)
                ? string.Empty
                : UiHelper.ResolveImageUrl(user.Avatar);
            var initials = GetInitials(user.FullName ?? user.Username);

            var sb = new StringBuilder();
            sb.Append(@"<aside class=""profile-sidebar"">");
            sb.Append(@"<div class=""profile-avatar-card"">");

            if (!string.IsNullOrWhiteSpace(avatarUrl))
            {
                sb.Append("<img class=\"profile-avatar-img\" src=\"" + UiHelper.Attr(avatarUrl) + "\" alt=\"Ảnh đại diện\" onerror=\"this.style.display='none';document.getElementById('avi').style.display='grid';\" />");
                sb.Append("<div id=\"avi\" class=\"profile-avatar-initials\" style=\"display:none;\">" + UiHelper.E(initials) + "</div>");
            }
            else
            {
                sb.Append("<div class=\"profile-avatar-initials\">" + UiHelper.E(initials) + "</div>");
            }

            sb.Append("<div class=\"profile-avatar-name\">" + UiHelper.E(user.FullName ?? user.Username) + "</div>");
            sb.Append("<div class=\"profile-avatar-role\">" + UiHelper.E(user.RoleName) + "</div>");
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                sb.Append("<div class=\"profile-avatar-email\">" + UiHelper.E(user.Email) + "</div>");
            }
            sb.Append("</div>"); // avatar-card

            sb.Append("<div class=\"profile-sidebar-info\">");
            sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Ngày tham gia</span><span class=\"profile-info-val\">" + UiHelper.E(user.CreatedAt.ToString("dd/MM/yyyy")) + "</span></div>");
            if (user.LastLogin.HasValue)
            {
                sb.Append("<div class=\"profile-info-row\"><span class=\"profile-info-label\">Đăng nhập gần nhất</span><span class=\"profile-info-val\">" + UiHelper.E(user.LastLogin.Value.ToString("dd/MM/yyyy HH:mm")) + "</span></div>");
            }
            sb.Append("</div>");

            sb.Append("<div class=\"profile-sidebar-links\">");
            sb.Append("<a class=\"profile-nav-link\" href=\"Profile.aspx\">🧑 Hồ sơ cá nhân</a>");
            sb.Append("<a class=\"profile-nav-link profile-nav-active\" href=\"ChangePassword.aspx\">🔒 Đổi mật khẩu</a>");
            sb.Append("</div>");

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
    }
}
