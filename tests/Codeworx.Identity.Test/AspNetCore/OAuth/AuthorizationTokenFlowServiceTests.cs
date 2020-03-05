﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Codeworx.Identity.AspNetCore.OAuth;
using Codeworx.Identity.Cryptography.Json;
using Codeworx.Identity.Model;
using Codeworx.Identity.OAuth;
using Codeworx.Identity.OAuth.Authorization;
using Codeworx.Identity.Token;
using Moq;
using Xunit;

namespace Codeworx.Identity.Test.AspNetCore.OAuth
{
    public class AuthorizationTokenFlowServiceTests
    {
        [Fact]
        public async Task AuthorizeRequest_ClientNotAuthorized_ReturnsError()
        {
            const string ClientIdentifier = "6D5CD2A0-59D0-47BD-86A1-BF1E600935C3";

            var request = new AuthorizationRequestBuilder().WithClientId(ClientIdentifier)
                                                           .WithResponseType(Identity.OAuth.Constants.ResponseType.Token)
                                                           .Build();

            var identityServiceStub = new Mock<IIdentityService>();

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.Is<string>(v => v == "Authorized")))
                             .Returns(true);

            var clientRegistrationStub = new Mock<IClientRegistration>();
            clientRegistrationStub.SetupGet(p => p.ClientId)
                                  .Returns(ClientIdentifier);
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            var oAuthClientServiceStub = new Mock<IClientService>();
            oAuthClientServiceStub.Setup(p => p.GetById(It.Is<string>(x => x == ClientIdentifier)))
                                  .ReturnsAsync(clientRegistrationStub.Object);

            var scopeServiceStub = new Mock<IScopeService>();

            var instance = new AuthorizationTokenFlowService(identityServiceStub.Object, oAuthClientServiceStub.Object, scopeServiceStub.Object, new ITokenProvider[] { });
            var identity = GetIdentity();

            var result = await instance.AuthorizeRequest(request, identity);

            Assert.IsType<UnauthorizedClientResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_ClientNotRegistered_ReturnsError()
        {
            var request = new AuthorizationRequestBuilder().Build();
            var oAuthClientServiceStub = new Mock<IClientService>();
            var scopeServiceStub = new Mock<IScopeService>();
            var tokenProvidersStub = new Mock<IEnumerable<ITokenProvider>>();
            var identityServiceStub = new Mock<IIdentityService>();

            var instance = new AuthorizationTokenFlowService(identityServiceStub.Object, oAuthClientServiceStub.Object, scopeServiceStub.Object, tokenProvidersStub.Object);
            var identity = GetIdentity();

            var result = await instance.AuthorizeRequest(request, identity);

            Assert.IsType<InvalidRequestResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_UnknownScope_ReturnsError()
        {
            const string ClientIdentifier = "6D5CD2A0-59D0-47BD-86A1-BF1E600935C3";
            const string KnownScope = "knownScope";

            var request = new AuthorizationRequestBuilder().WithClientId(ClientIdentifier)
                                                           .WithResponseType(Identity.OAuth.Constants.ResponseType.Token)
                                                           .WithScope("unknownScope")
                                                           .Build();

            var identityServiceStub = new Mock<IIdentityService>();

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.Is<string>(v => v == Identity.OAuth.Constants.ResponseType.Token)))
                             .Returns(true);

            var clientRegistrationStub = new Mock<IClientRegistration>();
            clientRegistrationStub.SetupGet(p => p.ClientId)
                                  .Returns(ClientIdentifier);
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            var oAuthClientServiceStub = new Mock<IClientService>();
            oAuthClientServiceStub.Setup(p => p.GetById(It.Is<string>(x => x == ClientIdentifier)))
                                  .ReturnsAsync(clientRegistrationStub.Object);

            var scopeStub = new Mock<IScope>();
            scopeStub.SetupGet(p => p.ScopeKey)
                     .Returns(KnownScope);

            var scopeServiceStub = new Mock<IScopeService>();
            scopeServiceStub.Setup(p => p.GetScopes())
                            .ReturnsAsync(new List<IScope> { scopeStub.Object });

            var tokenProvidersStub = new Mock<IEnumerable<ITokenProvider>>();

            var instance = new AuthorizationTokenFlowService(identityServiceStub.Object, oAuthClientServiceStub.Object, scopeServiceStub.Object, tokenProvidersStub.Object);
            var identity = GetIdentity();

            var result = await instance.AuthorizeRequest(request, identity);

