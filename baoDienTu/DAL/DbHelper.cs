using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace baoDienTu.DAL
{
    public static class DbHelper
    {
        public static string ConnectionString
        {
            get { return ConfigurationManager.ConnectionStrings["BaoDienTuConnection"].ConnectionString; }
        }

        public static DataTable ExecuteDataTable(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = CreateCommand(connection, commandText, commandType, parameters))
            using (var adapter = new SqlDataAdapter(command))
            {
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
        }

        public static int ExecuteNonQuery(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = CreateCommand(connection, commandText, commandType, parameters))
            {
                connection.Open();
                return command.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string commandText, CommandType commandType, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = CreateCommand(connection, commandText, commandType, parameters))
            {
                connection.Open();
                return command.ExecuteScalar();
            }
        }

        public static SqlParameter Param(string name, object value)
        {
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public static SqlParameter OutputParam(string name, SqlDbType type)
        {
            return new SqlParameter(name, type) { Direction = ParameterDirection.Output };
        }

        private static SqlCommand CreateCommand(SqlConnection connection, string commandText, CommandType commandType, SqlParameter[] parameters)
        {
            var command = new SqlCommand(commandText, connection) { CommandType = commandType };
            if (parameters != null)
            {
                command.Parameters.AddRange(parameters);
            }
            return command;
        }
    }

    public static class DataRowExtensions
    {
        public static bool HasColumn(this DataRow row, string columnName)
        {
            return row != null && row.Table.Columns.Contains(columnName);
        }

        public static string GetString(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value ? Convert.ToString(row[columnName]) : string.Empty;
        }

        public static int GetInt(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value ? Convert.ToInt32(row[columnName]) : 0;
        }

        public static int? GetNullableInt(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value ? Convert.ToInt32(row[columnName]) : (int?)null;
        }

        public static byte GetByte(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value ? Convert.ToByte(row[columnName]) : (byte)0;
        }

        public static bool GetBool(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value && Convert.ToBoolean(row[columnName]);
        }

        public static DateTime GetDateTime(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value ? Convert.ToDateTime(row[columnName]) : DateTime.MinValue;
        }

        public static DateTime? GetNullableDateTime(this DataRow row, string columnName)
        {
            return row.HasColumn(columnName) && row[columnName] != DBNull.Value ? Convert.ToDateTime(row[columnName]) : (DateTime?)null;
        }
    }
}
