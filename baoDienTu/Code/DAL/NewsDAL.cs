using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class NewsDAL
    {
        public static List<NewsModel> GetFeatured(int top)
        {
            return MapNewsList(DBConnection.ExecuteDataTable("SELECT TOP (@Top) * FROM vw_FeaturedNews", CommandType.Text, DBConnection.Param("@Top", top)));
        }

        public static List<NewsModel> GetLatest(int top)
        {
            return MapNewsList(DBConnection.ExecuteDataTable("SELECT TOP (@Top) * FROM vw_LatestNews", CommandType.Text, DBConnection.Param("@Top", top)));
        }

        public static List<NewsModel> GetMostViewed(int top)
        {
            return MapNewsList(DBConnection.ExecuteDataTable("SELECT TOP (@Top) * FROM vw_MostViewedNews", CommandType.Text, DBConnection.Param("@Top", top)));
        }

        public static List<NewsModel> GetNewsList(int? catId, int page, int pageSize, out int total)
        {
            var totalParam = DBConnection.OutputParam("@Total", SqlDbType.Int);
            var table = DBConnection.ExecuteDataTable("sp_GetNewsList", CommandType.StoredProcedure,
                DBConnection.Param("@CatID", catId),
                DBConnection.Param("@Page", page),
                DBConnection.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static List<NewsModel> Search(string keyword, int page, int pageSize, out int total)
        {
            var totalParam = DBConnection.OutputParam("@Total", SqlDbType.Int);
            var table = DBConnection.ExecuteDataTable("sp_SearchNews", CommandType.StoredProcedure,
                DBConnection.Param("@Keyword", keyword ?? string.Empty),
                DBConnection.Param("@Page", page),
                DBConnection.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static List<NewsModel> GetByTag(string tagSlug, int page, int pageSize, out int total)
        {
            var totalParam = DBConnection.OutputParam("@Total", SqlDbType.Int);
            var table = DBConnection.ExecuteDataTable("sp_GetNewsByTag", CommandType.StoredProcedure,
                DBConnection.Param("@TagSlug", tagSlug ?? string.Empty),
                DBConnection.Param("@Page", page),
                DBConnection.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static NewsModel GetDetail(string slug)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetNewsDetail", CommandType.StoredProcedure, DBConnection.Param("@Slug", slug));
            return table.Rows.Count == 0 ? null : MapNews(table.Rows[0]);
        }

        public static NewsModel GetById(int newsId)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetNewsById", CommandType.StoredProcedure, DBConnection.Param("@NewsID", newsId));
            return table.Rows.Count == 0 ? null : MapNews(table.Rows[0]);
        }

        public static void IncreaseViewCount(int newsId)
        {
            DBConnection.ExecuteNonQuery("sp_IncreaseViewCount", CommandType.StoredProcedure, DBConnection.Param("@NewsID", newsId));
        }

        public static List<NewsModel> GetRelated(int newsId, int top)
        {
            var table = DBConnection.ExecuteDataTable(
                "SELECT TOP (@Top) nd.NewsID, nd.Title, nd.Slug, nd.Summary, nd.Thumbnail, nd.CatID, nd.CatName, nd.CatSlug, nd.AuthorName, nd.PublishedAt, nd.ViewCount FROM vw_NewsDetail nd WHERE nd.CatID = (SELECT CatID FROM News WHERE NewsID = @NewsID) AND nd.NewsID <> @NewsID ORDER BY nd.PublishedAt DESC",
                CommandType.Text,
                DBConnection.Param("@Top", top),
                DBConnection.Param("@NewsID", newsId));
            return MapNewsList(table);
        }

        public static List<NewsModel> GetAdminNews(byte? status, int? authorId, int? catId, string keyword, int page, int pageSize, out int total)
        {
            var totalParam = DBConnection.OutputParam("@Total", SqlDbType.Int);
            var table = DBConnection.ExecuteDataTable("sp_GetAdminNewsList", CommandType.StoredProcedure,
                DBConnection.Param("@Status", status),
                DBConnection.Param("@AuthorID", authorId),
                DBConnection.Param("@CatID", catId),
                DBConnection.Param("@Keyword", string.IsNullOrWhiteSpace(keyword) ? null : keyword),
                DBConnection.Param("@Page", page),
                DBConnection.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return MapNewsList(table);
        }

        public static int AddNews(string title, string slug, string summary, string content, string thumbnail, int authorId, int catId, bool allowComment)
        {
            var newId = DBConnection.OutputParam("@NewNewsID", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_AddNews", CommandType.StoredProcedure,
                DBConnection.Param("@Title", title),
                DBConnection.Param("@Slug", slug),
                DBConnection.Param("@Summary", summary),
                DBConnection.Param("@Content", content),
                DBConnection.Param("@Thumbnail", string.IsNullOrWhiteSpace(thumbnail) ? null : thumbnail),
                DBConnection.Param("@AuthorID", authorId),
                DBConnection.Param("@CatID", catId),
                DBConnection.Param("@AllowComment", allowComment),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static void UpdateNews(int newsId, string title, string slug, string summary, string content, string thumbnail, int catId, bool allowComment, bool isFeatured, bool isHot)
        {
            DBConnection.ExecuteNonQuery("sp_UpdateNews", CommandType.StoredProcedure,
                DBConnection.Param("@NewsID", newsId),
                DBConnection.Param("@Title", title),
                DBConnection.Param("@Slug", slug),
                DBConnection.Param("@Summary", summary),
                DBConnection.Param("@Content", content),
                DBConnection.Param("@Thumbnail", string.IsNullOrWhiteSpace(thumbnail) ? null : thumbnail),
                DBConnection.Param("@CatID", catId),
                DBConnection.Param("@AllowComment", allowComment),
                DBConnection.Param("@IsFeatured", isFeatured),
                DBConnection.Param("@IsHot", isHot));
        }

        public static void DeleteNews(int newsId)
        {
            DBConnection.ExecuteNonQuery("sp_DeleteNews", CommandType.StoredProcedure, DBConnection.Param("@NewsID", newsId));
        }

        public static void ApproveNews(int newsId, int adminId, bool approved, string reason)
        {
            DBConnection.ExecuteNonQuery("sp_ApproveNews", CommandType.StoredProcedure,
                DBConnection.Param("@NewsID", newsId),
                DBConnection.Param("@AdminID", adminId),
                DBConnection.Param("@IsApproved", approved),
                DBConnection.Param("@RejectReason", reason));
        }

        public static bool SlugExists(string slug, int excludeNewsId)
        {
            return Convert.ToInt32(DBConnection.ExecuteScalar(
                "SELECT COUNT(1) FROM News WHERE Slug = @Slug AND (@NewsID = 0 OR NewsID <> @NewsID)",
                CommandType.Text,
                DBConnection.Param("@Slug", slug),
                DBConnection.Param("@NewsID", excludeNewsId))) > 0;
        }

        internal static List<NewsModel> MapNewsList(DataTable table)
        {
            return table.Rows.Cast<DataRow>().Select(MapNews).ToList();
        }

        internal static NewsModel MapNews(DataRow row)
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
}
