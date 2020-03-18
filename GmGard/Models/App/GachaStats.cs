using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class GachaStats
    {
        public int TotalCards { get; set; }
        public IEnumerable<string> Progresses { get; set; }
        public IEnumerable<GachaItemDetails> UserCards { get; set; }
    }
}
