using GmGard.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GmGard.Services
{
    public class NickNameProvider : INickNameProvider
    {
        private UsersContext _udb;
        private IMemoryCache _cache;

        public NickNameProvider(IMemoryCache cache, UsersContext udb)
        {
            _udb = udb;
            _cache = cache;
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
                nick = _udb.Users.Where(u => u.UserName == user).Select(u => u.NickName).SingleOrDefault();
                _cache.Set("nick" + user, nick ?? string.Empty);
            }
            return nick;
        }

        public IDictionary<string, string> GetNickNames(IEnumerable<string> users)
        {
            var names = new HashSet<string>(users, StringComparer.OrdinalIgnoreCase);
            var result = new Dictionary<string, string>(names.Count(), StringComparer.OrdinalIgnoreCase);
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
                var name2nick = _udb.Users.Where(u => uncached.Contains(u.UserName)).ToDictionary(u => u.UserName.ToLower(), u => u.NickName);
                foreach (var name in uncached)
                {
                    string nick;
                    name2nick.TryGetValue(name.ToLower(), out nick);
                    nick = nick ?? string.Empty;
                    _cache.Set("nick" + name.ToLower(), nick);
                    result.Add(name, nick == string.Empty ? name : nick);
                }
            }
            return result;
        }

        public void UpdateNickNameCache(UserProfile user)
        {
            _cache.Set("nick" + user.UserName.ToLower(), user.NickName);
        }
    }
}
