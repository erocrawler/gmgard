using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class SpinWheelStatus
    {
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public int UserPoints { get; set; }
        public int WheelACost { get; set; }
        public int WheelADailyLimit { get; set; }
        public int WheelBCost { get; set; }
        public int CeilingCost { get; set; }
        public bool ShowRedeem { get; set; }
        public IEnumerable<Vouchers> Vouchers { get; set; }

        public IEnumerable<WheelPrize> WheelAPrizes { get; set; }
        public IEnumerable<WheelPrize> WheelBPrizes { get; set; }
        public IEnumerable<WheelPrize> DisplayPrizes { get; set; }
        public IEnumerable<WheelPrize> CouponPrizes { get; set; }

    }
}
