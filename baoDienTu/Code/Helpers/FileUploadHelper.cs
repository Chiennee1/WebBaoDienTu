using System;
using System.IO;
using System.Linq;
using System.Web;

namespace baoDienTu.Helpers
{
    public static class FileUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public static bool IsValidImage(HttpPostedFile file, int maxFileSizeMb, out string error)
        {
            error = string.Empty;
            if (file == null || file.ContentLength == 0)
            {
                error = "Vui lòng chọn ảnh cần upload.";
                return false;
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(ext))
            {
                error = "Chỉ cho phép ảnh JPG, PNG, GIF hoặc WEBP.";
                return false;
            }

            if (file.ContentLength > maxFileSizeMb * 1024 * 1024)
            {
                error = "Ảnh không được vượt quá " + maxFileSizeMb + "MB.";
                return false;
            }

            var header = new byte[4];
            file.InputStream.Read(header, 0, header.Length);
            file.InputStream.Seek(0, SeekOrigin.Begin);

            var isJpg = header[0] == 0xFF && header[1] == 0xD8;
            var isPng = header[0] == 0x89 && header[1] == 0x50;
            var isGif = header[0] == 0x47 && header[1] == 0x49;
            var isWebp = header[0] == 0x52 && header[1] == 0x49;
            if (!isJpg && !isPng && !isGif && !isWebp)
            {
                error = "File không phải định dạng ảnh hợp lệ.";
                return false;
            }

            return true;
        }

        public static string SaveImage(HttpPostedFile file, string serverFolder, string publicFolder)
        {
            Directory.CreateDirectory(serverFolder);
            var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
            file.SaveAs(Path.Combine(serverFolder, fileName));
            return VirtualPathUtility.ToAbsolute(publicFolder.TrimEnd('/') + "/" + fileName);
        }
    }
}
