﻿using System.Threading.Tasks;
using Codeworx.Identity.Model;

namespace Codeworx.Identity.OAuth
{
    public interface IClientAuthenticationService
    {
        Task<IClientRegistration> AuthenticateClient(TokenRequest request);
    }
}