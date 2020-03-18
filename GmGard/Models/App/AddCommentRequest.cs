using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GmGard.Models.App
{
    public class AddCommentRequest
    {
        public Comment.CommentType Type { get; set; }
        public int ItemId { get; set; }
        public string Content { get; set; }
        public int? Rating { get; set; }
    }
}
