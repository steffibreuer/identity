﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Codeworx.Identity.Model;

namespace Codeworx.Identity
{
    public interface IClaimsService
    {
        Task<IEnumerable<AssignedClaim>> GetClaimsAsync(IUser user, string tenantKey = null);
    }
}