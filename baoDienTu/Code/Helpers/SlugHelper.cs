using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace baoDienTu.Helpers
{
    public static class SlugHelper
    {
        public static string Generate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "bai-viet";
            }

            var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder();
            foreach (var ch in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(ch == 'đ' ? 'd' : ch);
                }
            }

            var slug = Regex.Replace(builder.ToString().Normalize(NormalizationForm.FormC), @"[^a-z0-9\s-]", string.Empty);
            slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
            return string.IsNullOrEmpty(slug) ? "bai-viet" : slug;
        }
    }
}
