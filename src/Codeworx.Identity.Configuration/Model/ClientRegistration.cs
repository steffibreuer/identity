﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Codeworx.Identity.Model;

namespace Codeworx.Identity.Configuration.Model
{
    public class ClientRegistration : IClientRegistration
    {
        public ClientRegistration(string clientId, string clientSecretHash, ClientType clientType, TimeSpan tokenExpiration, IEnumerable<string> validRedirectUrls = null, IUser user = null)
        {
            ClientId = clientId;
            ClientSecretHash = clientSecretHash;
            ClientType = clientType;
            TokenExpiration = tokenExpiration;
            if (validRedirectUrls != null)
            {
                ValidRedirectUrls = validRedirectUrls.Select(p => new Uri(p)).ToImmutableList();
            }
            else
            {
                ValidRedirectUrls = ImmutableList.Create<Uri>();
            }

            User = user;
        }

        public string ClientId { get; }

        public string ClientSecretHash { get; }

        public TimeSpan TokenExpiration { get; }

        public IReadOnlyList<Uri> ValidRedirectUrls { get; }

        public ClientType ClientType { get; }

        public IUser User { get; }
    }
}