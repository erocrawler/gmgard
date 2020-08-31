using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using GmGard.Models.App;
using GmGard.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Entity;
using GmGard.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GmGard.Controllers.App
{
    [Area("App")]
    [Produces("application/json")]
    [Route("api/Gacha/[action]")]
    [EnableCors("GmAppOrigin")]
    [Authorize]
    [ApiController]
    public class GachaController : AppControllerBase
    {
        private readonly UsersContext db_;
        protected readonly IMemoryCache cache_;
        protected readonly ExpUtil expUtil_;
        private readonly Random random_;
        private readonly GachaBonusService gachaBonus_;

        const int COST = 10;

        public GachaController(UsersContext db, IMemoryCache cache, ExpUtil expUtil, GachaBonusService gachaBonus)
        {
            db_ = db;
            cache_ = cache;
            expUtil_ = expUtil;
            random_ = new Random();
            gachaBonus_ = gachaBonus;
        }

        [HttpPost]
        public async Task<IActionResult> GetResult([FromBody]GachaRequest request)
        {
            if (!ValidatePool(request.Pool))
            {
                return BadRequest();
            }
            int count = request.Count;
            var currentCost = COST * count;
            var user = await db_.Users.SingleOrDefaultAsync(u => u.UserName == User.Identity.Name);
            if (user.Points < currentCost)
            {
                return Json(new GachaResult
                {
                    Success = false,
                    ErrorMessage = "棒棒糖不足",
                });
            }
            expUtil_.AddPoint(user, -currentCost);
            var result = new List<GachaItem>(count);
            for (int i = 0; i < count; ++i)
            {
                result.Add(await DoGachaAsync(request.Pool));
            }
            if (count == 10 && result.All(i => i.Rarity < 3))
            {
                // 10连保底
                result[random_.Next(10)] = await DoGachaAsync(request.Pool, 3);
            }
            db_.UserGachas.AddRange(result.Select(r => new UserGacha
            {
                ItemId = r.Id,
                UserID = user.Id,
                GachaTime = DateTime.Now,
                PoolName = request.Pool,
            }));
            await db_.SaveChangesAsync();

            var bonusResult = await gachaBonus_.HandleForItemsAsync(result);

            return Json(new GachaResult
            {
                Success = true,
                Items = result.Select(r => new GachaResult.GachaItem { Name = r.Name, Rarity = r.Rarity }),
                Rewards = bonusResult,
            });
        }

        [HttpGet]
        public JsonResult GetCurrentPools()
        {
            return Json(AllPools.Where(p => ValidatePool(p.Key)).Select(p => p.Value).OrderByDescending(v => v.StartTime));
        }

        [HttpGet]
        public async Task<JsonResult> GetStats()
        {
            var allItems = await db_.GachaItems.ToListAsync();
            var userItems = await db_.UserGachas.Where(u => u.User.UserName == User.Identity.Name).GroupBy(g => g.ItemId).Select(g => new
            {
                Id = g.Key,
                ItemCount = g.Count(),
            }).ToListAsync();
            return Json(new GachaStats
            {
                TotalCards = allItems.Count(),
                Progresses = await gachaBonus_.GetProgressForUserAsync(),
                UserCards = userItems.Join(allItems, u => u.Id, a => a.Id, (u, a) => new GachaItemDetails
                {
                    Name = a.Name,
                    Description = a.Description,
                    Title = a.Title,
                    Rarity = a.Rarity,
                    ItemCount = u.ItemCount,
                }),
            });
        }

        private class CommonPool : GachaSetting
        {
            public override int[] RarityDistribution => new[] { 30, 60, 85, 100 };
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.Common;
        }

        private class FourthAnniversaryPool : GachaSetting
        {
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.FourthAnniversary;
            public override DateTime? StartTime => new DateTime(2017, 10, 4, 0, 0, 0);
            public override DateTime? EndTime => new DateTime(2017, 12, 31, 23, 59, 59);
        }

        private class Touhou1stPool : GachaSetting
        {
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.Touhou1st;
            public override DateTime? StartTime => new DateTime(2018, 2, 6, 0, 0, 0);
            public override DateTime? EndTime => new DateTime(2018, 4, 30, 23, 59, 59);
        }

        private class April2018Pool : GachaSetting
        {
            public override int[] RarityDistribution => new[] { 28, 54, 79, 94, 100 }; // 28%, 26%, 25%, 15%, 6%
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.April2018;
            public override DateTime? StartTime => new DateTime(2018, 4, 1, 0, 0, 0);
            public override DateTime? EndTime => new DateTime(2018, 4, 15, 23, 59, 59);
        }

        private class June2018Pool : GachaSetting
        {
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.June2018;
            public override DateTime? StartTime => new DateTime(2018, 6, 3, 0, 0, 0);
            public override DateTime? EndTime => new DateTime(2018, 6, 30, 23, 59, 59);
        }

        private class April2019Pool : GachaSetting
        {
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.April2019;
            public override DateTime? StartTime => new DateTime(2019, 3, 31, 0, 0, 0);
            public override DateTime? EndTime => new DateTime(2019, 4, 30, 23, 59, 59);
        }

        private class August2020Pool : GachaSetting
        {
            public override GachaPool.PoolName PoolName => GachaPool.PoolName.August2020;
            public override DateTime? StartTime => new DateTime(2020, 8, 29, 0, 0, 0);
            public override DateTime? EndTime => new DateTime(2020, 9, 30, 23, 59, 59);
        }

        private readonly Dictionary<GachaPool.PoolName, GachaSetting> AllPools = new Dictionary<GachaPool.PoolName, GachaSetting>
        {
            { GachaPool.PoolName.Common, new CommonPool() },
            { GachaPool.PoolName.FourthAnniversary, new FourthAnniversaryPool() },
            { GachaPool.PoolName.Touhou1st, new Touhou1stPool() },
            { GachaPool.PoolName.April2018, new April2018Pool() },
            { GachaPool.PoolName.June2018, new June2018Pool() },
            { GachaPool.PoolName.April2019, new April2019Pool() },
            { GachaPool.PoolName.August2020, new August2020Pool() }
        };

        private bool ValidatePool(GachaPool.PoolName name)
        {
            if (!AllPools.ContainsKey(name))
            {
                return false;
            }
            var pool = AllPools[name];
            if (pool.StartTime.HasValue && DateTime.Now < pool.StartTime)
            {
                return false;
            }
            if (pool.EndTime.HasValue && DateTime.Now > pool.EndTime)
            {
                return false;
            }
            return true;
        }

        private async Task<IEnumerable<GachaPool>> AllItemsAsync()
        {
            return await cache_.GetOrCreateAsync("~GachaItems", (entry) =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(30));
                return db_.GachaPools.Include(p => p.Item).ToListAsync();
            });
        }

        private async Task<GachaItem> DoGachaAsync(GachaPool.PoolName poolName, int FixedRarity = 0)
        {
            var index = random_.Next(100);
            var rarity = FixedRarity;
            if (rarity == 0)
            {
                for (int i = 0; i < AllPools[poolName].RarityDistribution.Length; ++i)
                {
                    if (index < AllPools[poolName].RarityDistribution[i])
                    {
                        rarity = i + 1;
                        break;
                    }
                }
            }
            var allItems = await AllItemsAsync();
            var itemsInPool = allItems.Where(r => r.Name == poolName && r.Item.Rarity == rarity).OrderBy(_ => random_.Next());
            int totalWeight = itemsInPool.Sum(p => p.Weight);
            int itemIndex = random_.Next(totalWeight) + 1; // non-zero
            int current = 0;
            foreach (var item in itemsInPool)
            {
                current += item.Weight;
                if (current >= itemIndex)
                {
                    return item.Item;
                }
            }
            // Only possible if no item has the given rarity at this pool.
            throw new ApplicationException("Item exhausted! Bad pool config!");
        }
    }
}
