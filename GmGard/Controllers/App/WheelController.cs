using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GmGard.Models;
using GmGard.Models.App;
using GmGard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Authorize]
    [EnableCors("GmAppOrigin")]
    [Route("api/Wheel/[action]")]
    [ApiController]
    public class WheelController : AppControllerBase
    {
        private readonly ExpUtil _expUtil;
        private readonly UsersContext _udb;
        private readonly UserManager<UserProfile> _userManager;
        private readonly IOptions<WheelConfig> _wheelConfig; 
        private static readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public WheelController(
            UsersContext udb,
            UserManager<UserProfile> userManager,
            ExpUtil expUtil,
            IOptions<WheelConfig> options)
        {
            _udb = udb;
            _expUtil = expUtil;
            _userManager = userManager;
            _wheelConfig = options;
        }

        bool IsActive => _wheelConfig.Value != null && _wheelConfig.Value.EventStart < DateTime.Now && DateTime.Now < _wheelConfig.Value.EventEnd;
        IEnumerable<WheelPrize> AllPrizes => _wheelConfig.Value.WheelAPrizes.Concat(_wheelConfig.Value.WheelBPrizes).Concat(_wheelConfig.Value.RedeemPrizes);

        // GET: api/wheel/get
        [HttpGet]
        public async Task<JsonResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            var vouchers = await _udb.UserVouchers.Where(v => v.UserID == user.Id).ToListAsync();
            return Json(new SpinWheelStatus
            {
                Title = _wheelConfig.Value.Title,
                IsActive = IsActive, 
                UserPoints = user.Points, 
                Vouchers = vouchers.Select(v => Vouchers.FromUserVoucher(v, user.UserName)),
                WheelAPrizes = _wheelConfig.Value.WheelAPrizes,
                WheelBPrizes = _wheelConfig.Value.WheelBPrizes,
                WheelACost = _wheelConfig.Value.WheelACost,
                WheelBCost = _wheelConfig.Value.WheelBLPCost,
                CeilingCost = _wheelConfig.Value.CeilingCost,
            });
        }

        // POST api/wheel/spin
        [HttpPost]
        public async Task<ActionResult> Spin(string wheelType)
        {
            if (!IsActive)
            {
                return BadRequest();
            }
            if (wheelType == "a")
            {
                return await SpinAAsync();
            }
            else if (wheelType == "b")
            {
                return await SpinBAsync();
            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<ActionResult> RedeemPoints(string voucherId)
        {
            if (!Guid.TryParse(voucherId, out Guid guid)) {
                return BadRequest();
            }
            var v = await _udb.UserVouchers.FindAsync(guid);
            if (v == null || v.UserID != null || v.VoucherKind != UserVoucher.Kind.LuckyPoint)
            {
                return BadRequest();
            }
            var user = await _userManager.GetUserAsync(User);
            v.UserID = user.Id;
            v.IssueTime = DateTime.Now;
            await _udb.SaveChangesAsync();
            return Json(AllPrizes.First(p => p.IsVoucher && p.RedeemItemName == v.RedeemItem));
        }

        [HttpPost]
        public async Task<ActionResult> Exchange(string voucherId)
        {
            if (!Guid.TryParse(voucherId, out Guid guid))
            {
                return BadRequest();
            }
            var v = await _udb.UserVouchers.Where(v => v.User.UserName == User.Identity.Name && v.VoucherID == guid).FirstOrDefaultAsync();
            if (v == null || v.VoucherKind != UserVoucher.Kind.Prize)
            {
                return BadRequest();
            }
            var item = AllPrizes.First(p => p.IsRealItem && p.RedeemItemName == v.RedeemItem);
            v.RedeemItem = string.Format("{0}（已折换）", v.RedeemItem);
            v.UseTime = DateTime.Now;
            var subVoucher = new UserVoucher
            {
                VoucherID = Guid.NewGuid(),
                UserID = v.UserID,
                IssueTime = DateTime.Now,
                VoucherKind = UserVoucher.Kind.LuckyPoint,
                RedeemItem = string.Format("{0}/{0}", item.PrizeLPValue),
            };
            var returnedVoucher = new UserVoucher
            {
                VoucherID = Guid.NewGuid(),
                IssueTime = DateTime.Now,
                RedeemItem = item.PrizeName,
                VoucherKind = UserVoucher.Kind.Prize,
            };
            _udb.UserVouchers.Add(subVoucher);
            _udb.UserVouchers.Add(returnedVoucher);
            await _udb.SaveChangesAsync();
            return Json(Vouchers.FromUserVoucher(subVoucher, User.Identity.Name));
        }

        [HttpPost]
        public async Task<ActionResult> RedeemCeiling()
        {
            var user = await _userManager.GetUserAsync(User);
            var ceilCount = await _udb.UserVouchers
              .CountAsync(v => v.UserID == user.Id && v.VoucherKind == UserVoucher.Kind.CeilingPrize);
            var lps = await _udb.UserVouchers
              .Where(v => v.UserID == user.Id && v.VoucherKind == UserVoucher.Kind.LuckyPoint).ToListAsync();
            var spentLp = lps.Aggregate(0, (sum, v) =>
            {
                var tokens = v.RedeemItem.Split('/');
                int curValue = int.Parse(tokens[0]);
                int totalValue = int.Parse(tokens[1]);
                return sum + totalValue - curValue;
            });
            if ((ceilCount+1)* _wheelConfig.Value.CeilingCost > spentLp)
            {
                return BadRequest();
            }
            var v = new UserVoucher
            {
                IssueTime = DateTime.Now,
                UserID = user.Id,
                VoucherID = Guid.NewGuid(),
                VoucherKind = UserVoucher.Kind.CeilingPrize,
            };
            _udb.UserVouchers.Add(v);
            await _udb.SaveChangesAsync();
            return Json(Vouchers.FromUserVoucher(v, User.Identity.Name));
        }

        [HttpGet, Authorize(Roles = "Administrator,AdManager")]
        public async Task<ActionResult> GetForUser(string name)
        {
            UserProfile u = await _userManager.FindByNameAsync(name);
            if (u == null)
            {
                return NotFound();
            }
            var vouchers = await _udb.UserVouchers.Where(v => v.UserID == u.Id).ToListAsync();
            return Json(vouchers.Select(v => Vouchers.FromUserVoucher(v, name)));
        }

        [HttpPost, Authorize(Roles = "Administrator,AdManager")]
        public async Task<ActionResult> MarkRedeemed(string voucherId)
        {
            if (!Guid.TryParse(voucherId, out Guid guid))
            {
                return BadRequest();
            }
            var v = await _udb.UserVouchers.FindAsync(guid);
            if (v == null || v.UserID == null)
            {
                return BadRequest();
            }
            v.UseTime = DateTime.Now;
            await _udb.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, Authorize(Roles = "Administrator,AdManager")]
        public async Task<ActionResult> Stock(string prizeName, int count) 
        {
            var prize = AllPrizes.FirstOrDefault(p => p.PrizeName == prizeName);
            if (prize == null || count > 100)
            {
                return BadRequest();
            }
            string item = prize.RedeemItemName;
            var kind = UserVoucher.Kind.Prize;
            if (prize.IsVoucher)
            {
                kind = UserVoucher.Kind.LuckyPoint;
            }
            for (int i = 0; i < count; i++)
            {
                var voucher = new UserVoucher
                {
                    IssueTime = DateTime.Now,
                    RedeemItem = item,
                    VoucherID = Guid.NewGuid(),
                    VoucherKind = kind,
                };
                _udb.UserVouchers.Add(voucher);
            }
            await _udb.SaveChangesAsync();
            return Ok();
        }

        [HttpGet, Authorize(Roles = "Administrator,AdManager")]
        public async Task<ActionResult> Stock()
        {
            var stocks = await _udb.UserVouchers.Where(v => v.VoucherKind == UserVoucher.Kind.LuckyPoint || v.VoucherKind == UserVoucher.Kind.Prize)
                .Select(v => new { voucher = v, name = v.User.UserName }).GroupBy(v => v.voucher.RedeemItem)
                .ToDictionaryAsync(g => g.Key, g => g.Select(v => Vouchers.FromUserVoucher(v.voucher, v.name)));
            var prizeStock = _wheelConfig.Value.WheelAPrizes
                .Concat(_wheelConfig.Value.WheelBPrizes).Where(p => p.IsRealItem)
                .Concat(_wheelConfig.Value.RedeemPrizes)
                .GroupBy(w => w.PrizeName)
                .Select(p => new
                {
                    prizeName = p.First().PrizeName,
                    stock = stocks.GetValueOrDefault(p.First().RedeemItemName),
                });
            return Json(prizeStock);
        }

        private WheelPrize DrawPrize(List<WheelPrize> wheelPrizes)
        {
            int sum = wheelPrizes.Sum(w => w.DrawPercentage);
            var rnd = new Random();
            int val = rnd.Next(sum);
            foreach(var p in wheelPrizes)
            {
                val -= p.DrawPercentage;
                if (val <= 0)
                {
                    return p;
                }
            }
            throw new ApplicationException("No prize selected. Bad config!");
        }

        private async Task<ActionResult> SpinAAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var todayDrawCount = await _udb.UserVouchers
                .Where(v => v.UserID == user.Id && v.VoucherKind == UserVoucher.Kind.WheelA && DbFunctions.DiffDays(v.IssueTime, DateTime.Today) == 0)
                .CountAsync();
            if (todayDrawCount >= 3)
            {
                return BadRequest(new { drawCount = todayDrawCount });
            }
            if (user.Points < _wheelConfig.Value.WheelACost)
            {
                return BadRequest(new { points = user.Points });
            }
            _expUtil.AddPoint(user, -_wheelConfig.Value.WheelACost);
            var prize = DrawPrize(_wheelConfig.Value.WheelAPrizes);
            var v = new UserVoucher
            {
                VoucherID = Guid.NewGuid(),
                VoucherKind = UserVoucher.Kind.WheelA,
                IssueTime = DateTime.Now,
                UserID = user.Id,
                UseTime = DateTime.Now,
                RedeemItem = prize.PrizeName,
            };
            _udb.UserVouchers.Add(v);
            SpinWheelResult result = await ProcessPrizeAsync(user, prize);
            await _udb.SaveChangesAsync();
            return Json(result);
        }

        private async Task<SpinWheelResult> ProcessPrizeAsync(UserProfile user, WheelPrize prize)
        {
            SpinWheelResult result = new SpinWheelResult
            {
                Prize = new WheelPrize {
                    PrizeName = prize.PrizeName,
                    PrizePic = prize.PrizePic,
                    PrizeLPValue = prize.PrizeLPValue,
                    IsVoucher = prize.IsVoucher,
                    IsRealItem = prize.IsRealItem,
                },
            };
            if (prize.IsRealItem)
            {
                await _semaphoreSlim.WaitAsync();
                try
                {
                    var stock = await _udb.UserVouchers
                            .Where(v => v.RedeemItem == prize.PrizeName && v.VoucherKind == UserVoucher.Kind.Prize && v.UserID == null)
                            .FirstOrDefaultAsync();
                    if (stock != null)
                    {
                        stock.UserID = user.Id;
                        stock.IssueTime = DateTime.Now;
                        result.Voucher = Vouchers.FromUserVoucher(stock, user.UserName);
                    }
                    else
                    {
                        result.Prize.PrizeName = string.Format("{0}（售罄）", prize.PrizeName);
                        var subVoucher = new UserVoucher
                        {
                            VoucherID = Guid.NewGuid(),
                            UserID = user.Id,
                            IssueTime = DateTime.Now,
                            VoucherKind = UserVoucher.Kind.LuckyPoint,
                            RedeemItem = string.Format("{0}/{0}", prize.PrizeLPValue),
                        };
                        _udb.UserVouchers.Add(subVoucher);
                        result.Voucher = Vouchers.FromUserVoucher(subVoucher, user.UserName);
                    }
                    await _udb.SaveChangesAsync();
                }
                finally
                {
                    _semaphoreSlim.Release();
                }
            }
            else if (prize.IsVoucher)
            {
                var subVoucher = new UserVoucher
                {
                    VoucherID = Guid.NewGuid(),
                    UserID = user.Id,
                    IssueTime = DateTime.Now,
                    VoucherKind = UserVoucher.Kind.LuckyPoint,
                    RedeemItem = string.Format("{0}/{0}", prize.PrizeLPValue),
                };
                _udb.UserVouchers.Add(subVoucher);
                result.Voucher = Vouchers.FromUserVoucher(subVoucher, user.UserName);
            }
            return result;
        }

        private async Task<ActionResult> SpinBAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var success = await TrySpendLPAsync(user, _wheelConfig.Value.WheelBLPCost);
            if (!success)
            {
                return BadRequest(new { err = "not enough lp" });
            }
            var prize = DrawPrize(_wheelConfig.Value.WheelBPrizes);
            var v = new UserVoucher
            {
                VoucherID = Guid.NewGuid(),
                VoucherKind = UserVoucher.Kind.WheelA,
                IssueTime = DateTime.Now,
                UserID = user.Id,
                UseTime = DateTime.Now,
                RedeemItem = prize.PrizeName,
            };
            _udb.UserVouchers.Add(v);
            SpinWheelResult result = await ProcessPrizeAsync(user, prize);
            await _udb.SaveChangesAsync();
            return Json(result);
        }

        private async Task<bool> TrySpendLPAsync(UserProfile user, int cost)
        {
            if (cost <= 0)
            {
                throw new ArgumentException($"Invalid cost {cost}");
            }
            var lps = await _udb.UserVouchers
               .Where(v => v.UserID == user.Id && v.VoucherKind == UserVoucher.Kind.LuckyPoint && !v.RedeemItem.StartsWith("0/")).ToListAsync();
            foreach (var lp in lps)
            {
                var tokens = lp.RedeemItem.Split('/');
                int curValue = int.Parse(tokens[0]);
                int totalValue = int.Parse(tokens[1]);
                if (cost <= curValue)
                {
                    curValue -= cost;
                    cost = 0;
                }
                else
                {
                    cost -= curValue;
                    curValue = 0;
                }
                lp.RedeemItem = string.Format("{0}/{1}", curValue, totalValue);
                lp.UseTime = DateTime.Now;
                if (cost == 0)
                {
                    break;
                }
            }
            if (cost > 0)
            {
                return false;
            }
            return true;
        }
    }
}
