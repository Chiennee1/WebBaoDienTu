using System;

namespace baoDienTu.Models
{
    public class CommentModel
    {
        public int CmtID { get; set; }
        public int NewsID { get; set; }
        public string NewsTitle { get; set; }
        public string NewsSlug { get; set; }
        public int? UserID { get; set; }
        public string DisplayName { get; set; }
        public string DisplayEmail { get; set; }
        public string Content { get; set; }
        public int? ParentID { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
