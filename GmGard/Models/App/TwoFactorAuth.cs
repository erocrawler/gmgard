using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GmGard.Models.App
{
    public class TwoFactorAuthenticationModel
    {
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public bool Is2faEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
    }

    public class TwoFactorAuthSharedKey
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
    }
}
