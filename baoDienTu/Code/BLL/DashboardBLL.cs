using baoDienTu.DAL;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class DashboardBLL
    {
        public static DashboardModel GetStats()
        {
            return DashboardDAL.GetStats();
        }
    }
}
