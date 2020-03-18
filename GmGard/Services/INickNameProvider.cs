using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Services
{
    public interface INickNameProvider
    {
        string GetNickName(string UserName);
        IDictionary<string, string> GetNickNames(IEnumerable<string> users);
        void UpdateNickNameCache(UserProfile user);
    }
}
