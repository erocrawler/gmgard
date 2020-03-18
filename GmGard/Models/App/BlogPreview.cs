using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class BlogPreview
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public string Brief { get; set; }
        public string ThumbUrl { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateDate { get; set; }
        public User Author { get; set; }
    }
}
