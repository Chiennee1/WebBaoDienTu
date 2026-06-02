using System;
using System.Text;
using System.Web.UI;
using baoDienTu.BLL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.Admin
{
    public partial class ManageCategory : Page
    {
        private OperationResult _result;
        private CategoryModel _edit;

        protected void Page_Load(object sender, EventArgs e)
        {
            AuthGuard.RequireRole(this, "Admin");
            if (Request.HttpMethod == "POST")
            {
                HandlePost();
            }
            int editId;
            if (int.TryParse(Request.QueryString["id"], out editId))
            {
                _edit = CategoryService.GetById(editId);
            }
        }

        protected string RenderPage()
        {
            var builder = new StringBuilder(UiHelper.Alert(_result));
            var categories = CategoryService.GetCategories(null);
            builder.Append("<div class=\"form-panel\" style=\"margin-bottom:18px\"><h2>" + (_edit == null ? "Thêm chuyên mục" : "Sửa chuyên mục") + "</h2><input type=\"hidden\" name=\"saveCategory\" value=\"1\" />");
            if (_edit != null)
            {
                builder.Append("<input type=\"hidden\" name=\"catId\" value=\"" + _edit.CatID + "\" />");
            }
            builder.Append("<div class=\"form-grid\"><div class=\"field\"><label>Tên</label><input name=\"catName\" value=\"" + UiHelper.Attr(_edit == null ? string.Empty : _edit.CatName) + "\" required /></div>");
            builder.Append("<div class=\"field\"><label>Slug</label><input name=\"slug\" value=\"" + UiHelper.Attr(_edit == null ? string.Empty : _edit.Slug) + "\" /></div>");
            builder.Append("<div class=\"field\"><label>Chuyên mục cha</label><select name=\"parentId\"><option value=\"\">Không có</option>");
            foreach (var cat in categories)
            {
                if (_edit != null && cat.CatID == _edit.CatID) continue;
                builder.Append("<option value=\"" + cat.CatID + "\"" + (_edit != null && _edit.ParentID == cat.CatID ? " selected" : string.Empty) + ">" + UiHelper.E(cat.CatName) + "</option>");
            }
            builder.Append("</select></div><div class=\"field\"><label>Thứ tự</label><input name=\"sortOrder\" type=\"number\" value=\"" + (_edit == null ? 0 : _edit.SortOrder) + "\" /></div>");
            builder.Append("<div class=\"field full\"><label>Mô tả</label><textarea name=\"description\">" + UiHelper.E(_edit == null ? string.Empty : _edit.Description) + "</textarea></div>");
            builder.Append("<div class=\"field\"><label>Trạng thái</label><select name=\"isActive\"><option value=\"1\"" + (_edit == null || _edit.IsActive ? " selected" : string.Empty) + ">Hoạt động</option><option value=\"0\"" + (_edit != null && !_edit.IsActive ? " selected" : string.Empty) + ">Ẩn</option></select></div></div>");
            builder.Append("<div class=\"btn-row\" style=\"margin-top:16px\"><button class=\"btn-main\" type=\"submit\">Lưu chuyên mục</button><a class=\"btn-soft\" href=\"ManageCategory.aspx\">Làm mới</a></div></div>");

            builder.Append("<div class=\"table-wrap\"><table class=\"data-table\"><thead><tr><th>Tên</th><th>Slug</th><th>Cha</th><th>Số bài</th><th>Trạng thái</th><th>Thao tác</th></tr></thead><tbody>");
            foreach (var cat in categories)
            {
                builder.Append("<tr><td><strong>" + UiHelper.E(cat.CatName) + "</strong></td><td>" + UiHelper.E(cat.Slug) + "</td><td>" + UiHelper.E(cat.ParentName) + "</td><td>" + cat.NewsCount + "</td><td>" + (cat.IsActive ? "Hoạt động" : "Ẩn") + "</td><td><a class=\"btn-soft\" href=\"ManageCategory.aspx?id=" + cat.CatID + "\">Sửa</a> <button class=\"btn-danger\" name=\"toggleCategory\" value=\"" + cat.CatID + "\" type=\"submit\">" + (cat.IsActive ? "Ẩn" : "Hiện") + "</button></td></tr>");
            }
            builder.Append("</tbody></table></div>");
            return AdminUiHelper.Layout("Quản lý chuyên mục", builder.ToString());
        }

        private void HandlePost()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Request.Form["toggleCategory"]))
                {
                    var category = CategoryService.GetById(Convert.ToInt32(Request.Form["toggleCategory"]));
                    CategoryService.SetActive(category.CatID, !category.IsActive);
                    _result = OperationResult.Ok("Đã cập nhật trạng thái chuyên mục.");
                    return;
                }

                if (Request.Form["saveCategory"] == "1")
                {
                    int? id = string.IsNullOrWhiteSpace(Request.Form["catId"]) ? (int?)null : Convert.ToInt32(Request.Form["catId"]);
                    int? parentId = string.IsNullOrWhiteSpace(Request.Form["parentId"]) ? (int?)null : Convert.ToInt32(Request.Form["parentId"]);
                    var sortOrder = string.IsNullOrWhiteSpace(Request.Form["sortOrder"]) ? 0 : Convert.ToInt32(Request.Form["sortOrder"]);
                    _result = CategoryService.Save(id, Request.Form["catName"], Request.Form["slug"], parentId, Request.Form["description"], sortOrder, Request.Form["isActive"] == "1");
                }
            }
            catch (Exception ex)
            {
                _result = OperationResult.Fail(ex.Message);
            }
        }
    }
}
