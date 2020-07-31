using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

namespace PsefApi.Misc
{
    /// <summary>
    /// Api helpers.
    /// </summary>
    internal static class ApiHelper
    {
        /// <summary>
        /// Retrieve id of the user executing the request.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <returns>User Id.</returns>
        internal static string GetUserId(ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                .Value;
        }

        /// <summary>
        /// OpenApiSecurityRequirement for OpenApiOperation
        /// </summary>
        /// <value>OpenApiSecurityRequirement</value>
        internal static IList<OpenApiSecurityRequirement> Requirements { get; set; }

        /// <summary>
        /// Initialize OpenApiSecurityRequirement for OpenApiOperation
        /// </summary>
        /// <param name="scope">Api scope</param>
        internal static void InitializeRequirements(string scope)
        {
            Requirements = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = ApiInfo.SchemeOauth2
                            }
                        },
                        new List<string>
                        {
                            scope
                        }
                    }
                }
            };
        }
    }
}