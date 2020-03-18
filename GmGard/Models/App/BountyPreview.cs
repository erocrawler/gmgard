using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class BountyPreview
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Author { get; set; }
        public string AuthorAvatar { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Image { get; set; }
        public int Prize { get; set; }
        public bool IsAccepted { get; set; }
        public int AnswerCount { get; set; }
    }
}
