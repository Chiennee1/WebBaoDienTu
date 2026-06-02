using System.Collections.Generic;
using System.Data;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class TagDAL
    {
        public static void AddTagsToNews(int newsId, string tagNames)
        {
            DBConnection.ExecuteNonQuery("sp_AddTagsToNews", CommandType.StoredProcedure,
                DBConnection.Param("@NewsID", newsId),
                DBConnection.Param("@TagNames", tagNames ?? string.Empty));
        }

        public static List<TagModel> GetTagsByNews(int newsId)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetTagsByNews", CommandType.StoredProcedure, DBConnection.Param("@NewsID", newsId));
            return table.Rows.Cast<DataRow>().Select(MapTag).ToList();
        }

        public static List<TagModel> GetPopularTags(int top)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetPopularTags", CommandType.StoredProcedure, DBConnection.Param("@Top", top));
            return table.Rows.Cast<DataRow>().Select(MapTag).ToList();
        }

        public static TagModel GetBySlug(string slug)
        {
            var table = DBConnection.ExecuteDataTable("SELECT TOP 1 * FROM Tags WHERE Slug = @Slug", CommandType.Text, DBConnection.Param("@Slug", slug));
            return table.Rows.Count == 0 ? null : MapTag(table.Rows[0]);
        }

        private static TagModel MapTag(DataRow row)
        {
            return new TagModel
            {
                TagID = row.GetInt("TagID"),
                TagName = row.GetString("TagName"),
                Slug = row.GetString("Slug"),
                UseCount = row.GetInt("UseCount"),
                CreatedAt = row.GetDateTime("CreatedAt")
            };
        }
    }
}
