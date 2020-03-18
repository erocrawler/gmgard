using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class GachaResult
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public int CurrentPoint { get; set; }
        public IEnumerable<string> Rewards { get; set; }

        public class GachaItem
        {
            public string Name { get; set; }
            public int Rarity { get; set; }
        }

        public IEnumerable<GachaItem> Items { get; set; }
    }
}
