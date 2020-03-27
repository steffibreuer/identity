﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading.Tasks;
using Codeworx.Identity.AspNetCore.OAuth;
using Codeworx.Identity.Cache;
using Codeworx.Identity.Model;
using Codeworx.Identity.OAuth;
using Codeworx.Identity.OAuth.Token;
using Moq;
using Xunit;

namespace Codeworx.Identity.Test.AspNetCore.OAuth
{
    public class TokenServiceTests
    {
        [Fact]
        public async Task AuthorizeRequest_InvalidClient_ReturnsError()
        {
            var request = new AuthorizationCodeTokenRequestBuilder().Build();

            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();

            var tokenFlowServiceStub = new Mock<ITokenResultService>();
            tokenFlowServiceStub.SetupGet(p => p.SupportedGrantType)
                                .Returns(request.GrantType);

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();
            clientAuthenticationStub.Setup(p => p.AuthenticateClient(It.IsAny<TokenRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                                           .ReturnsAsync(new AuthenticateClientResult { TokenResult = new InvalidRequestResult(), ClientRegistration = null });

            var instance = new TokenService(null,new List<ITokenResultService> { tokenFlowServiceStub.Object }, requestValidatorStub.Object, clientAuthenticationStub.Object);

            var result = await instance.AuthorizeRequest(request, null, null, null);

            Assert.IsType<InvalidRequestResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_InvalidClientDueToInvalidRequest_ReturnsError()
        {
            var request = new AuthorizationCodeTokenRequestBuilder().Build();

            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();

            var tokenFlowServiceStub = new Mock<ITokenResultService>();
            tokenFlowServiceStub.SetupGet(p => p.SupportedGrantType)
                                .Returns(request.GrantType);

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();
            clientAuthenticationStub.Setup(p => p.AuthenticateClient(It.IsAny<TokenRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .ReturnsAsync(new AuthenticateClientResult { TokenResult = new InvalidRequestResult(), ClientRegistration = null });

            var instance = new TokenService(null,new List<ITokenResultService> { tokenFlowServiceStub.Object }, requestValidatorStub.Object, clientAuthenticationStub.Object);

            var result = await instance.AuthorizeRequest(request, null, null, null);

            Assert.IsType<InvalidRequestResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_InvalidRequest_ReturnsError()
        {
            var validationResultStub = new Mock<IValidationResult<TokenErrorResponse>>();
            validationResultStub.SetupGet(p => p.Error)
                                .Returns(new TokenErrorResponse(string.Empty, string.Empty, string.Empty));

            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();
            requestValidatorStub.Setup(p => p.IsValid(It.IsAny<TokenRequest>()))
                                .ReturnsAsync(validationResultStub.Object);

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();

            var instance = new TokenService(null,new List<ITokenResultService>(), requestValidatorStub.Object, clientAuthenticationStub.Object);

            var request = new AuthorizationCodeTokenRequestBuilder().Build();

            var result = await instance.AuthorizeRequest(request, null, null, null);

            Assert.IsType<InvalidRequestResult>(result);
        }

        [Fact(Skip = "Implement")]
        public async Task AuthorizeRequest_InvalidScope_ReturnsError()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task AuthorizeRequest_RequestNull_ThrowsException()
        {
            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();

            var instance = new TokenService(null,new List<ITokenResultService>(), requestValidatorStub.Object, clientAuthenticationStub.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => instance.AuthorizeRequest(null, null, null, null));
        }

        [Fact]
        public async Task AuthorizeRequest_UnauthorizedClient_ReturnsError()
        {
            var request = new AuthorizationCodeTokenRequestBuilder().WithGrantType("unAuthorized")
                                                                    .Build();

            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();

            var tokenFlowServiceStub = new Mock<ITokenResultService>();
            tokenFlowServiceStub.SetupGet(p => p.SupportedGrantType)
                                .Returns(request.GrantType);

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.Is<string>(v => v == "Authorized")))
                             .Returns(true);

            var clientRegistrationStub = new Mock<IClientRegistration>();
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();
            clientAuthenticationStub.Setup(p => p.AuthenticateClient(It.IsAny<TokenRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .ReturnsAsync(new AuthenticateClientResult { ClientRegistration = clientRegistrationStub.Object });

            var instance = new TokenService(null, new List<ITokenResultService> { tokenFlowServiceStub.Object }, requestValidatorStub.Object, clientAuthenticationStub.Object);

            var result = await instance.AuthorizeRequest(request, null, null, null);

            Assert.IsType<UnauthorizedClientResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_UnsupportedGrantType_ReturnsError()
        {
            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();

            var instance = new TokenService(null, new List<ITokenResultService>(), requestValidatorStub.Object, clientAuthenticationStub.Object);

            var request = new AuthorizationCodeTokenRequestBuilder().WithGrantType("Unsupported")
                                                                    .Build();

            var result = await instance.AuthorizeRequest(request, null, null, null);

            Assert.IsType<UnsupportedGrantTypeResult>(result);
        }

        [Fact]
        public async Task AuthorizeRequest_ValidRequest_SuccessReturned()
        {
            var expectedToken = "AccessToken";
            var expectedCache = new Dictionary<string, string>
            {
                {"abc", "def"}
            };

            var expectedUser = new ClaimsIdentity();

            var request = new AuthorizationCodeTokenRequestBuilder().Build();

            var cacheMock = new Mock<IAuthorizationCodeCache>();
            cacheMock.Setup(p => p.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(expectedCache);

            var requestValidatorStub = new Mock<IRequestValidator<TokenRequest, TokenErrorResponse>>();

            var tokenFlowServiceStub = new Mock<ITokenResultService>();
            tokenFlowServiceStub.SetupGet(p => p.SupportedGrantType)
                                .Returns(request.GrantType);
            tokenFlowServiceStub.Setup(p => p.CreateAccessToken(It.IsAny<Dictionary<string, string>>(),It.IsAny<ClaimsIdentity>()))
                                .ReturnsAsync(expectedToken);

            var supportedFlowStub = new Mock<ISupportedFlow>();
            supportedFlowStub.Setup(p => p.IsSupported(It.Is<string>(v => v == request.GrantType)))
                             .Returns(true);

            var clientRegistrationStub = new Mock<IClientRegistration>();
            clientRegistrationStub.SetupGet(p => p.SupportedFlow)
                                  .Returns(ImmutableList.Create(supportedFlowStub.Object));

            var clientAuthenticationStub = new Mock<IClientAuthenticationService>();
            clientAuthenticationStub.Setup(p => p.AuthenticateClient(It.IsAny<TokenRequest>(), It.IsAny<string>(), It.IsAny<string>()))
                                    .ReturnsAsync(new AuthenticateClientResult { ClientRegistration = clientRegistrationStub.Object });

            var instance = new TokenService(cacheMock.Object,new List<ITokenResultService> { tokenFlowServiceStub.Object }, requestValidatorStub.Object, clientAuthenticationStub.Object);

            var result = await instance.AuthorizeRequest(request, null, null, expectedUser);

            Assert.IsType<SuccessfulTokenResult>(result);
            requestValidatorStub.Verify(p=> p.IsValid(It.IsAny<TokenRequest>()),Times.Once);
            clientAuthenticationStub.Verify(p => p.AuthenticateClient(It.IsAny<TokenRequest>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            supportedFlowStub.Verify(p => p.IsSupported(It.Is<string>(v => v == request.GrantType)), Times.Once);
            tokenFlowServiceStub.Verify(p => p.CreateAccessToken(expectedCache, expectedUser), Times.Once);
            Assert.Equal(expectedToken, result.Response.AccessToken);
        }
    }
}