using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class Paged<T>
    {
        public Paged(PagedList.IPagedList<T> pagedList)
        {
            Items = pagedList;
            PageCount = pagedList.PageCount;
            TotalItemCount = pagedList.TotalItemCount;
            PageNumber = pagedList.PageNumber;
            PageSize = pagedList.PageSize;
            Skip = pagedList.PageSize * (PageNumber - 1);
        }

        public IEnumerable<T> Items { get; set; }

        public int PageCount { get; set; }

        public int TotalItemCount { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int Skip { get; set; }
    }
}
