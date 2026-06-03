using System;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Pages
{
    public partial class NewsDetail : Page
    {
        private OperationResult _commentResult;

        protected void Page_Load(object sender, EventArgs e)
        {
            UiHelper.DisableBrowserCache();

            if (Request.HttpMethod == "POST" && Request.Form["commentAction"] == "send")
            {
                AddComment();
            }
            else if (Request.QueryString["comment"] == "sent")
            {
                _commentResult = OperationResult.Ok("Bình luận đã được gửi và đang chờ quản trị viên duyệt.");
            }
        }

        protected string RenderPage()
        {
            try
            {
                var slug = Request.QueryString["slug"];
                var news = NewsService.GetDetail(slug);
                if (news == null)
                {
                    return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"empty-state\">Bài viết không tồn tại hoặc chưa được duyệt.</div></div></div>";
                }

                if (!IsPostBack && Request.QueryString["comment"] != "sent")
                {
                    NewsService.IncreaseViewCount(news.NewsID);
                    news.ViewCount++;
                }

                Page.Title = news.Title;
                var related = NewsService.GetRelated(news.NewsID, 5);
                var comments = CommentService.GetApprovedByNews(news.NewsID);
                var tags = TagBLL.GetTagsByNews(news.NewsID);
                var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl content-layout\">");
                builder.Append("<article class=\"article-detail\">");
                builder.Append("<div class=\"article-meta\"><span>" + UiHelper.E(news.CatName) + "</span><span>" + UiHelper.E(UiHelper.Date(news.PublishedAt)) + "</span><span>" + news.ViewCount + " lượt xem</span></div>");
                builder.Append("<h1>" + UiHelper.E(news.Title) + "</h1>");
                builder.Append("<p class=\"article-summary\">" + UiHelper.E(news.Summary) + "</p>");
                builder.Append(UiHelper.ImageBlock(news, string.Empty));
                builder.Append("<div class=\"article-body\">" + news.Content + "</div>");
                if (tags.Count > 0)
                {
                    builder.Append("<div class=\"tag-row\">");
                    foreach (var tag in tags)
                    {
                        builder.Append("<a class=\"tag-pill\" href=\"" + UiHelper.Attr(UiHelper.TagUrl(tag)) + "\">" + UiHelper.E(tag.TagName) + "</a>");
                    }
                    builder.Append("</div>");
                }
                builder.Append("<div class=\"btn-row\" style=\"margin-top:22px\"><a class=\"btn-soft\" href=\"" + UiHelper.Attr(UiHelper.PrintUrl(news)) + "\" target=\"_blank\">In bài viết</a><button class=\"btn-soft\" type=\"button\" onclick=\"toggleShareBox()\">Gửi cho bạn</button></div>");
                builder.Append(RenderShareBox(news));
                builder.Append(RenderComments(news, comments));
                builder.Append("</article>");

                builder.Append("<aside class=\"panel\"><div class=\"section-title\"><h2>Tin liên quan</h2></div>");
                if (related.Count == 0)
                {
                    builder.Append("<p class=\"muted\">Chưa có tin liên quan.</p>");
                }
                else
                {
                    foreach (var item in related)
                    {
                        builder.Append(UiHelper.SmallArticle(item));
                    }
                }
                builder.Append("</aside></div></div>");
                return builder.ToString();
            }
            catch (Exception ex)
            {
                return "<div class=\"page-shell\"><div class=\"container-xl\"><div class=\"app-alert danger\">Không thể tải bài viết: " + UiHelper.E(ex.Message) + "</div></div></div>";
            }
        }

        private string RenderShareBox(NewsModel news)
        {
            var builder = new StringBuilder();
            builder.Append("<div id=\"shareBox\" class=\"form-panel share-box\" aria-live=\"polite\"><h3>Gửi tin cho bạn</h3>");
            builder.Append("<input type=\"hidden\" id=\"shareNewsId\" value=\"" + news.NewsID + "\" />");
            builder.Append("<div class=\"form-grid\"><div class=\"field\"><label for=\"shareSenderName\">Tên của bạn</label><input id=\"shareSenderName\" maxlength=\"100\" /></div>");
            builder.Append("<div class=\"field\"><label for=\"shareSenderEmail\">Email của bạn</label><input id=\"shareSenderEmail\" type=\"email\" maxlength=\"150\" /></div>");
            builder.Append("<div class=\"field\"><label for=\"shareReceiverEmail\">Email người nhận</label><input id=\"shareReceiverEmail\" type=\"email\" maxlength=\"150\" required /></div>");
            builder.Append("<div class=\"field\"><label for=\"shareMessage\">Lời nhắn</label><input id=\"shareMessage\" maxlength=\"500\" /></div></div>");
            builder.Append("<div class=\"btn-row\" style=\"margin-top:14px\"><button id=\"shareSubmit\" class=\"btn-main\" type=\"button\" onclick=\"shareNews()\">Gửi</button><span id=\"shareResult\" class=\"muted share-result\"></span></div></div>");
            builder.Append("<script>");
            builder.Append("function toggleShareBox(){var box=document.getElementById('shareBox');if(box){box.classList.toggle('is-open');}}");
            builder.Append("function shareNews(){var result=document.getElementById('shareResult');var button=document.getElementById('shareSubmit');var receiver=document.getElementById('shareReceiverEmail');if(result){result.className='muted share-result';result.textContent='';}if(!receiver||!receiver.value.trim()||!receiver.checkValidity()){if(result){result.className='share-result danger';result.textContent='Vui lòng nhập email người nhận hợp lệ.';}if(receiver){receiver.focus();}return;}var body=new URLSearchParams();body.append('newsId',(document.getElementById('shareNewsId')||{}).value||'');body.append('senderName',(document.getElementById('shareSenderName')||{}).value||'');body.append('senderEmail',(document.getElementById('shareSenderEmail')||{}).value||'');body.append('receiverEmail',receiver.value||'');body.append('message',(document.getElementById('shareMessage')||{}).value||'');if(button){button.disabled=true;button.textContent='Đang gửi...';}fetch('" + ResolveUrl("~/Handlers/ShareNews.ashx") + "',{method:'POST',headers:{'Content-Type':'application/x-www-form-urlencoded; charset=UTF-8'},body:body.toString()}).then(function(r){return r.json();}).then(function(data){if(result){result.className='share-result '+(data.success?'success':'danger');result.textContent=data.message||'';}if(data.success){['shareSenderName','shareSenderEmail','shareReceiverEmail','shareMessage'].forEach(function(id){var el=document.getElementById(id);if(el){el.value='';}});}}).catch(function(){if(result){result.className='share-result danger';result.textContent='Không thể gửi yêu cầu. Vui lòng thử lại.';}}).finally(function(){if(button){button.disabled=false;button.textContent='Gửi';}});}");
            builder.Append("</script>");
            return builder.ToString();
        }

        private void AddComment()
        {
            var news = NewsService.GetDetail(Request.QueryString["slug"]);
            if (news == null)
            {
                _commentResult = OperationResult.Fail("Bài viết không tồn tại.");
                return;
            }

            var content = Request.Form["content"];
            if (string.IsNullOrWhiteSpace(content))
            {
                _commentResult = OperationResult.Fail("Vui lòng nhập nội dung bình luận.");
                return;
            }

            content = content.Trim();
            if (content.Length > 2000)
            {
                _commentResult = OperationResult.Fail("Bình luận không được vượt quá 2000 ký tự.");
                return;
            }

            int? userId = AuthGuard.IsAuthenticated ? AuthGuard.CurrentUserId : (int?)null;
            var guestName = (Request.Form["guestName"] ?? string.Empty).Trim();
            var guestEmail = (Request.Form["guestEmail"] ?? string.Empty).Trim();
            if (!AuthGuard.IsAuthenticated)
            {
                if (string.IsNullOrWhiteSpace(guestName))
                {
                    _commentResult = OperationResult.Fail("Vui lòng nhập họ tên.");
                    return;
                }

                if (!IsValidEmail(guestEmail))
                {
                    _commentResult = OperationResult.Fail("Vui lòng nhập email hợp lệ.");
                    return;
                }
            }

            _commentResult = CommentService.Add(news.NewsID, userId, guestName, guestEmail, content);
            if (_commentResult.Success)
            {
                Response.Redirect(UiHelper.NewsUrl(news.Slug) + "&comment=sent#comments", true);
            }
        }

        private string RenderComments(NewsModel news, System.Collections.Generic.List<CommentModel> comments)
        {
            var builder = new StringBuilder("<section id=\"comments\" style=\"margin-top:30px\"><div class=\"section-title\"><h2>Bình luận</h2></div>");
            builder.Append(UiHelper.Alert(_commentResult));
            if (comments.Count == 0)
            {
                builder.Append("<p class=\"muted\">Chưa có bình luận được duyệt.</p>");
            }
            else
            {
                foreach (var comment in comments)
                {
                    builder.Append("<div class=\"comment-item\"><strong>" + UiHelper.E(comment.DisplayName) + "</strong><span class=\"muted\"> · " + UiHelper.E(UiHelper.Date(comment.CreatedAt)) + "</span><p>" + UiHelper.E(comment.Content) + "</p></div>");
                }
            }

            if (news.AllowComment)
            {
                builder.Append("<div class=\"form-panel\" style=\"margin-top:18px\"><h3>Gửi bình luận</h3><p class=\"muted\">Bình luận sẽ hiển thị sau khi quản trị viên duyệt.</p><input type=\"hidden\" name=\"commentAction\" value=\"send\" />");
                if (!AuthGuard.IsAuthenticated)
                {
                    builder.Append("<div class=\"form-grid\"><div class=\"field\"><label>Họ tên</label><input name=\"guestName\" maxlength=\"100\" required /></div><div class=\"field\"><label>Email</label><input name=\"guestEmail\" type=\"email\" maxlength=\"150\" required /></div></div>");
                }
                builder.Append("<div class=\"field full\" style=\"margin-top:14px\"><label>Nội dung</label><textarea name=\"content\" maxlength=\"2000\" required></textarea></div><div class=\"btn-row\" style=\"margin-top:14px\"><button class=\"btn-main\" type=\"submit\">Gửi chờ duyệt</button></div></div>");
            }
            else
            {
                builder.Append("<p class=\"muted\">Bài viết này hiện tắt chức năng bình luận.</p>");
            }

            builder.Append("</section>");
            return builder.ToString();
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(email.Trim());
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}
