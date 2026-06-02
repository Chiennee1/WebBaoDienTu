using System.Collections.Generic;
using baoDienTu.DAL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class UserBLL
    {
        public static OperationResult Login(string username, string password, out UserModel user)
        {
            user = null;
            var saltTable = UserDAL.GetLoginSalt(username);
            if (saltTable.Rows.Count == 0)
            {
                return OperationResult.Fail("Tài khoản không tồn tại hoặc đã bị khóa.");
            }

            var hash = SecurityHelper.HashPassword(password, saltTable.Rows[0].GetString("Salt"));
            var table = UserDAL.VerifyLogin(username, hash);
            if (table.Rows.Count == 0)
            {
                return OperationResult.Fail("Không thể xác thực tài khoản.");
            }

            if (table.Columns.Contains("Result"))
            {
                var code = table.Rows[0].GetInt("Result");
                if (code == -1) return OperationResult.Fail("Tài khoản đang bị khóa 15 phút do đăng nhập sai nhiều lần.");
                if (code == -3) return OperationResult.Fail("Tài khoản đã bị vô hiệu hóa.");
                return OperationResult.Fail("Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            user = UserDAL.MapUser(table.Rows[0]);
            return OperationResult.Ok("Đăng nhập thành công.");
        }

        public static OperationResult Register(string username, string password, string email, string fullName)
        {
            var salt = SecurityHelper.GenerateSalt();
            var hash = SecurityHelper.HashPassword(password, salt);
            var result = UserDAL.Register(username, hash, salt, email, fullName);
            if (result == -1) return OperationResult.Fail("Tên đăng nhập đã tồn tại.");
            if (result == -2) return OperationResult.Fail("Email đã được sử dụng.");
            return OperationResult.Ok("Đăng ký thành công. Bạn có thể đăng nhập ngay.");
        }

        public static UserModel GetById(int userId)
        {
            return UserDAL.GetById(userId);
        }

        public static List<UserModel> GetUsers(int page, int pageSize, string keyword, out int total)
        {
            return UserDAL.GetUsers(page, pageSize, keyword, out total);
        }

        public static OperationResult UpdateProfile(int userId, string fullName, string phone, string avatar)
        {
            UserDAL.UpdateProfile(userId, fullName, phone, avatar);
            return OperationResult.Ok("Đã cập nhật hồ sơ.");
        }

        public static OperationResult ChangePassword(int userId, string oldPassword, string newPassword)
        {
            var oldSalt = UserDAL.GetSaltById(userId);
            var oldHash = SecurityHelper.HashPassword(oldPassword, oldSalt);
            var newSalt = SecurityHelper.GenerateSalt();
            var newHash = SecurityHelper.HashPassword(newPassword, newSalt);
            return UserDAL.ChangePassword(userId, oldHash, newHash, newSalt) == 1
                ? OperationResult.Ok("Đã đổi mật khẩu.")
                : OperationResult.Fail("Mật khẩu hiện tại không đúng.");
        }

        public static void SetActive(int userId, bool active)
        {
            UserDAL.SetActive(userId, active);
        }
    }
}
