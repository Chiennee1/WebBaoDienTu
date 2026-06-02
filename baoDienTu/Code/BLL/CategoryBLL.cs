using System.Collections.Generic;
using baoDienTu.DAL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class CategoryBLL
    {
        public static List<CategoryModel> GetCategories(bool? active)
        {
            return CategoryDAL.GetCategories(active);
        }

        public static CategoryModel GetBySlug(string slug)
        {
            return CategoryDAL.GetBySlug(slug);
        }

        public static CategoryModel GetById(int id)
        {
            return CategoryDAL.GetById(id);
        }

        public static OperationResult Save(int? id, string name, string slug, int? parentId, string description, int sortOrder, bool isActive)
        {
            slug = string.IsNullOrWhiteSpace(slug) ? SlugHelper.Generate(name) : SlugHelper.Generate(slug);
            if (id.HasValue)
            {
                CategoryDAL.UpdateCategory(id.Value, name, slug, parentId, description, sortOrder, isActive);
                return OperationResult.Ok("Đã cập nhật chuyên mục.");
            }

            var newId = CategoryDAL.AddCategory(name, slug, parentId, description, sortOrder);
            return newId == -1
                ? OperationResult.Fail("Slug chuyên mục đã tồn tại.")
                : OperationResult.Ok("Đã thêm chuyên mục.");
        }

        public static void SetActive(int id, bool active)
        {
            CategoryDAL.SetActive(id, active);
        }
    }
}
