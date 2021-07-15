using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using ClickersAPI.DTO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ClickersAPI.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> _Queryable, PaginationDto _Pagination)
        {
            return _Queryable.Skip((_Pagination.Page - 1) * _Pagination.RecordsPerPage)
                .Take(_Pagination.RecordsPerPage);
        }
        
        public static Task<TSource[]> ToArrayAsyncSafe<TSource>(this IQueryable<TSource> _Source)
        {
            if (_Source == null)
                throw new ArgumentNullException(nameof(_Source));
            return _Source is IDbAsyncEnumerable<TSource> ? 
                _Source.ToArrayAsync() : Task.FromResult(_Source.ToArray());
        }

        public static Task<List<TSource>> ToListAsyncSafe<TSource>(this IQueryable<TSource> _Source)
        {
            if (_Source == null)
                throw new ArgumentNullException(nameof(_Source));
            return _Source is IDbAsyncEnumerable<TSource> ? 
                _Source.ToListAsync() : Task.FromResult(_Source.ToList());
        }

        public static Task<int> CountAsyncSafe<TSource>(this IQueryable<TSource> _Source)
        {
            if (_Source == null)
                throw new ArgumentException(nameof(_Source));
            return _Source is IDbAsyncEnumerable<TSource> ? _Source.CountAsync() : Task.FromResult(_Source.Count());
        }
    }
}
