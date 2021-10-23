using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class SendMessageRequest
    {
        [MaxLength(80, ErrorMessage = "标题不得超过80字符")]
        public string Title { get; set; }
        [Required(ErrorMessage = "请输入正文"), DataType(DataType.MultilineText)]
        public string Content { get; set; }
        [Required(ErrorMessage = "请输入收件人"), MaxLength(30)]
        public string Recipient { get; set; }
    }
}
