using System.Data;

namespace baoDienTu.DAL
{
    public static class ShareLogDAL
    {
        public static void Add(int newsId, string senderName, string senderEmail, string receiverEmail, string message, bool isSent)
        {
            DBConnection.ExecuteNonQuery("sp_AddShareLog", CommandType.StoredProcedure,
                DBConnection.Param("@NewsID", newsId),
                DBConnection.Param("@SenderName", senderName),
                DBConnection.Param("@SenderEmail", senderEmail),
                DBConnection.Param("@ReceiverEmail", receiverEmail),
                DBConnection.Param("@Message", message),
                DBConnection.Param("@IsSent", isSent));
        }
    }
}
