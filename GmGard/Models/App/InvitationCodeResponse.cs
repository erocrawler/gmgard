using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class InvitationCodeResponse
    {
        public User User { get; set; }
        public IEnumerable<CodeDetail> Codes { get; set; }
        public User InvitedBy { get; set; }
    }

    public class CodeDetail
    {
        public string Code { get; set; }
        public User UsedBy { get; set; }
    }
}
