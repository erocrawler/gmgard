using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class BlogDetails
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string Content { get; set; }
        public string[] ImageUrls { get; set; }
        public string ThumbUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public int CategoryId { get; set; }
        public int? ParentCategoryId { get; set; }
        public User Author { get; set; }
        public string AuthorDesc { get; set; }
        public bool? IsApproved { get; set; }
        public long VisitCount { get; set; }
        public BlogLink[] Links { get; set; }
        public BlogRatingDisplay Rating { get; set; }
        public Dictionary<int, string> Tags { get; set; }
        public Comment[] TopComments { get; set; }

        public bool LockTags { get; set; }
        public bool NoRate { get; set; }
        public bool NoComment { get; set; }

        public bool IsFavorite { get; set; }
        public int? UserRating { get; set; }
    }
}
