﻿namespace Codeworx.Identity.OAuth.Binding.Authorization
{
    public class ScopeDuplicatedResult : IRequestBindingResult<AuthorizationRequest, AuthorizationErrorResponse>
    {
        public ScopeDuplicatedResult(string state)
        {
            this.Error = new AuthorizationErrorResponse(Constants.Error.InvalidRequest, Constants.ScopeName, null, state);
        }

        public AuthorizationRequest Result => null;

        public AuthorizationErrorResponse Error { get; }
    }
}