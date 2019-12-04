using System.Collections.Generic;
using System.Security.Claims;
using ent.manager.WebApi.Results;

namespace ent.manager.WebApi.Helpers
{
    public static class RoleHelper
    {
        public static GetTokenDetailsResult GetTokenDetails(this IEnumerable<Claim> claims)
        {
            GetTokenDetailsResult result = new GetTokenDetailsResult() {  Role="" , Username=""};

            var claimsEnumerator = claims.GetEnumerator();

            while (claimsEnumerator.MoveNext())
            {
                var claim = claimsEnumerator.Current;
                if(claim.Type.Contains(@"identity/claims/name"))
                {
                    result.Username = claim.Value;
                }

                if (claim.Type.Contains(@"identity/claims/role"))
                {
                    result.Role = claim.Value;
                }
            }

            return result;
        }
    }
}
