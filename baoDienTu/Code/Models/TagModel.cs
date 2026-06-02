using System;

namespace baoDienTu.Models
{
    public class TagModel
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
        public string Slug { get; set; }
        public int UseCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
