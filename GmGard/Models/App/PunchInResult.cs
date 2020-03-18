using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class PunchInResult
    {
        public int ConsecutiveDays { get; set; }
        public int ExpBonus { get; set; }
        public int Points { get; set; }

        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
