using System.Security.Claims;

namespace Aloha.Security.Authorizations
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the user ID from a ClaimsPrincipal (from the 'sub' claim in Keycloak tokens)
        /// </summary>
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal), "ClaimsPrincipal cannot be null");

            return principal?.FindFirstValue("userId")
                ?? principal?.FindFirstValue("sub")
                ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new InvalidOperationException("User ID not found in token");
        }


        /// <summary>
        /// Checks if the user has a specific role
        /// </summary>
        public static bool HasRole(this ClaimsPrincipal principal, string roleName)
        {
            return principal.Claims
                .Where(c => c.Type == "roles" || c.Type == ClaimTypes.Role)
                .Any(c => c.Value.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all roles assigned to the user
        /// </summary>
        public static string[] GetRoles(this ClaimsPrincipal principal)
        {
            return principal.Claims
                .Where(c => c.Type == "roles" || c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToArray();
        }

        /// <summary>
        /// Gets a specific claim value by type
        /// </summary>
        public static string GetClaimValue(this ClaimsPrincipal principal, string claimType)
        {
            return principal.FindFirst(claimType)?.Value;
        }

        // get UserName from claims
        public static string GetUserName(this ClaimsPrincipal principal)
        {
            return principal?.FindFirstValue("userName")
                ?? principal?.FindFirstValue("preferred_username")
                ?? principal?.FindFirstValue(ClaimTypes.Name)
                ?? throw new InvalidOperationException("Username not found in token");
        }
    }
}