﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class GetInvitationCodeRequest
    {
        public string UserName { get; set; }
        public string Code { get; set; }
    }
}
