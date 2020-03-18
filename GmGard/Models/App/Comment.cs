using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class Comment
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CommentType
        {
            None = 0,
            Blog,
        }

        public CommentType Type { get; set; }
        public int ItemId { get; set; }
        public int CommentId { get; set; }
        public string Content { get; set; }
        public string Author { get; set; }
        public DateTime CreateDate { get; set; }
        public int? Rating { get; set; }
        public int UpvoteCount { get; set; }
        public int DownvoteCount { get; set; }
        public bool? IsUpvoted { get; set; }
        public bool? IsDownvoted { get; set; }
        public IEnumerable<CommentReply> Replies { get; set; }
    }
}
