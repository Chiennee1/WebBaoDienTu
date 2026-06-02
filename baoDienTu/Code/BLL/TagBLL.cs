using System.Collections.Generic;
using System.Linq;
using baoDienTu.DAL;
using baoDienTu.Models;

namespace baoDienTu.BLL
{
    public static class TagBLL
    {
        public static void AddTagsToNews(int newsId, string tagNames)
        {
            TagDAL.AddTagsToNews(newsId, tagNames);
        }

        public static List<TagModel> GetTagsByNews(int newsId)
        {
            return TagDAL.GetTagsByNews(newsId);
        }

        public static string GetTagNameCsv(int newsId)
        {
            return string.Join(", ", GetTagsByNews(newsId).Select(t => t.TagName));
        }

        public static List<TagModel> GetPopularTags(int top)
        {
            return TagDAL.GetPopularTags(top);
        }

        public static TagModel GetBySlug(string slug)
        {
            return TagDAL.GetBySlug(slug);
        }
    }
}
