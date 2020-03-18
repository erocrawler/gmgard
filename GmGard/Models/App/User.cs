using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string NickName { get; set; }
        public int Points { get; set; }
        public int Experience { get; set; }
        public int Level { get; set; }
        public int ConsecutiveSign { get; set; }
        public DateTime? LastSignDate { get; set; }
        public string Avatar { get; set; }
        public string Comment { get; set; }
        public IEnumerable<string> Roles { get; set; }

        static public User FromUserProfile(UserProfile profile, string avatar = null)
        {
            return new User
            {
                UserId = profile.Id,
                UserName = profile.UserName,
                NickName = profile.NickName,
                Points = profile.Points,
                Experience = profile.Experience,
                Level = profile.Level,
                Comment = profile.UserComment,
                ConsecutiveSign = profile.ConsecutiveSign,
                LastSignDate = profile.LastSignDate < new DateTime(2000, 1, 1) ? new DateTime?() : profile.LastSignDate,
                Avatar = avatar,
            };
        }
    }
}
