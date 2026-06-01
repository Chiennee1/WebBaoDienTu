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

    public class UserModel
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public string Avatar { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }

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

    public class DashboardStats
    {
        public int TotalApprovedNews { get; set; }
        public int TotalPendingNews { get; set; }
        public int TotalActiveUsers { get; set; }
        public int TotalSubscribers { get; set; }
        public int TotalPendingComments { get; set; }
        public int TotalViews { get; set; }
    }

    public class OperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static OperationResult Ok(string message)
        {
            return new OperationResult { Success = true, Message = message };
        }

        public static OperationResult Fail(string message)
        {
            return new OperationResult { Success = false, Message = message };
        }
    }
}
