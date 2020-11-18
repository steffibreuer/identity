﻿using System;
using System.Threading.Tasks;
using Codeworx.Identity.Model;

namespace Codeworx.Identity.Login
{
    public class FormsLoginProcessor : ILoginProcessor
    {
        private readonly IIdentityService _identityService;

        public FormsLoginProcessor(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public Type RequestParameterType { get; } = typeof(LoginFormRequest);

        public string Template => Constants.Templates.FormsLogin;

        public Task<ILoginRegistrationInfo> GetRegistrationInfoAsync(ProviderRequest request, ILoginRegistration configuration)
        {
            string error = null;
            request.ProviderErrors.TryGetValue(configuration.Id, out error);

            return Task.FromResult<ILoginRegistrationInfo>(new FormsLoginRegistrationInfo(configuration.Id, request.UserName, error));
        }

        public async Task<SignInResponse> ProcessAsync(ILoginRegistration registration, object request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var loginRequest = ToLoginFormRequest(request);

            var returnUrl = loginRequest.ReturnUrl;

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                returnUrl = null;
            }

            var identity = await _identityService.LoginAsync(loginRequest.UserName, loginRequest.Password).ConfigureAwait(false);

            return new SignInResponse(identity, returnUrl);
        }

        private LoginFormRequest ToLoginFormRequest(object request)
        {
            var loginRequest = request as LoginFormRequest;

            if (loginRequest == null)
            {
                throw new ArgumentException($"The argument ist not of type {RequestParameterType}", nameof(request));
            }

            return loginRequest;
        }
    }
}