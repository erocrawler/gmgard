using GmGard.Models;
using Microsoft.Extensions.Caching.Memory;
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

        public TitleNickNameProvider(IMemoryCache cache, UsersContext udb, TitleService titleService)
        {
            _udb = udb;
            _cache = cache;
            _titleService = titleService;
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
                var result = _udb.Users.Where(u => u.UserName == user).Select(u => new {
                    u.NickName,
                    Title = u.quest == null ? new int?() : u.quest.Title
                }).SingleOrDefault();
                nick = result == null ? string.Empty : BuildNickName(result.Title, result.NickName, user);
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
                var name2nick = _udb.Users.Where(u => uncached.Contains(u.UserName))
                    .ToDictionary(u => u.UserName.ToLower(), u => new {
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
                        nick = BuildNickName(r.Title, r.NickName, name);
                    }
                    _cache.Set("nick" + key, nick);
                    result.Add(name, nick == string.Empty ? name : nick);
                }
            }
            return result;
        }

        public void UpdateNickNameCache(UserProfile user)
        {
            _cache.Set("nick" + user.UserName.ToLower(), BuildNickName(user.quest?.Title, user.NickName, user.UserName));
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
                sb.AppendFormat("[{0}] ", _titleService.GetTitleName(title.Value));
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