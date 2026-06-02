using System;

namespace baoDienTu.Helpers
{
    public static class PagingHelper
    {
        public static int NormalizePage(string value)
        {
            int page;
            return int.TryParse(value, out page) && page > 0 ? page : 1;
        }

        public static int PageCount(int total, int pageSize)
        {
            return Math.Max(1, (int)Math.Ceiling(total / (double)Math.Max(1, pageSize)));
        }
    }
}
