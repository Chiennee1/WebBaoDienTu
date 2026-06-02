using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class CategoryDAL
    {
        public static List<CategoryModel> GetCategories(bool? active)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetCategories", CommandType.StoredProcedure, DBConnection.Param("@IsActive", active));
            return table.Rows.Cast<DataRow>().Select(MapCategory).ToList();
        }

        public static CategoryModel GetBySlug(string slug)
        {
            var table = DBConnection.ExecuteDataTable("SELECT TOP 1 * FROM Categories WHERE Slug = @Slug", CommandType.Text, DBConnection.Param("@Slug", slug));
            return table.Rows.Count == 0 ? null : MapCategory(table.Rows[0]);
        }

        public static CategoryModel GetById(int id)
        {
            var table = DBConnection.ExecuteDataTable("SELECT TOP 1 * FROM Categories WHERE CatID = @CatID", CommandType.Text, DBConnection.Param("@CatID", id));
            return table.Rows.Count == 0 ? null : MapCategory(table.Rows[0]);
        }

        public static int AddCategory(string name, string slug, int? parentId, string description, int sortOrder)
        {
            var newId = DBConnection.OutputParam("@NewCatID", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_AddCategory", CommandType.StoredProcedure,
                DBConnection.Param("@CatName", name),
                DBConnection.Param("@Slug", slug),
                DBConnection.Param("@ParentID", parentId),
                DBConnection.Param("@Description", description),
                DBConnection.Param("@SortOrder", sortOrder),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static void UpdateCategory(int id, string name, string slug, int? parentId, string description, int sortOrder, bool isActive)
        {
            DBConnection.ExecuteNonQuery("sp_UpdateCategory", CommandType.StoredProcedure,
                DBConnection.Param("@CatID", id),
                DBConnection.Param("@CatName", name),
                DBConnection.Param("@Slug", slug),
                DBConnection.Param("@ParentID", parentId),
                DBConnection.Param("@Description", description),
                DBConnection.Param("@SortOrder", sortOrder),
                DBConnection.Param("@IsActive", isActive));
        }

        public static void SetActive(int id, bool active)
        {
            DBConnection.ExecuteNonQuery("UPDATE Categories SET IsActive = @Active WHERE CatID = @CatID", CommandType.Text,
                DBConnection.Param("@Active", active),
                DBConnection.Param("@CatID", id));
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
                IsActive = row.HasColumn("IsActive") ? row.GetBool("IsActive") : true,
                NewsCount = row.GetInt("NewsCount"),
                Breadcrumb = row.GetString("Breadcrumb"),
                CreatedAt = row.GetDateTime("CreatedAt")
            };
        }
    }
}
