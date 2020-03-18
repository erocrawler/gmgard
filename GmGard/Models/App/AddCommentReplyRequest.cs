using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GmGard.Models.App
{
    public class AddCommentReplyRequest
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
    }
}
