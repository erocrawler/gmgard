using GmGard.Models;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using static GmGard.Models.UserQuest;

namespace GmGard.Services
{
    public class TitleService
    {
        private readonly UsersContext _usersContext;
        private readonly List<TitleConfig> _titleConfigs;
        public TitleService(UsersContext usersContext)
        {
            _usersContext = usersContext;
            _titleConfigs = _usersContext.TitleConfigs.ToList();
        }

        public string GetTitleName(int titleId)
        {
            return _titleConfigs.FirstOrDefault(x => x.TitleID == titleId)?.TitleName ?? "";
        }

        public int? GetTitleId(string titleName)
        {
            return _titleConfigs.FirstOrDefault(x => x.TitleName == titleName)?.TitleID;
        }

        public Dictionary<int, string> AllTitleBackgrounds => _titleConfigs.ToDictionary(x => x.TitleID, x => x.TitleImage);

        public string GetTitleBackground(int titleId)
        {
            return _titleConfigs.FirstOrDefault(x => x.TitleID == titleId)?.TitleImage ?? "";
        }

        public IEnumerable<TitleConfig> AllUserTitles(UserQuest user)
        {
            return user
                .AllTitles()
                .Join(_titleConfigs, x => x, y => y.TitleID, (x, y) => y);
        }
    }
}
