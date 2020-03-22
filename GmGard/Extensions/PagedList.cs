using System.Data.Entity;
using System.Threading.Tasks;
using System;
using System.Linq;
using X.PagedList;

namespace GmGard.Extensions
{
    public class PagedList<T> : BasePagedList<T>
    {
        private PagedList() { }

        public static async Task<IPagedList<T>> CreateAsync(IQueryable<T> superset, int pageNumber, int pageSize)
        {
            var list = new PagedList<T>();
            list.TotalItemCount = superset == null ? 0 : await superset.CountAsync();
            await list.InitAsync(superset, pageNumber, pageSize);
            return list;
        }

        private async Task InitAsync(IQueryable<T> superset, int pageNumber, int pageSize)
        {
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");
            PageSize = pageSize;
            PageNumber = 1;
            PageCount = TotalItemCount > 0 ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;
            if (pageNumber > 0 && pageNumber <= PageCount)
            {
                PageNumber = pageNumber;
            }
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var num = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = num > TotalItemCount ? TotalItemCount : num;
            if (superset == null || TotalItemCount <= 0)
                return;
            var skipCount = pageNumber == 1 ? 0 : (pageNumber - 1) * pageSize;
            Subset.AddRange(await superset.Skip(skipCount).Take(pageSize).ToListAsync());
        }

        public static IPagedList<T> Create(IQueryable<T> superset, int pageNumber, int pageSize)
        {
            var list = new PagedList<T>();
            list.TotalItemCount = superset == null ? 0 : superset.Count();
            list.Init(superset, pageNumber, pageSize);
            return list;
        }

        public static IPagedList<T> Create(IQueryable<T> superset, int pageNumber, int pageSize, int itemCount)
        {
            var list = new PagedList<T>();
            list.TotalItemCount = superset == null ? 0 : itemCount;
            list.Init(superset, pageNumber, pageSize);
            return list;
        }

        private void Init(IQueryable<T> superset, int pageNumber, int pageSize)
        {
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "PageSize cannot be less than 1.");
            PageSize = pageSize;
            PageNumber = 1;
            PageCount = TotalItemCount > 0 ? (int)Math.Ceiling(TotalItemCount / (double)PageSize) : 0;
            if (pageNumber > 0 && pageNumber <= PageCount)
            {
                PageNumber = pageNumber;
            }
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var num = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = num > TotalItemCount ? TotalItemCount : num;
            if (superset == null || TotalItemCount <= 0)
                return;
            var skipCount = pageNumber == 1 ? 0 : (pageNumber - 1) * pageSize;
            Subset.AddRange(superset.Skip(skipCount).Take(pageSize).ToList());
        }
    }

    public static class PagedListExtensions
    {
        /// <summary>
        /// Creates a subset of this collection of objects that can be individually accessed by index and containing metadata about the collection of objects the subset was created from.
        /// </summary>
        /// <typeparam name="T">The type of object the collection should contain.</typeparam>
        /// <param name="superset">The collection of objects to be divided into subsets. If the collection implements <see cref="IQueryable{T}"/>, it will be treated as such.</param>
        /// <param name="pageNumber">The one-based index of the subset of objects to be contained by this instance.</param>
        /// <param name="pageSize">The maximum size of any individual subset.</param>
        /// <returns>A subset of this collection of objects that can be individually accessed by index and containing metadata about the collection of objects the subset was created from.</returns>
        /// <seealso cref="PagedList{T}"/>
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> superset, int pageNumber, int pageSize)
        {
            return await PagedList<T>.CreateAsync(superset, pageNumber, pageSize);
        }

        public static IPagedList<T> ToPagedList<T>(this IQueryable<T> superset, int pageNumber, int pageSize)
        {
            return PagedList<T>.Create(superset, pageNumber, pageSize);
        }

        public static IPagedList<T> ToPagedList<T>(this IQueryable<T> superset, int pageNumber, int pageSize, int itemCount)
        {
            return PagedList<T>.Create(superset, pageNumber, pageSize, itemCount);
        }
    }
}
