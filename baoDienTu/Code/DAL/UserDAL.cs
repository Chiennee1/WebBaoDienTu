using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class UserDAL
    {
        public static DataTable GetLoginSalt(string username)
        {
            return DBConnection.ExecuteDataTable("sp_Login", CommandType.StoredProcedure,
                DBConnection.Param("@Username", username),
                DBConnection.Param("@Password", string.Empty));
        }

        public static DataTable VerifyLogin(string username, string hash)
        {
            return DBConnection.ExecuteDataTable("sp_VerifyLogin", CommandType.StoredProcedure,
                DBConnection.Param("@Username", username),
                DBConnection.Param("@HashedPw", hash));
        }

        public static int Register(string username, string hash, string salt, string email, string fullName)
        {
            var newId = DBConnection.OutputParam("@NewUserID", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_RegisterUser", CommandType.StoredProcedure,
                DBConnection.Param("@Username", username),
                DBConnection.Param("@Password", hash),
                DBConnection.Param("@Salt", salt),
                DBConnection.Param("@Email", email),
                DBConnection.Param("@FullName", fullName),
                newId);
            return newId.Value == DBNull.Value ? 0 : Convert.ToInt32(newId.Value);
        }

        public static UserModel GetById(int userId)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetUserById", CommandType.StoredProcedure, DBConnection.Param("@UserID", userId));
            return table.Rows.Count == 0 ? null : MapUser(table.Rows[0]);
        }

        public static List<UserModel> GetUsers(int page, int pageSize, string keyword, out int total)
        {
            var totalParam = DBConnection.OutputParam("@Total", SqlDbType.Int);
            var table = DBConnection.ExecuteDataTable("sp_GetAllUsers", CommandType.StoredProcedure,
                DBConnection.Param("@RoleID", null),
                DBConnection.Param("@IsActive", null),
                DBConnection.Param("@Keyword", string.IsNullOrWhiteSpace(keyword) ? null : keyword),
                DBConnection.Param("@Page", page),
                DBConnection.Param("@PageSize", pageSize),
                totalParam);
            total = totalParam.Value == DBNull.Value ? 0 : Convert.ToInt32(totalParam.Value);
            return table.Rows.Cast<DataRow>().Select(MapUser).ToList();
        }

        public static void UpdateProfile(int userId, string fullName, string phone, string avatar)
        {
            DBConnection.ExecuteNonQuery("sp_UpdateUserProfile", CommandType.StoredProcedure,
                DBConnection.Param("@UserID", userId),
                DBConnection.Param("@FullName", fullName),
                DBConnection.Param("@Phone", phone),
                DBConnection.Param("@Avatar", string.IsNullOrWhiteSpace(avatar) ? null : avatar));
        }

        public static int ChangePassword(int userId, string oldHash, string newHash, string newSalt)
        {
            var result = DBConnection.OutputParam("@Result", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_ChangePassword", CommandType.StoredProcedure,
                DBConnection.Param("@UserID", userId),
                DBConnection.Param("@OldHashedPw", oldHash),
                DBConnection.Param("@NewHashedPw", newHash),
                DBConnection.Param("@NewSalt", newSalt),
                result);
            return result.Value == DBNull.Value ? -1 : Convert.ToInt32(result.Value);
        }

        public static string GetSaltById(int userId)
        {
            return Convert.ToString(DBConnection.ExecuteScalar("SELECT Salt FROM Users WHERE UserID = @UserID", CommandType.Text, DBConnection.Param("@UserID", userId)));
        }

        public static void SetActive(int userId, bool active)
        {
            DBConnection.ExecuteNonQuery("UPDATE Users SET IsActive = @Active WHERE UserID = @UserID", CommandType.Text,
                DBConnection.Param("@Active", active),
                DBConnection.Param("@UserID", userId));
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
}
