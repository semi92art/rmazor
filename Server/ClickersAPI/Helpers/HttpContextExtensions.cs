using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ClickersAPI.Helpers
{
    public static class HttpContextExtensions
    {
        public static async Task InsertPaginationParametersResponse<T>(this HttpContext _HttpContext,
            IQueryable<T> _Queryable, int _RecordsPerPage)
        {
            if (_HttpContext == null)
                throw new ArgumentException(nameof(_HttpContext));

            double count = await _Queryable.CountAsyncSafe();
            double totalAmountPages = Math.Ceiling(count / _RecordsPerPage);
            _HttpContext.Response.Headers.Add("totalAmountPages", totalAmountPages.ToString(CultureInfo.InvariantCulture));
        }
    }
}
