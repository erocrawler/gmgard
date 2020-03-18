using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public interface IRecommendationProvider
    {
        bool IsValid();

        Task<IEnumerable<Blog>> GetRecommendationAsync(Blog blog, IEnumerable<string> tags, int count);
    }
}
