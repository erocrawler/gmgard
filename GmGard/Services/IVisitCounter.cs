using System.Collections.Generic;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public interface IVisitCounter
    {
        long GetBlogVisit(int id, bool increment = false);
        long GetTopicVisit(int id, bool increment = false);
        long GetBountyVisit(int id, bool increment = false);
        void PrepareBlogVisits(IEnumerable<int> ids);
        void PrepareBountyVisits(IEnumerable<int> ids);
        Task SaveVisits();
    }
}