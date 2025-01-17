﻿namespace Codeworx.Identity.Login
{
    internal class FormsInvitationRegistrationInfo : ILoginRegistrationInfo
    {
        public FormsInvitationRegistrationInfo(string providerId, string userName, string error = null)
        {
            UserName = userName;
            ProviderId = providerId;
            Error = error;
        }

        public string UserName { get; }

        public string ProviderId { get; }

        public string Error { get; }

        public string Template => Constants.Templates.FormsInvitation;

        public bool HasRedirectUri(out string redirectUrl)
        {
            redirectUrl = null;
            return false;
        }
    }
}