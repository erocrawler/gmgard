using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class Vouchers
    {
        public string VoucherID { get; set; }
        public DateTime IssueTime { get; set; }
        public DateTime? UseTime { get; set; }
        public string RedeemItem { get; set; }
        public UserVoucher.Kind Kind { get; set; }
        public string UserName { get; set; }

        static public Vouchers FromUserVoucher(UserVoucher userVoucher, string userName)
        {
            return new Vouchers
            {
                VoucherID = userVoucher.VoucherID.ToString(),
                IssueTime = userVoucher.IssueTime,
                UseTime = userVoucher.UseTime,
                RedeemItem = userVoucher.RedeemItem,
                Kind = userVoucher.VoucherKind,
                UserName = userName,
            };
        }
    }
}
