using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class PunchInHistoryResponse
    {
        public class PunchIn
        {
            public DateTime TimeStamp { get; set; }
            public bool IsMakeUp { get; set; }
        }

        public IEnumerable<PunchIn> PunchIns { get; set; }
        public int? LegacySignDays { get; set; }
        public DateTime MinSignDate { get; set; }
    }
}
