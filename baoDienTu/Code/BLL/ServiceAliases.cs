using System.Collections.Generic;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class NewsService
    {
        public static List<NewsModel> GetFeatured(int top) { return NewsBLL.GetFeatured(top); }
        public static List<NewsModel> GetLatest(int top) { return NewsBLL.GetLatest(top); }
        public static List<NewsModel> GetMostViewed(int top) { return NewsBLL.GetMostViewed(top); }
        public static List<NewsModel> GetNewsList(int? catId, int page, int pageSize, out int total) { return NewsBLL.GetNewsList(catId, page, pageSize, out total); }
        public static List<NewsModel> Search(string keyword, int page, int pageSize, out int total) { return NewsBLL.Search(keyword, page, pageSize, out total); }
        public static NewsModel GetDetail(string slug) { return NewsBLL.GetDetail(slug); }
        public static NewsModel GetById(int newsId) { return NewsBLL.GetById(newsId); }
        public static void IncreaseViewCount(int newsId) { NewsBLL.IncreaseViewCount(newsId); }
        public static List<NewsModel> GetRelated(int newsId, int top) { return NewsBLL.GetRelated(newsId, top); }
        public static List<NewsModel> GetAdminNews(byte? status, int? authorId, int? catId, string keyword, int page, int pageSize, out int total) { return NewsBLL.GetAdminNews(status, authorId, catId, keyword, page, pageSize, out total); }
        public static int AddNews(string title, string summary, string content, string thumbnail, int authorId, int catId, bool allowComment) { return NewsBLL.AddNews(title, summary, content, thumbnail, authorId, catId, allowComment); }
        public static void UpdateNews(int newsId, string title, string summary, string content, string thumbnail, int catId, bool allowComment, bool isFeatured, bool isHot) { NewsBLL.UpdateNews(newsId, title, summary, content, thumbnail, catId, allowComment, isFeatured, isHot); }
        public static void DeleteNews(int newsId) { NewsBLL.DeleteNews(newsId); }
        public static void ApproveNews(int newsId, int adminId, bool approved, string reason) { NewsBLL.ApproveNews(newsId, adminId, approved, reason); }
    }

    public static class CategoryService
    {
        public static List<CategoryModel> GetCategories(bool? active) { return CategoryBLL.GetCategories(active); }
        public static CategoryModel GetBySlug(string slug) { return CategoryBLL.GetBySlug(slug); }
        public static CategoryModel GetById(int id) { return CategoryBLL.GetById(id); }
        public static OperationResult Save(int? id, string name, string slug, int? parentId, string description, int sortOrder, bool isActive) { return CategoryBLL.Save(id, name, slug, parentId, description, sortOrder, isActive); }
        public static void SetActive(int id, bool active) { CategoryBLL.SetActive(id, active); }
    }

    public static class UserService
    {
        public static OperationResult Login(string username, string password, out UserModel user) { return UserBLL.Login(username, password, out user); }
        public static OperationResult Register(string username, string password, string email, string fullName) { return UserBLL.Register(username, password, email, fullName); }
        public static List<UserModel> GetUsers(int page, int pageSize, string keyword, out int total) { return UserBLL.GetUsers(page, pageSize, keyword, out total); }
        public static void SetActive(int userId, bool active) { UserBLL.SetActive(userId, active); }
    }

    public static class CommentService
    {
        public static List<CommentModel> GetApprovedByNews(int newsId) { return CommentBLL.GetApprovedByNews(newsId); }
        public static OperationResult Add(int newsId, int? userId, string guestName, string guestEmail, string content) { return CommentBLL.Add(newsId, userId, guestName, guestEmail, content); }
        public static List<CommentModel> GetAdminComments(bool? approved, int page, int pageSize, out int total) { return CommentBLL.GetAdminComments(approved, page, pageSize, out total); }
        public static void Approve(int commentId, bool approved) { CommentBLL.Approve(commentId, approved); }
        public static void Delete(int commentId) { CommentBLL.Delete(commentId); }
    }

    public static class NewsletterService
    {
        public static OperationResult Subscribe(string email, string fullName) { return NewsletterBLL.Subscribe(email, fullName); }
    }

    public static class DashboardService
    {
        public static DashboardModel GetStats() { return DashboardBLL.GetStats(); }
    }
}
