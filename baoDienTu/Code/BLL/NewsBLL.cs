using System.Collections.Generic;
using baoDienTu.DAL;
using baoDienTu.Helpers;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class NewsBLL
    {
        public static List<NewsModel> GetFeatured(int top)
        {
            return NewsDAL.GetFeatured(top);
        }

        public static List<NewsModel> GetLatest(int top)
        {
            return NewsDAL.GetLatest(top);
        }

        public static List<NewsModel> GetMostViewed(int top)
        {
            return NewsDAL.GetMostViewed(top);
        }

        public static List<NewsModel> GetNewsList(int? catId, int page, int pageSize, out int total)
        {
            return NewsDAL.GetNewsList(catId, page, pageSize, out total);
        }

        public static List<NewsModel> Search(string keyword, int page, int pageSize, out int total)
        {
            return NewsDAL.Search(keyword, page, pageSize, out total);
        }

        public static List<NewsModel> GetByTag(string tagSlug, int page, int pageSize, out int total)
        {
            return NewsDAL.GetByTag(tagSlug, page, pageSize, out total);
        }

        public static NewsModel GetDetail(string slug)
        {
            return NewsDAL.GetDetail(slug);
        }

        public static NewsModel GetById(int newsId)
        {
            return NewsDAL.GetById(newsId);
        }

        public static void IncreaseViewCount(int newsId)
        {
            NewsDAL.IncreaseViewCount(newsId);
        }

        public static List<NewsModel> GetRelated(int newsId, int top)
        {
            return NewsDAL.GetRelated(newsId, top);
        }

        public static List<NewsModel> GetAdminNews(byte? status, int? authorId, int? catId, string keyword, int page, int pageSize, out int total)
        {
            return NewsDAL.GetAdminNews(status, authorId, catId, keyword, page, pageSize, out total);
        }

        public static int AddNews(string title, string summary, string content, string thumbnail, int authorId, int catId, bool allowComment)
        {
            return AddNews(title, summary, content, thumbnail, authorId, catId, allowComment, string.Empty);
        }

        public static int AddNews(string title, string summary, string content, string thumbnail, int authorId, int catId, bool allowComment, string tagNames)
        {
            var slug = CreateUniqueSlug(title, 0);
            var id = NewsDAL.AddNews(title, slug, summary, content, thumbnail, authorId, catId, allowComment);
            if (id > 0 && !string.IsNullOrWhiteSpace(tagNames))
            {
                TagDAL.AddTagsToNews(id, tagNames);
            }
            return id;
        }

        public static void UpdateNews(int newsId, string title, string summary, string content, string thumbnail, int catId, bool allowComment, bool isFeatured, bool isHot)
        {
            UpdateNews(newsId, title, summary, content, thumbnail, catId, allowComment, isFeatured, isHot, string.Empty);
        }

        public static void UpdateNews(int newsId, string title, string summary, string content, string thumbnail, int catId, bool allowComment, bool isFeatured, bool isHot, string tagNames)
        {
            var slug = CreateUniqueSlug(title, newsId);
            NewsDAL.UpdateNews(newsId, title, slug, summary, content, thumbnail, catId, allowComment, isFeatured, isHot);
            if (tagNames != null)
            {
                TagDAL.AddTagsToNews(newsId, tagNames);
            }
        }

        public static void DeleteNews(int newsId)
        {
            NewsDAL.DeleteNews(newsId);
        }

        public static void ApproveNews(int newsId, int adminId, bool approved, string reason)
        {
            NewsDAL.ApproveNews(newsId, adminId, approved, reason);
        }

        private static string CreateUniqueSlug(string title, int excludeNewsId)
        {
            var baseSlug = SlugHelper.Generate(title);
            var slug = baseSlug;
            var index = 2;
            while (NewsDAL.SlugExists(slug, excludeNewsId))
            {
                slug = baseSlug + "-" + index++;
            }
            return slug;
        }
    }
}
