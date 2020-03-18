using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class RateResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public BlogRatingDisplay Rating { get; set; }
    }
}
