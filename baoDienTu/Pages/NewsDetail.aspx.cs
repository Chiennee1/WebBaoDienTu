using System;
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
            if (Request.HttpMethod == "POST" && Request.Form["commentAction"] == "send")
            {
                AddComment();
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

                if (!IsPostBack)
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
                builder.Append("<div class=\"btn-row\" style=\"margin-top:22px\"><a class=\"btn-soft\" href=\"" + UiHelper.Attr(UiHelper.PrintUrl(news)) + "\" target=\"_blank\">In bài viết</a><button class=\"btn-soft\" type=\"button\" onclick=\"document.getElementById('shareBox').classList.toggle('is-open')\">Gửi cho bạn</button></div>");
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
            builder.Append("<div id=\"shareBox\" class=\"form-panel share-box\"><h3>Gửi tin cho bạn</h3>");
            builder.Append("<input type=\"hidden\" id=\"shareNewsId\" value=\"" + news.NewsID + "\" />");
            builder.Append("<div class=\"form-grid\"><div class=\"field\"><label>Tên của bạn</label><input id=\"shareSenderName\" /></div>");
            builder.Append("<div class=\"field\"><label>Email của bạn</label><input id=\"shareSenderEmail\" type=\"email\" /></div>");
            builder.Append("<div class=\"field\"><label>Email người nhận</label><input id=\"shareReceiverEmail\" type=\"email\" required /></div>");
            builder.Append("<div class=\"field\"><label>Lời nhắn</label><input id=\"shareMessage\" /></div></div>");
            builder.Append("<div class=\"btn-row\" style=\"margin-top:14px\"><button class=\"btn-main\" type=\"button\" onclick=\"shareNews()\">Gửi</button><span id=\"shareResult\" class=\"muted\"></span></div></div>");
            builder.Append("<script>function shareNews(){var body=new URLSearchParams();['shareNewsId','shareSenderName','shareSenderEmail','shareReceiverEmail','shareMessage'].forEach(function(id){var el=document.getElementById(id);body.append(id.replace('share','').replace('NewsId','newsId').replace('SenderName','senderName').replace('SenderEmail','senderEmail').replace('ReceiverEmail','receiverEmail').replace('Message','message'),el?el.value:'');});fetch('" + ResolveUrl("~/Handlers/ShareNews.ashx") + "',{method:'POST',body:body}).then(function(r){return r.json();}).then(function(data){document.getElementById('shareResult').textContent=data.message||'';});}</script>");
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

            int? userId = AuthGuard.IsAuthenticated ? AuthGuard.CurrentUserId : (int?)null;
            _commentResult = CommentService.Add(news.NewsID, userId, Request.Form["guestName"], Request.Form["guestEmail"], content);
        }

        private string RenderComments(NewsModel news, System.Collections.Generic.List<CommentModel> comments)
        {
            var builder = new StringBuilder("<section style=\"margin-top:30px\"><div class=\"section-title\"><h2>Bình luận</h2></div>");
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
                builder.Append("<div class=\"form-panel\" style=\"margin-top:18px\"><h3>Gửi bình luận</h3><input type=\"hidden\" name=\"commentAction\" value=\"send\" />");
                if (!AuthGuard.IsAuthenticated)
                {
                    builder.Append("<div class=\"form-grid\"><div class=\"field\"><label>Họ tên</label><input name=\"guestName\" required /></div><div class=\"field\"><label>Email</label><input name=\"guestEmail\" type=\"email\" required /></div></div>");
                }
                builder.Append("<div class=\"field full\" style=\"margin-top:14px\"><label>Nội dung</label><textarea name=\"content\" required></textarea></div><div class=\"btn-row\" style=\"margin-top:14px\"><button class=\"btn-main\" type=\"submit\">Gửi chờ duyệt</button></div></div>");
            }

            builder.Append("</section>");
            return builder.ToString();
        }
    }
}
