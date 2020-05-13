using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class WheelConfig
    {
        public string Title { get; set; }
        public List<WheelPrize> WheelAPrizes { get; set; }
        public List<WheelPrize> WheelBPrizes { get; set; }
        public List<WheelPrize> RedeemPrizes { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public int WheelACost { get; set; }
        public int WheelADailyLimit { get; set; }
        public int WheelBLPCost { get; set; }
        public int CeilingCost { get; set; }
        public bool ShowRedeem { get; set; }
        public List<WheelPrize> DisplayPrizes { get; set; }
        public List<WheelPrize> CouponPrizes { get; set; }
    }

    public class WheelPrize
    {
        public string PrizeName { get; set; }
        public string PrizePic { get; set; }
        public bool IsRealItem { get; set; }
        public bool IsVoucher { get; set; }
        public int PrizeLPValue { get; set; }
        public string ItemLink { get; set; }
        [JsonIgnore]
        public int DrawPercentage { get; set; }

        public string RedeemItemName => IsVoucher ? string.Format("{0}/{0}", PrizeLPValue) : PrizeName;
    }
}
