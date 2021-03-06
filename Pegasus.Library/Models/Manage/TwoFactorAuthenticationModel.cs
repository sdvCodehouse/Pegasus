﻿namespace Pegasus.Library.Models.Manage
{
    public class TwoFactorAuthenticationModel : ManageBaseModel
    {
        public bool HasAuthenticator { get; set; }
        public int RecoveryCodesLeft { get; set; }
        public bool Is2FaEnabled { get; set; }
        public bool IsMachineRemembered { get; set; }
    }
}
