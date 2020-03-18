using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class CommentReply
    {
        public int ReplyId { get; set; }
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
