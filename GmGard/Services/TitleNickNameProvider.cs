using GmGard.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public class TitleNickNameProvider : INickNameProvider
    {
        private UsersContext _udb;
        private IMemoryCache _cache;
        private TitleService _titleService;
        private ILogger<TitleNickNameProvider> _logger;

        public TitleNickNameProvider(IMemoryCache cache, UsersContext udb, TitleService titleService, ILogger<TitleNickNameProvider> logger)
        {
            _udb = udb;
            _cache = cache;
            _titleService = titleService;
            _logger = logger;
        }

        public string GetNickName(string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return string.Empty;
            }
            user = user.ToLower();
            string nick = _cache.Get<string>("nick" + user);
            
            if (nick == string.Empty)
            {
                nick = user;
            }
            else if (nick == null)
            {
                _logger.LogDebug("Cache miss for user: {User}", user);
                var result = _udb.Users.Where(u => u.UserName.ToLower() == user).Select(u => new {
                    u.UserName,
                    u.NickName,
                    Title = u.quest == null ? new int?() : u.quest.Title
                }).SingleOrDefault();
                
                nick = result == null ? string.Empty : BuildNickName(result.Title, result.NickName, result.UserName);
                _cache.Set("nick" + user, nick);
            }
            
            return nick;
        }

        public IDictionary<string, string> GetNickNames(IEnumerable<string> users)
        {
            var names = new HashSet<string>(users, StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<string, string>(names.Count, StringComparer.OrdinalIgnoreCase);
            var uncached = new List<string>();
            foreach (var n in names)
            {
                string cached = _cache.Get<string>("nick" + n.ToLower());
                if (cached != null)
                {
                    result.Add(n, cached == string.Empty ? n : cached);
                }
                else
                {
                    uncached.Add(n);
                }
            }
            if (uncached.Count > 0)
            {
                var uncachedLower = uncached.Select(n => n.ToLower()).ToList();
                var name2nick = _udb.Users.Where(u => uncachedLower.Contains(u.UserName.ToLower()))
                    .ToDictionary(u => u.UserName.ToLower(), u => new {
                        u.UserName,
                        u.NickName,
                        Title = u.quest == null ? new int?() : u.quest.Title
                    });
                foreach (var name in uncached)
                {
                    var key = name.ToLower();
                    string nick = string.Empty;
                    if (name2nick.ContainsKey(key))
                    {
                        var r = name2nick[key];
                        nick = BuildNickName(r.Title, r.NickName, r.UserName);
                    }
                    _cache.Set("nick" + key, nick, new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions 
                    { 
                        SlidingExpiration = TimeSpan.FromMinutes(20),
                        Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal
                    });
                    result.Add(name, nick == string.Empty ? name : nick);
                }
            }
            return result;
        }

        public void UpdateNickNameCache(UserProfile user)
        {
            var nick = BuildNickName(user.quest?.Title, user.NickName, user.UserName);
            _logger.LogInformation("Updated nickname cache for {Username}", user.UserName);
            
            _cache.Set("nick" + user.UserName.ToLower(), nick, 
                new Microsoft.Extensions.Caching.Memory.MemoryCacheEntryOptions 
                { 
                    SlidingExpiration = TimeSpan.FromMinutes(20),
                    Priority = Microsoft.Extensions.Caching.Memory.CacheItemPriority.Normal
                });
        }

        private string BuildNickName(int? title, string nickname, string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return string.Empty;
            }
            StringBuilder sb = new();
            if (title.HasValue && title > 0)
            {
                var titleName = _titleService.GetTitleName(title.Value);
                sb.AppendFormat("[{0}] ", titleName);
            }
            
            if (string.IsNullOrEmpty(nickname))
            {
                nickname = username;
            }
            
            sb.Append(nickname);
            return sb.ToString();
        }
    }
}