            Assert.IsType<UnknownScopeResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_ValidRequestWithEmptyScope_ReturnsResponse()
        {
            const string AuthorizationToken = "SAMPLE_ACCESS_TOKEN";
            const string ClientIdentifier = "6D5CD2A0-59D0-47BD-86A1-BF1E600935C3";
            const string KnownScope = "knownScope";

            var request = new AuthorizationRequestBuilder().WithClientId(ClientIdentifier)
                                                           .WithResponseType(Identity.OAuth.Constants.ResponseType.Token)
                                                           .WithScope(string.Empty)
                                                           .Build();

            var identityServiceStub = new Mock<IIdentityService>();
            var clientRegistrationStub = new Mock<IClientRegistration>();
            var oAuthClientServiceStub = new Mock<IClientService>();
            var scopeStub = new Mock<IScope>();
            var scopeServiceStub = new Mock<IScopeService>();

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.IsAny<string>()))
                .Returns(true);

            clientRegistrationStub.SetupGet(p => p.ClientId)
                                  .Returns(ClientIdentifier);
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            oAuthClientServiceStub.Setup(p => p.GetById(It.Is<string>(x => x == ClientIdentifier)))
                                  .ReturnsAsync(clientRegistrationStub.Object);

            scopeStub.SetupGet(p => p.ScopeKey)
                     .Returns(KnownScope);

            scopeServiceStub.Setup(p => p.GetScopes())
                            .ReturnsAsync(new List<IScope> { scopeStub.Object });

            var tokenStub = new Mock<IToken>();
            tokenStub.Setup(p => p.SerializeAsync())
                .ReturnsAsync(AuthorizationToken);

            var tokenProviderStub = new Mock<ITokenProvider>();
            tokenProviderStub.SetupGet(p => p.ConfigurationType)
                .Returns(typeof(JwtConfiguration));
            tokenProviderStub.SetupGet(p => p.TokenType)
                .Returns("jwt");
            tokenProviderStub.Setup(p => p.CreateAsync(It.IsAny<JwtConfiguration>()))
                .ReturnsAsync(tokenStub.Object);

            identityServiceStub.Setup(p => p.GetIdentityAsync(It.IsAny<ClaimsIdentity>()))
                .ReturnsAsync(new IdentityData(Constants.DefaultAdminUserId, Constants.DefaultAdminUserName, new[] { new TenantInfo { Key = Constants.DefaultTenantId, Name = Constants.DefaultTenantName } }));

            var instance = new AuthorizationTokenFlowService(identityServiceStub.Object, oAuthClientServiceStub.Object, scopeServiceStub.Object, new[] { tokenProviderStub.Object });
            var identity = GetIdentity();

            var result = await instance.AuthorizeRequest(request, identity);

            Assert.IsType<SuccessfulTokenAuthorizationResult>(result);
            Assert.Equal(AuthorizationToken, (result.Response as AuthorizationTokenResponse)?.Token);
        }

        [Fact]
        public async Task AuthorizeRequest_ValidRequestWithoutScope_ReturnsResponse()
        {
            const string AuthorizationToken = "AuthorizationToken";
            const string ClientIdentifier = "6D5CD2A0-59D0-47BD-86A1-BF1E600935C3";
            const string KnownScope = "knownScope";

            var request = new AuthorizationRequestBuilder().WithClientId(ClientIdentifier)
                                                           .WithResponseType(Identity.OAuth.Constants.ResponseType.Token)
                                                           .WithScope(null)
                                                           .Build();

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.Is<string>(v => v == Identity.OAuth.Constants.ResponseType.Token)))
                             .Returns(true);

            var clientRegistrationStub = new Mock<IClientRegistration>();
            var oAuthClientServiceStub = new Mock<IClientService>();
            var scopeStub = new Mock<IScope>();
            var scopeServiceStub = new Mock<IScopeService>();

            clientRegistrationStub.SetupGet(p => p.ClientId)
                                  .Returns(ClientIdentifier);
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            oAuthClientServiceStub.Setup(p => p.GetById(It.Is<string>(x => x == ClientIdentifier)))
                                  .ReturnsAsync(clientRegistrationStub.Object);

            scopeStub.SetupGet(p => p.ScopeKey)
                     .Returns(KnownScope);

            scopeServiceStub.Setup(p => p.GetScopes())
                            .ReturnsAsync(new List<IScope> { scopeStub.Object });

            var identityServiceStup = new Mock<IIdentityService>();

            identityServiceStup.Setup(p => p.GetIdentityAsync(It.IsAny<ClaimsIdentity>()))
              .ReturnsAsync(new IdentityData(Constants.DefaultAdminUserId, Constants.DefaultAdminUserName, new[] { new TenantInfo { Key = Constants.DefaultTenantId, Name = Constants.DefaultTenantName } }));

            var tokenStub = new Mock<IToken>();
            tokenStub.Setup(p => p.SerializeAsync())
                .ReturnsAsync(AuthorizationToken);

            var tokenProviderStub = new Mock<ITokenProvider>();
            tokenProviderStub.SetupGet(p => p.ConfigurationType)
                .Returns(typeof(JwtConfiguration));
            tokenProviderStub.SetupGet(p => p.TokenType)
                .Returns("jwt");
            tokenProviderStub.Setup(p => p.CreateAsync(It.IsAny<JwtConfiguration>()))
                .ReturnsAsync(tokenStub.Object);

            var instance = new AuthorizationTokenFlowService(identityServiceStup.Object, oAuthClientServiceStub.Object, scopeServiceStub.Object, new[] { tokenProviderStub.Object });
            var identity = GetIdentity();

            var result = await instance.AuthorizeRequest(request, identity);

            Assert.IsType<SuccessfulTokenAuthorizationResult>(result);
            Assert.Equal(AuthorizationToken, (result.Response as AuthorizationTokenResponse)?.Token);
        }

        [Fact]
        public async Task AuthorizeRequest_ValidRequestWithScope_ReturnsResponse()
        {
            const string AuthorizationToken = "AuthorizationToken";
            const string ClientIdentifier = "6D5CD2A0-59D0-47BD-86A1-BF1E600935C3";
            const string KnownScope = "knownScope";

            var request = new AuthorizationRequestBuilder().WithClientId(ClientIdentifier)
                                                           .WithResponseType(Identity.OAuth.Constants.ResponseType.Token)
                                                           .WithScope(KnownScope)
                                                           .Build();

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.Is<string>(v => v == Identity.OAuth.Constants.ResponseType.Token)))
                             .Returns(true);

            var clientRegistrationStub = new Mock<IClientRegistration>();
            clientRegistrationStub.SetupGet(p => p.ClientId)
                                  .Returns(ClientIdentifier);
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            var oAuthClientServiceStub = new Mock<IClientService>();
            oAuthClientServiceStub.Setup(p => p.GetById(It.Is<string>(x => x == ClientIdentifier)))
                                  .ReturnsAsync(clientRegistrationStub.Object);

            var scopeStub = new Mock<IScope>();
            scopeStub.SetupGet(p => p.ScopeKey)
                     .Returns(KnownScope);

            var scopeServiceStub = new Mock<IScopeService>();
            scopeServiceStub.Setup(p => p.GetScopes())
                            .ReturnsAsync(new List<IScope> { scopeStub.Object });

            var tokenStub = new Mock<IToken>();
            tokenStub.Setup(p => p.SerializeAsync())
                .ReturnsAsync(AuthorizationToken);

            var oauthIdentityServiceStup = new Mock<IIdentityService>();

            oauthIdentityServiceStup.Setup(p => p.GetIdentityAsync(It.IsAny<ClaimsIdentity>()))
              .ReturnsAsync(new IdentityData(Constants.DefaultAdminUserId, Constants.DefaultAdminUserName, new[] { new TenantInfo { Key = Constants.DefaultTenantId, Name = Constants.DefaultTenantName } }));

            var tokenProviderStub = new Mock<ITokenProvider>();
            tokenProviderStub.SetupGet(p => p.TokenType).Returns("jwt");
            tokenProviderStub.SetupGet(p => p.ConfigurationType).Returns(typeof(JwtConfiguration));
            tokenProviderStub.Setup(p => p.CreateAsync(It.IsAny<object>()))
                .ReturnsAsync(tokenStub.Object);

            var instance = new AuthorizationTokenFlowService(oauthIdentityServiceStup.Object, oAuthClientServiceStub.Object, scopeServiceStub.Object, new[] { tokenProviderStub.Object });

            var identity = GetIdentity();

            var result = await instance.AuthorizeRequest(request, identity);

            Assert.IsType<SuccessfulTokenAuthorizationResult>(result);
            Assert.Equal(AuthorizationToken, (result.Response as AuthorizationTokenResponse)?.Token);
        }

        private static ClaimsIdentity GetIdentity()
        {
            return new IdentityData(Constants.DefaultAdminUserId, Constants.DefaultAdminUserName, new[] { new TenantInfo { Key = Constants.DefaultTenantId, Name = Constants.DefaultTenantName } }).ToClaimsPrincipal().Identity as ClaimsIdentity;
        }
    }
}