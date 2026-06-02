using System.Collections.Generic;
using baoDienTu.DAL;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class SettingBLL
    {
        public static string Get(string key)
        {
            return SettingDAL.Get(key);
        }

        public static Dictionary<string, string> GetAll()
        {
            return SettingDAL.GetAll();
        }

        public static OperationResult Save(Dictionary<string, string> values)
        {
            foreach (var item in values)
            {
                SettingDAL.Set(item.Key, item.Value);
            }
            return OperationResult.Ok("Đã lưu cấu hình hệ thống.");
        }
    }
}
