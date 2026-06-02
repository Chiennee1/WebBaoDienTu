using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class NewsRepository
    {
        public static List<NewsModel> GetFeatured(int top)
        {
            return MapNewsList(DbHelper.ExecuteDataTable("SELECT TOP (@Top) * FROM vw_FeaturedNews", CommandType.Text, DbHelper.Param("@Top", top)));
        }

        public static List<NewsModel> GetLatest(int top)
        {
            return MapNewsList(DbHelper.ExecuteDataTable("SELECT TOP (@Top) * FROM vw_LatestNews", CommandType.Text, DbHelper.Param("@Top", top)));
        }

        public static List<NewsModel> GetMostViewed(int top)
        {
            return MapNewsList(DbHelper.ExecuteDataTable("SELECT TOP (@Top) * FROM vw_MostViewedNews", CommandType.Text, DbHelper.Param("@Top", top)));
        }

        public static List<NewsModel> GetNewsList(int? catId, int page, int pageSize, out int total)
        {
            var totalParam = DbHelper.OutputParam("@Total", SqlDbType.Int);
            var table = DbHelper.ExecuteDataTable("sp_GetNewsList", CommandType.StoredProcedure,
                DbHelper.Param("@CatID", catId),
                DbHelper.Param("@Page", page),
                DbHelper.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static List<NewsModel> Search(string keyword, int page, int pageSize, out int total)
        {
            var totalParam = DbHelper.OutputParam("@Total", SqlDbType.Int);
            var table = DbHelper.ExecuteDataTable("sp_SearchNews", CommandType.StoredProcedure,
                DbHelper.Param("@Keyword", keyword ?? string.Empty),
                DbHelper.Param("@Page", page),
                DbHelper.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static NewsModel GetDetail(string slug)
        {
            var table = DbHelper.ExecuteDataTable("sp_GetNewsDetail", CommandType.StoredProcedure, DbHelper.Param("@Slug", slug));
            return table.Rows.Count == 0 ? null : MapNews(table.Rows[0]);
        }

        public static NewsModel GetById(int newsId)
        {
            var table = DbHelper.ExecuteDataTable("sp_GetNewsById", CommandType.StoredProcedure, DbHelper.Param("@NewsID", newsId));
            return table.Rows.Count == 0 ? null : MapNews(table.Rows[0]);
        }

        public static void IncreaseViewCount(int newsId)
        {
            DbHelper.ExecuteNonQuery("sp_IncreaseViewCount", CommandType.StoredProcedure, DbHelper.Param("@NewsID", newsId));
        }

        public static List<NewsModel> GetRelated(int newsId, int top)
        {
            var table = DbHelper.ExecuteDataTable(
                "SELECT TOP (@Top) nd.NewsID, nd.Title, nd.Slug, nd.Summary, nd.Thumbnail, nd.CatID, nd.CatName, nd.CatSlug, nd.AuthorName, nd.PublishedAt, nd.ViewCount FROM vw_NewsDetail nd WHERE nd.CatID = (SELECT CatID FROM News WHERE NewsID = @NewsID) AND nd.NewsID <> @NewsID ORDER BY nd.PublishedAt DESC",
                CommandType.Text,
                DbHelper.Param("@Top", top),
                DbHelper.Param("@NewsID", newsId));
            return MapNewsList(table);
        }

        public static List<NewsModel> GetAdminNews(byte? status, int? authorId, int? catId, string keyword, int page, int pageSize, out int total)
        {
            var totalParam = DbHelper.OutputParam("@Total", SqlDbType.Int);
            var table = DbHelper.ExecuteDataTable("sp_GetAdminNewsList", CommandType.StoredProcedure,
                DbHelper.Param("@Status", status),
                DbHelper.Param("@AuthorID", authorId),
                DbHelper.Param("@CatID", catId),
                DbHelper.Param("@Keyword", string.IsNullOrWhiteSpace(keyword) ? null : keyword),
                DbHelper.Param("@Page", page),
                DbHelper.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static int AddNews(string title, string slug, string summary, string content, string thumbnail, int authorId, int catId, bool allowComment)
        {
            var newId = DbHelper.OutputParam("@NewNewsID", SqlDbType.Int);
            DbHelper.ExecuteNonQuery("sp_AddNews", CommandType.StoredProcedure,
                DbHelper.Param("@Title", title),
                DbHelper.Param("@Slug", slug),
                DbHelper.Param("@Summary", summary),
                DbHelper.Param("@Content", content),
                DbHelper.Param("@Thumbnail", string.IsNullOrWhiteSpace(thumbnail) ? null : thumbnail),
                DbHelper.Param("@AuthorID", authorId),
                DbHelper.Param("@CatID", catId),
                DbHelper.Param("@AllowComment", allowComment),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static void UpdateNews(int newsId, string title, string slug, string summary, string content, string thumbnail, int catId, bool allowComment, bool isFeatured, bool isHot)
        {
            DbHelper.ExecuteNonQuery("sp_UpdateNews", CommandType.StoredProcedure,
                DbHelper.Param("@NewsID", newsId),
                DbHelper.Param("@Title", title),
                DbHelper.Param("@Slug", slug),
                DbHelper.Param("@Summary", summary),
                DbHelper.Param("@Content", content),
                DbHelper.Param("@Thumbnail", string.IsNullOrWhiteSpace(thumbnail) ? null : thumbnail),
                DbHelper.Param("@CatID", catId),
                DbHelper.Param("@AllowComment", allowComment),
                DbHelper.Param("@IsFeatured", isFeatured),
                DbHelper.Param("@IsHot", isHot));
        }

        public static void DeleteNews(int newsId)
        {
            DbHelper.ExecuteNonQuery("sp_DeleteNews", CommandType.StoredProcedure, DbHelper.Param("@NewsID", newsId));
        }

        public static void ApproveNews(int newsId, int adminId, bool approved, string reason)
        {
            DbHelper.ExecuteNonQuery("sp_ApproveNews", CommandType.StoredProcedure,
                DbHelper.Param("@NewsID", newsId),
                DbHelper.Param("@AdminID", adminId),
                DbHelper.Param("@IsApproved", approved),
                DbHelper.Param("@RejectReason", reason));
        }

        public static bool SlugExists(string slug, int excludeNewsId)
        {
            return Convert.ToInt32(DbHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM News WHERE Slug = @Slug AND (@NewsID = 0 OR NewsID <> @NewsID)",
                CommandType.Text,
                DbHelper.Param("@Slug", slug),
                DbHelper.Param("@NewsID", excludeNewsId))) > 0;
        }

        private static List<NewsModel> MapNewsList(DataTable table)
        {
            return table.Rows.Cast<DataRow>().Select(MapNews).ToList();
        }

        private static NewsModel MapNews(DataRow row)
        {
            return new NewsModel
            {
                NewsID = row.GetInt("NewsID"),
                Title = row.GetString("Title"),
                Slug = row.GetString("Slug"),
                Summary = row.GetString("Summary"),
                Content = row.GetString("Content"),
                Thumbnail = row.GetString("Thumbnail"),
                AuthorID = row.GetInt("AuthorID"),
                AuthorName = row.GetString("AuthorName"),
                AuthorEmail = row.GetString("AuthorEmail"),
                CatID = row.GetInt("CatID"),
                CatName = row.GetString("CatName"),
                CatSlug = row.GetString("CatSlug"),
                Status = row.GetByte("Status"),
                IsApproved = row.GetBool("IsApproved"),
                AllowComment = row.GetBool("AllowComment"),
                IsFeatured = row.GetBool("IsFeatured"),
                IsHot = row.GetBool("IsHot"),
                ViewCount = row.GetInt("ViewCount"),
                RejectReason = row.GetString("RejectReason"),
                PublishedAt = row.GetNullableDateTime("PublishedAt"),
                CreatedAt = row.GetDateTime("CreatedAt"),
                UpdatedAt = row.GetDateTime("UpdatedAt")
            };
        }
    }

    public static class CategoryRepository
    {
        public static List<CategoryModel> GetCategories(bool? active)
        {
            var table = DbHelper.ExecuteDataTable("sp_GetCategories", CommandType.StoredProcedure, DbHelper.Param("@IsActive", active));
            return table.Rows.Cast<DataRow>().Select(MapCategory).ToList();
        }

        public static CategoryModel GetBySlug(string slug)
        {
            var table = DbHelper.ExecuteDataTable("SELECT TOP 1 * FROM Categories WHERE Slug = @Slug", CommandType.Text, DbHelper.Param("@Slug", slug));
            return table.Rows.Count == 0 ? null : MapCategory(table.Rows[0]);
        }

        public static CategoryModel GetById(int id)
        {
            var table = DbHelper.ExecuteDataTable("SELECT TOP 1 * FROM Categories WHERE CatID = @CatID", CommandType.Text, DbHelper.Param("@CatID", id));
            return table.Rows.Count == 0 ? null : MapCategory(table.Rows[0]);
        }

        public static int AddCategory(string name, string slug, int? parentId, string description, int sortOrder)
        {
            var newId = DbHelper.OutputParam("@NewCatID", SqlDbType.Int);
            DbHelper.ExecuteNonQuery("sp_AddCategory", CommandType.StoredProcedure,
                DbHelper.Param("@CatName", name),
                DbHelper.Param("@Slug", slug),
                DbHelper.Param("@ParentID", parentId),
                DbHelper.Param("@Description", description),
                DbHelper.Param("@SortOrder", sortOrder),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static void UpdateCategory(int id, string name, string slug, int? parentId, string description, int sortOrder, bool isActive)
        {
            DbHelper.ExecuteNonQuery("sp_UpdateCategory", CommandType.StoredProcedure,
                DbHelper.Param("@CatID", id),
                DbHelper.Param("@CatName", name),
                DbHelper.Param("@Slug", slug),
                DbHelper.Param("@ParentID", parentId),
                DbHelper.Param("@Description", description),
                DbHelper.Param("@SortOrder", sortOrder),
                DbHelper.Param("@IsActive", isActive));
        }

        public static void SetActive(int id, bool active)
        {
            DbHelper.ExecuteNonQuery("UPDATE Categories SET IsActive = @Active WHERE CatID = @CatID", CommandType.Text,
                DbHelper.Param("@Active", active),
                DbHelper.Param("@CatID", id));
        }

        private static CategoryModel MapCategory(DataRow row)
        {
            return new CategoryModel
            {
                CatID = row.GetInt("CatID"),
                CatName = row.GetString("CatName"),
                Slug = row.GetString("Slug"),
                ParentID = row.GetNullableInt("ParentID"),
                ParentName = row.GetString("ParentName"),
                Description = row.GetString("Description"),
                SortOrder = row.GetInt("SortOrder"),
                IsActive = row.GetBool("IsActive"),
                NewsCount = row.GetInt("NewsCount"),
                Breadcrumb = row.GetString("Breadcrumb"),
                CreatedAt = row.GetDateTime("CreatedAt")
            };
        }
    }

    public static class UserRepository
    {
        public static DataTable GetLoginSalt(string username)
        {
            return DbHelper.ExecuteDataTable("sp_Login", CommandType.StoredProcedure,
                DbHelper.Param("@Username", username),
                DbHelper.Param("@Password", string.Empty));
        }

        public static DataTable VerifyLogin(string username, string hash)
        {
            return DbHelper.ExecuteDataTable("sp_VerifyLogin", CommandType.StoredProcedure,
                DbHelper.Param("@Username", username),
                DbHelper.Param("@HashedPw", hash));
        }

        public static int Register(string username, string hash, string salt, string email, string fullName)
        {
            var newId = DbHelper.OutputParam("@NewUserID", SqlDbType.Int);
            DbHelper.ExecuteNonQuery("sp_RegisterUser", CommandType.StoredProcedure,
                DbHelper.Param("@Username", username),
                DbHelper.Param("@Password", hash),
                DbHelper.Param("@Salt", salt),
                DbHelper.Param("@Email", email),
                DbHelper.Param("@FullName", fullName),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static List<UserModel> GetUsers(int page, int pageSize, string keyword, out int total)
        {
            var totalParam = DbHelper.OutputParam("@Total", SqlDbType.Int);
            var table = DbHelper.ExecuteDataTable("sp_GetAllUsers", CommandType.StoredProcedure,
                DbHelper.Param("@RoleID", null),
                DbHelper.Param("@IsActive", null),
                DbHelper.Param("@Keyword", string.IsNullOrWhiteSpace(keyword) ? null : keyword),
                DbHelper.Param("@Page", page),
                DbHelper.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return table.Rows.Cast<DataRow>().Select(MapUser).ToList();
        }

        public static void SetActive(int userId, bool active)
        {
            DbHelper.ExecuteNonQuery("UPDATE Users SET IsActive = @Active WHERE UserID = @UserID", CommandType.Text,
                DbHelper.Param("@Active", active),
                DbHelper.Param("@UserID", userId));
        }

        public static UserModel MapUser(DataRow row)
        {
            return new UserModel
            {
                UserID = row.GetInt("UserID"),
                Username = row.GetString("Username"),
                Email = row.GetString("Email"),
                FullName = row.GetString("FullName"),
                RoleID = row.GetInt("RoleID"),
                RoleName = row.GetString("RoleName"),
                IsActive = row.HasColumn("IsActive") ? row.GetBool("IsActive") : true,
                Avatar = row.GetString("Avatar"),
                Phone = row.GetString("Phone"),
                CreatedAt = row.GetDateTime("CreatedAt"),
                LastLogin = row.GetNullableDateTime("LastLogin")
            };
        }
    }

    public static class CommentRepository
    {
        public static List<CommentModel> GetApprovedByNews(int newsId)
        {
            var table = DbHelper.ExecuteDataTable("sp_GetCommentsByNews", CommandType.StoredProcedure, DbHelper.Param("@NewsID", newsId));
            return table.Rows.Cast<DataRow>().Select(MapComment).ToList();
        }

        public static int Add(int newsId, int? userId, string guestName, string guestEmail, string content)
        {
            var newId = DbHelper.OutputParam("@NewCmtID", SqlDbType.Int);
            DbHelper.ExecuteNonQuery("sp_AddComment", CommandType.StoredProcedure,
                DbHelper.Param("@NewsID", newsId),
                DbHelper.Param("@UserID", userId),
                DbHelper.Param("@GuestName", guestName),
                DbHelper.Param("@GuestEmail", guestEmail),
                DbHelper.Param("@Content", content),
                DbHelper.Param("@ParentID", null),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static List<CommentModel> GetAdminComments(bool? approved, int page, int pageSize, out int total)
        {
            var where = approved.HasValue ? "WHERE IsApproved = @Approved" : string.Empty;
            total = Convert.ToInt32(DbHelper.ExecuteScalar(
                "SELECT COUNT(1) FROM vw_CommentDetails " + where,
                CommandType.Text,
                DbHelper.Param("@Approved", approved)));
            var table = DbHelper.ExecuteDataTable(
                "SELECT * FROM vw_CommentDetails " + where + " ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                CommandType.Text,
                DbHelper.Param("@Approved", approved),
                DbHelper.Param("@Offset", (page - 1) * pageSize),
                DbHelper.Param("@PageSize", pageSize));
            return table.Rows.Cast<DataRow>().Select(MapComment).ToList();
        }

        public static void Approve(int commentId, bool approved)
        {
            DbHelper.ExecuteNonQuery("sp_ApproveComment", CommandType.StoredProcedure,
                DbHelper.Param("@CmtID", commentId),
                DbHelper.Param("@IsApproved", approved));
        }

        public static void Delete(int commentId)
        {
            DbHelper.ExecuteNonQuery("DELETE FROM Comments WHERE CmtID = @CmtID", CommandType.Text, DbHelper.Param("@CmtID", commentId));
        }

        private static CommentModel MapComment(DataRow row)
        {
            return new CommentModel
            {
                CmtID = row.GetInt("CmtID"),
                NewsID = row.GetInt("NewsID"),
                NewsTitle = row.GetString("NewsTitle"),
                NewsSlug = row.GetString("NewsSlug"),
                UserID = row.GetNullableInt("UserID"),
                DisplayName = row.GetString("DisplayName"),
                DisplayEmail = row.GetString("DisplayEmail"),
                Content = row.GetString("Content"),
                ParentID = row.GetNullableInt("ParentID"),
                IsApproved = row.GetBool("IsApproved"),
                CreatedAt = row.GetDateTime("CreatedAt")
            };
        }
    }

    public static class NewsletterRepository
    {
        public static int Subscribe(string email, string fullName, string token, string unsubToken)
        {
            var result = DbHelper.OutputParam("@Result", SqlDbType.Int);
            DbHelper.ExecuteNonQuery("sp_SubscribeNewsletter", CommandType.StoredProcedure,
                DbHelper.Param("@Email", email),
                DbHelper.Param("@FullName", fullName),
                DbHelper.Param("@Token", token),
                DbHelper.Param("@UnsubToken", unsubToken),
                result);
            return result.Value == DBNull.Value ? 0 : Convert.ToInt32(result.Value);
        }
    }

    public static class DashboardRepository
    {
        public static DashboardStats GetStats()
        {
            var table = DbHelper.ExecuteDataTable("SELECT TOP 1 * FROM vw_AdminDashboard", CommandType.Text);
            if (table.Rows.Count == 0)
            {
                return new DashboardStats();
            }

            var row = table.Rows[0];
            return new DashboardStats
            {
                TotalApprovedNews = row.GetInt("TotalApprovedNews"),
                TotalPendingNews = row.GetInt("TotalPendingNews"),
                TotalActiveUsers = row.GetInt("TotalActiveUsers"),
                TotalSubscribers = row.GetInt("TotalSubscribers"),
                TotalPendingComments = row.GetInt("TotalPendingComments"),
                TotalViews = row.GetInt("TotalViews")
            };
        }
    }
}
