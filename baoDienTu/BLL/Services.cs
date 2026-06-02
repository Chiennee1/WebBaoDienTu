using System;
using System.Collections.Generic;
using baoDienTu.DAL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class NewsService
    {
        public static List<NewsModel> GetFeatured(int top)
        {
            return NewsRepository.GetFeatured(top);
        }

        public static List<NewsModel> GetLatest(int top)
        {
            return NewsRepository.GetLatest(top);
        }

        public static List<NewsModel> GetMostViewed(int top)
        {
            return NewsRepository.GetMostViewed(top);
        }

        public static List<NewsModel> GetNewsList(int? catId, int page, int pageSize, out int total)
        {
            return NewsRepository.GetNewsList(catId, page, pageSize, out total);
        }

        public static List<NewsModel> Search(string keyword, int page, int pageSize, out int total)
        {
            return NewsRepository.Search(keyword, page, pageSize, out total);
        }

        public static NewsModel GetDetail(string slug)
        {
            return NewsRepository.GetDetail(slug);
        }

        public static NewsModel GetById(int newsId)
        {
            return NewsRepository.GetById(newsId);
        }

        public static void IncreaseViewCount(int newsId)
        {
            NewsRepository.IncreaseViewCount(newsId);
        }

        public static List<NewsModel> GetRelated(int newsId, int top)
        {
            return NewsRepository.GetRelated(newsId, top);
        }

        public static List<NewsModel> GetAdminNews(byte? status, int? authorId, int? catId, string keyword, int page, int pageSize, out int total)
        {
            return NewsRepository.GetAdminNews(status, authorId, catId, keyword, page, pageSize, out total);
        }

        public static int AddNews(string title, string summary, string content, string thumbnail, int authorId, int catId, bool allowComment)
        {
            var slug = CreateUniqueSlug(title, 0);
            return NewsRepository.AddNews(title, slug, summary, content, thumbnail, authorId, catId, allowComment);
        }

        public static void UpdateNews(int newsId, string title, string summary, string content, string thumbnail, int catId, bool allowComment, bool isFeatured, bool isHot)
        {
            var slug = CreateUniqueSlug(title, newsId);
            NewsRepository.UpdateNews(newsId, title, slug, summary, content, thumbnail, catId, allowComment, isFeatured, isHot);
        }

        public static void DeleteNews(int newsId)
        {
            NewsRepository.DeleteNews(newsId);
        }

        public static void ApproveNews(int newsId, int adminId, bool approved, string reason)
        {
            NewsRepository.ApproveNews(newsId, adminId, approved, reason);
        }

        private static string CreateUniqueSlug(string title, int excludeNewsId)
        {
            var baseSlug = SlugHelper.Generate(title);
            var slug = baseSlug;
            var index = 2;
            while (NewsRepository.SlugExists(slug, excludeNewsId))
            {
                slug = baseSlug + "-" + index++;
            }
            return slug;
        }
    }

    public static class CategoryService
    {
        public static List<CategoryModel> GetCategories(bool? active)
        {
            return CategoryRepository.GetCategories(active);
        }

        public static CategoryModel GetBySlug(string slug)
        {
            return CategoryRepository.GetBySlug(slug);
        }

        public static CategoryModel GetById(int id)
        {
            return CategoryRepository.GetById(id);
        }

        public static OperationResult Save(int? id, string name, string slug, int? parentId, string description, int sortOrder, bool isActive)
        {
            slug = string.IsNullOrWhiteSpace(slug) ? SlugHelper.Generate(name) : SlugHelper.Generate(slug);
            if (id.HasValue)
            {
                CategoryRepository.UpdateCategory(id.Value, name, slug, parentId, description, sortOrder, isActive);
                return OperationResult.Ok("Đã cập nhật chuyên mục.");
            }

            var newId = CategoryRepository.AddCategory(name, slug, parentId, description, sortOrder);
            return newId == -1
                ? OperationResult.Fail("Slug chuyên mục đã tồn tại.")
                : OperationResult.Ok("Đã thêm chuyên mục.");
        }

        public static void SetActive(int id, bool active)
        {
            CategoryRepository.SetActive(id, active);
        }
    }

    public static class UserService
    {
        public static OperationResult Login(string username, string password, out UserModel user)
        {
            user = null;
            var saltTable = UserRepository.GetLoginSalt(username);
            if (saltTable.Rows.Count == 0)
            {
                return OperationResult.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            var hash = PasswordHasher.Hash(saltTable.Rows[0].GetString("Salt"), password);
            var table = UserRepository.VerifyLogin(username, hash);

            if (table.Rows.Count == 0)
            {
                return OperationResult.Fail("Không thể xác thực tài khoản.");
            }

            if (table.Columns.Contains("Result"))
            {
                var code = table.Rows[0].GetInt("Result");
                if (code == -1) return OperationResult.Fail("Tài khoản đang bị khóa 15 phút do đăng nhập sai nhiều lần.");
                if (code == -3) return OperationResult.Fail("Tài khoản đã bị vô hiệu hóa.");
                return OperationResult.Fail("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            user = UserRepository.MapUser(table.Rows[0]);
            return OperationResult.Ok("Đăng nhập thành công.");
        }

        public static OperationResult Register(string username, string password, string email, string fullName)
        {
            var salt = PasswordHasher.CreateSalt();
            var hash = PasswordHasher.Hash(salt, password);
            var result = UserRepository.Register(username, hash, salt, email, fullName);
            if (result == -1) return OperationResult.Fail("Tên đăng nhập đã tồn tại.");
            if (result == -2) return OperationResult.Fail("Email đã được sử dụng.");
            return OperationResult.Ok("Đăng ký thành công. Bạn có thể đăng nhập ngay.");
        }

        public static List<UserModel> GetUsers(int page, int pageSize, string keyword, out int total)
        {
            return UserRepository.GetUsers(page, pageSize, keyword, out total);
        }

        public static void SetActive(int userId, bool active)
        {
            UserRepository.SetActive(userId, active);
        }
    }

    public static class CommentService
    {
        public static List<CommentModel> GetApprovedByNews(int newsId)
        {
            return CommentRepository.GetApprovedByNews(newsId);
        }

        public static OperationResult Add(int newsId, int? userId, string guestName, string guestEmail, string content)
        {
            var result = CommentRepository.Add(newsId, userId, guestName, guestEmail, content);
            return result == -1
                ? OperationResult.Fail("Bài viết hiện không cho phép bình luận.")
                : OperationResult.Ok("Bình luận đã được gửi và đang chờ duyệt.");
        }

        public static List<CommentModel> GetAdminComments(bool? approved, int page, int pageSize, out int total)
        {
            return CommentRepository.GetAdminComments(approved, page, pageSize, out total);
        }

        public static void Approve(int commentId, bool approved)
        {
            CommentRepository.Approve(commentId, approved);
        }

        public static void Delete(int commentId)
        {
            CommentRepository.Delete(commentId);
        }
    }

    public static class NewsletterService
    {
        public static OperationResult Subscribe(string email, string fullName)
        {
            var code = NewsletterRepository.Subscribe(email, fullName, Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));
            return code == 1
                ? OperationResult.Ok("Đăng ký thành công. MVP đã lưu thông tin, email xác nhận sẽ được bật ở bước sau.")
                : OperationResult.Ok("Email này đã có trong danh sách đăng ký.");
        }
    }

    public static class DashboardService
    {
        public static DashboardStats GetStats()
        {
            return DashboardRepository.GetStats();
        }
    }
}
