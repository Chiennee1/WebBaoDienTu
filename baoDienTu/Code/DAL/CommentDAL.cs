using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class CommentDAL
    {
        public static List<CommentModel> GetApprovedByNews(int newsId)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetCommentsByNews", CommandType.StoredProcedure, DBConnection.Param("@NewsID", newsId));
            return table.Rows.Cast<DataRow>().Select(MapComment).ToList();
        }

        public static int Add(int newsId, int? userId, string guestName, string guestEmail, string content)
        {
            var newId = DBConnection.OutputParam("@NewCmtID", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_AddComment", CommandType.StoredProcedure,
                DBConnection.Param("@NewsID", newsId),
                DBConnection.Param("@UserID", userId),
                DBConnection.Param("@GuestName", guestName),
                DBConnection.Param("@GuestEmail", guestEmail),
                DBConnection.Param("@Content", content),
                DBConnection.Param("@ParentID", null),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static List<CommentModel> GetAdminComments(bool? approved, int page, int pageSize, out int total)
        {
            var where = approved.HasValue ? "WHERE IsApproved = @Approved" : string.Empty;
            total = Convert.ToInt32(DBConnection.ExecuteScalar(
                "SELECT COUNT(1) FROM vw_CommentDetails " + where,
                CommandType.Text,
                DBConnection.Param("@Approved", approved)));
            var table = DBConnection.ExecuteDataTable(
                "SELECT * FROM vw_CommentDetails " + where + " ORDER BY CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY",
                CommandType.Text,
                DBConnection.Param("@Approved", approved),
                DBConnection.Param("@Offset", (page - 1) * pageSize),
                DBConnection.Param("@PageSize", pageSize));
            return table.Rows.Cast<DataRow>().Select(MapComment).ToList();
        }

        public static void Approve(int commentId, bool approved)
        {
            DBConnection.ExecuteNonQuery("sp_ApproveComment", CommandType.StoredProcedure,
                DBConnection.Param("@CmtID", commentId),
                DBConnection.Param("@IsApproved", approved));
        }

        public static void Delete(int commentId)
        {
            DBConnection.ExecuteNonQuery("DELETE FROM Comments WHERE CmtID = @CmtID", CommandType.Text, DBConnection.Param("@CmtID", commentId));
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
}
