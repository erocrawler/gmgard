using GmGard.Models;
using GmGard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

namespace GmGard.Controllers
{
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    public class AvatarController : Controller
    {
        private UsersContext _db;
        private IMemoryCache _cache;

        private const string defaultavatar = "/Images/nazoshinshi.jpg";

        public AvatarController(UsersContext udb, IMemoryCache cache)
        {
            _db = udb;
            _cache = cache;
        }

        //
        // GET: /Avatar/Show/name
        public ActionResult Show(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name) || !User.Identity.IsAuthenticated)
            {
                return Redirect(defaultavatar);
            }
            else
            {
                try
                {
                    Pictures PicData = _cache.Get<Pictures>(CacheService.GetAvatarCacheKey(name)) ?? _db.Avatars.FirstOrDefault(p => p.PicUserName == name) ?? new Pictures();
                    _cache.Set(CacheService.GetAvatarCacheKey(name), PicData);
                    if (PicData.PicName == null)
                    {
                        return Redirect(defaultavatar);
                    }
                    return Redirect("/Images/Avatar/" + System.Net.WebUtility.UrlEncode(PicData.PicName) + "?" + PicData.PicDate.Ticks);
                }
                catch
                {
                    return Redirect(defaultavatar);
                }
            }
        }
    }
}