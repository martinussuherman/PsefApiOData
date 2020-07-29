using System.Linq;
using System.Security.Claims;

namespace PsefApi.Misc
{
    /// <summary>
    /// Api helpers.
    /// </summary>
    public static class ApiHelper
    {
        /// <summary>
        /// Retrieve id of the user executing the request.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <returns>User Id.</returns>
        public static string GetUserId(ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == "sub")
                .Value;
        }
    }
}