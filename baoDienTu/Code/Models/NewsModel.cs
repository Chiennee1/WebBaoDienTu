using System;

namespace baoDienTu.Models
{
    public class NewsModel
    {
        public int NewsID { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Summary { get; set; }
        public string Content { get; set; }
        public string Thumbnail { get; set; }
        public int AuthorID { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public int CatID { get; set; }
        public string CatName { get; set; }
        public string CatSlug { get; set; }
        public byte Status { get; set; }
        public bool IsApproved { get; set; }
        public bool AllowComment { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsHot { get; set; }
        public int ViewCount { get; set; }
        public string RejectReason { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
