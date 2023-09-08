using System;
using System.Collections;
using System.Collections.Generic;

namespace GmGard.Models
{
    public partial class UserQuest
    {
        public enum UserProfession
        {
            None = 0,
            Loli,
            Onee,
            Otokonoko,
            Futa,
            Shiro,
        }

        private static readonly Dictionary<UserProfession, string> professionName = new Dictionary<UserProfession, string>
        {
            { UserProfession.Loli, "萝莉" },
            { UserProfession.Onee, "御姐" },
            { UserProfession.Otokonoko, "伪娘" },
            { UserProfession.Futa, "扶她" },
            { UserProfession.Shiro, "白学家" }
        };

        public static string ProfessionName(UserProfession profession)
        {
            professionName.TryGetValue(profession, out string name);
            return name;
        }

        public static UserProfession GetProfession(string profession)
        {
            foreach (var pair in professionName)
            {
                if (profession == pair.Value)
                {
                    return pair.Key;
                }
            }
            return UserProfession.None;
        }

        public bool HasTitle(int title)
        {
            if (title == 0) 
            {
                return true;
            }
            if (Titles == null)
            {
                return false;
            }
            BitArray ba = new(Titles);
            if (ba.Length < title)
            {
                return false;
            }
            return ba.Get(title - 1);
        }

        public IEnumerable<int> AllTitles()
        {
            var titles = new List<int>();
            if (Titles == null)
            {
                return titles;
            }
            BitArray ba = new(Titles);
            for (int i = 0; i < ba.Count; i++)
            {
                if (ba.Get(i))
                {
                    titles.Add(i + 1);
                }
            }
            return titles;
        }

        public void AddTitle(int title)
        {
            Titles ??= new byte[1];
            var bitPos = title - 1;
            int len = Math.Max(Titles.Length, (bitPos + 8) / 8);
            BitArray ba = new(Titles)
            {
                Length = len * 8
            };
            ba.Set(bitPos, true);
            var newTitles = new byte[len];
            ba.CopyTo(newTitles, 0);
            Titles = newTitles;
        }
    }
}