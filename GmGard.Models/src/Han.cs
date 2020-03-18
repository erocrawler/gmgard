using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GmGard.Models
{
    public class HanGroup
    {
        public int HanGroupID { get; set; }

        [MaxLength(20)]
        public string GroupUri { get; set; }

        [MaxLength(50)]
        public string Title { get; set; }

        [MaxLength(512)]
        public string Logo { get; set; }

        [MaxLength(2048)]
        public string Intro { get; set; }

        public virtual List<HanGroupBlog> blogs { get; set; }
        public virtual List<HanGroupMember> members { get; set; }
    }

    public class HanGroupMember
    {
        [Key, Column(Order = 1)]
        public int HanGroupID { get; set; }

        [ForeignKey("HanGroupID")]
        public virtual HanGroup hangroup { get; set; }

        [Key, Column(Order = 2), MaxLength(20)]
        public string Username { get; set; }

        public int GroupLvl { get; set; }
    }

    public class HanGroupBlog
    {
        [Key, Column(Order = 1)]
        public int HanGroupID { get; set; }

        [ForeignKey("HanGroupID")]
        public virtual HanGroup hangroup { get; set; }

        [Key, Column(Order = 2)]
        public int BlogID { get; set; }

        [ForeignKey("BlogID")]
        public virtual Blog blog { get; set; }
    }

    public class HanGroupMemberComparer : IEqualityComparer<HanGroupMember>
    {
        public bool Equals(HanGroupMember x, HanGroupMember y)
        {
            return x.HanGroupID == y.HanGroupID && x.Username == y.Username;
        }

        public int GetHashCode(HanGroupMember obj)
        {
            return Tuple.Create(obj.HanGroupID, obj.Username).GetHashCode();
        }
    }

    public class HanGroupBlogComparer : IEqualityComparer<HanGroupBlog>
    {
        public bool Equals(HanGroupBlog x, HanGroupBlog y)
        {
            return x.HanGroupID == y.HanGroupID && x.BlogID == y.BlogID;
        }

        public int GetHashCode(HanGroupBlog obj)
        {
            return obj.GetHashCode();
        }
    }
}