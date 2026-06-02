using System;
using System.Text;
using System.Web;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu
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
                var builder = new StringBuilder("<div class=\"page-shell\"><div class=\"container-xl content-layout\">");
                builder.Append("<article class=\"article-detail\">");
                builder.Append("<div class=\"article-meta\"><span>" + UiHelper.E(news.CatName) + "</span><span>" + UiHelper.E(UiHelper.Date(news.PublishedAt)) + "</span><span>" + news.ViewCount + " lượt xem</span></div>");
                builder.Append("<h1>" + UiHelper.E(news.Title) + "</h1>");
                builder.Append("<p class=\"article-summary\">" + UiHelper.E(news.Summary) + "</p>");
                builder.Append(UiHelper.ImageBlock(news, string.Empty));
                builder.Append("<div class=\"article-body\">" + news.Content + "</div>");
                builder.Append("<div class=\"btn-row\" style=\"margin-top:22px\"><button class=\"btn-soft\" type=\"button\" onclick=\"window.print()\">In bài viết</button><a class=\"btn-soft\" href=\"mailto:?subject=" + UiHelper.Attr(news.Title) + "&body=" + UiHelper.Attr(Request.Url.ToString()) + "\">Gửi cho bạn</a></div>");
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
