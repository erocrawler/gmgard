using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class PunchInRequest
    {
        public DateTime? Date { get; set; }
        public bool UseTicket { get; set; }
    }
}
