﻿using Modix.Data.Models.Core;

namespace Modix.WebServer.Models
{
    public class RoleClaimModifyData
    {
        public AuthorizationClaim Claim { get; set; }
        public ClaimMappingType? MappingType { get; set; }
        public ulong RoleId { get; set; }
    }
}
