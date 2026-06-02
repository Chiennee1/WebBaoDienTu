using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class NewsletterDAL
    {
        public static int Subscribe(string email, string fullName, string token, string unsubToken)
        {
            var result = DBConnection.OutputParam("@Result", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_SubscribeNewsletter", CommandType.StoredProcedure,
                DBConnection.Param("@Email", email),
                DBConnection.Param("@FullName", fullName),
                DBConnection.Param("@Token", token),
                DBConnection.Param("@UnsubToken", unsubToken),
                result);
            return result.Value == DBNull.Value ? 0 : Convert.ToInt32(result.Value);
        }

        public static int Confirm(string token)
        {
            var result = DBConnection.OutputParam("@Result", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_ConfirmNewsletter", CommandType.StoredProcedure,
                DBConnection.Param("@Token", token),
                result);
            return result.Value == DBNull.Value ? -1 : Convert.ToInt32(result.Value);
        }

        public static int Unsubscribe(string token)
        {
            var result = DBConnection.OutputParam("@Result", SqlDbType.Int);
            DBConnection.ExecuteNonQuery("sp_UnsubscribeNewsletter", CommandType.StoredProcedure,
                DBConnection.Param("@Token", token),
                result);
            return result.Value == DBNull.Value ? -1 : Convert.ToInt32(result.Value);
        }

        public static List<NewsletterModel> GetActiveSubscribers()
        {
            var table = DBConnection.ExecuteDataTable("sp_GetActiveSubscribers", CommandType.StoredProcedure);
            return table.Rows.Cast<DataRow>().Select(MapNewsletter).ToList();
        }

        public static List<NewsletterModel> GetAll()
        {
            var table = DBConnection.ExecuteDataTable("SELECT * FROM Newsletter ORDER BY SubscribedAt DESC", CommandType.Text);
            return table.Rows.Cast<DataRow>().Select(MapNewsletter).ToList();
        }

        public static void AddSendHistory(string subject, string htmlContent, int sentBy, int totalSent)
        {
            DBConnection.ExecuteNonQuery(
                "INSERT INTO Newsletter_Sends (Subject, HtmlContent, SentBy, TotalSent) VALUES (@Subject, @HtmlContent, @SentBy, @TotalSent)",
                CommandType.Text,
                DBConnection.Param("@Subject", subject),
                DBConnection.Param("@HtmlContent", htmlContent),
                DBConnection.Param("@SentBy", sentBy),
                DBConnection.Param("@TotalSent", totalSent));
        }

        private static NewsletterModel MapNewsletter(DataRow row)
        {
            return new NewsletterModel
            {
                SubID = row.GetInt("SubID"),
                Email = row.GetString("Email"),
                FullName = row.GetString("FullName"),
                IsActive = row.HasColumn("IsActive") && row.GetBool("IsActive"),
                IsConfirmed = row.HasColumn("IsConfirmed") && row.GetBool("IsConfirmed"),
                ConfirmToken = row.GetString("ConfirmToken"),
                UnsubscribeToken = row.GetString("UnsubscribeToken"),
                SubscribedAt = row.GetDateTime("SubscribedAt"),
                ConfirmedAt = row.GetNullableDateTime("ConfirmedAt")
            };
        }
    }
}
