using System.Data;
using baoDienTu.Models;

namespace baoDienTu.DAL
{
    public static class DashboardDAL
    {
        public static DashboardModel GetStats()
        {
            var table = DBConnection.ExecuteDataTable("SELECT TOP 1 * FROM vw_AdminDashboard", CommandType.Text);
            if (table.Rows.Count == 0)
            {
                return new DashboardModel();
            }

            var row = table.Rows[0];
            return new DashboardModel
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
