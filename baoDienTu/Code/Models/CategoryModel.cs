using System;

namespace baoDienTu.Models
{
    public class CategoryModel
    {
        public int CatID { get; set; }
        public string CatName { get; set; }
        public string Slug { get; set; }
        public int? ParentID { get; set; }
        public string ParentName { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int NewsCount { get; set; }
        public string Breadcrumb { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
