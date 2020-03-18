using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class AddReplyResponse
    {
        public string Message { get; set; }
        public int CommentId { get; set; }
        public int? ReplyId { get; set; }
    }
}
