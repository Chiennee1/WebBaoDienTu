using System.Collections.Generic;
using System.Data;

namespace baoDienTu.DAL
{
    public static class SettingDAL
    {
        public static string Get(string key)
        {
            var table = DBConnection.ExecuteDataTable("sp_GetSetting", CommandType.StoredProcedure, DBConnection.Param("@Key", key));
            return table.Rows.Count == 0 ? string.Empty : table.Rows[0].GetString("SettingValue");
        }

        public static Dictionary<string, string> GetAll()
        {
            var table = DBConnection.ExecuteDataTable("SELECT SettingKey, SettingValue FROM Settings ORDER BY SettingKey", CommandType.Text);
            var values = new Dictionary<string, string>();
            foreach (DataRow row in table.Rows)
            {
                values[row.GetString("SettingKey")] = row.GetString("SettingValue");
            }
            return values;
        }

        public static void Set(string key, string value)
        {
            DBConnection.ExecuteNonQuery("sp_SetSetting", CommandType.StoredProcedure,
                DBConnection.Param("@Key", key),
                DBConnection.Param("@Value", value));
        }
    }
}
