using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class GachaRequest
    {
        public int Count { get; set; }
        public GachaPool.PoolName Pool { get; set; }
    }
}
