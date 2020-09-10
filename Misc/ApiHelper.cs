using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Api helpers.
    /// </summary>
    internal class ApiHelper
    {
        /// <summary>
        /// OpenApiSecurityRequirement for OpenApiOperation
        /// </summary>
        /// <value>OpenApiSecurityRequirement</value>
        internal static IList<OpenApiSecurityRequirement> Requirements { get; set; }

        internal static string Authority { get; set; }

        internal static string Audience { get; set; }

        internal static string DelegationBaseUri { get; set; }

        internal static string DelegationClientId { get; set; }

        internal static string DelegationClientSecret { get; set; }

        internal static string DelegationScope { get; set; }

        internal static string OssBaseUri { get; set; }

        internal static string OssUser { get; set; }

        internal static string OssPassword { get; set; }

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
        /// Retrieve name of the user executing the request.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <returns>User name.</returns>
        internal static string GetUserName(ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "name")
                .Value;
        }

        /// <summary>
        /// Retrieve role of the user executing the request.
        /// </summary>
        /// <param name="user">User info.</param>
        /// <returns>User role.</returns>
        internal static string GetUserRole(ClaimsPrincipal user)
        {
            return user.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.Role || c.Type == "role")?
                .Value;
        }

        /// <summary>
        /// Read configuration data.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        internal static void ReadConfiguration(IConfiguration configuration)
        {
            IConfigurationSection bearerConfiguration = configuration.GetSection("Bearer");
            Authority = bearerConfiguration.GetValue<string>("Authority");
            Audience = bearerConfiguration.GetValue<string>("Audience");

            IConfigurationSection delegationConfiguration = configuration.GetSection("Delegation");
            DelegationBaseUri = delegationConfiguration.GetValue<string>("BaseUri");
            DelegationClientId = delegationConfiguration.GetValue<string>("ClientId");
            DelegationClientSecret = delegationConfiguration.GetValue<string>("ClientSecret");
            DelegationScope = delegationConfiguration.GetValue<string>("Scope");

            IConfigurationSection ossConfiguration = configuration.GetSection("OssApi");
            OssBaseUri = ossConfiguration.GetValue<string>("BaseUri");
            OssUser = ossConfiguration.GetValue<string>("User");
            OssPassword = ossConfiguration.GetValue<string>("Password");
        }

        /// <summary>
        /// Initialize OpenApiSecurityRequirement for OpenApiOperation
        /// </summary>
        internal static void InitializeRequirements()
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
                            Audience
                        }
                    }
                }
            };
        }
    }
}