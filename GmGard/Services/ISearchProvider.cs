using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public interface ISearchProvider
    {
        Task<SearchBlogResult> SearchBlogAsync(SearchModel searchModel, int pageNumber, int pageSize);
    }
}
