using System;

namespace baoDienTu.Models
{
    public class NewsletterModel
    {
        public int SubID { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public bool IsConfirmed { get; set; }
        public string ConfirmToken { get; set; }
        public string UnsubscribeToken { get; set; }
        public DateTime SubscribedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
    }
}
