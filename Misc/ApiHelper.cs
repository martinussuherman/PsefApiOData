using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;

namespace PsefApi.Misc
{
    /// <summary>
    /// Api helpers.
    /// </summary>
    internal class ApiHelper
    {
        /// <summary>
        /// Retrieve delegation token for calling another API from this API.
        /// </summary>
        /// <param name="userToken">User token to pass to IdentityServer.</param>
        /// <returns>Access token for the other API.</returns>
        internal async Task<TokenResponse> DelegateAsync(string userToken)
        {
            // var client = _httpClientFactory.CreateClient();
            // or
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync(Authority);

            // send custom grant to token endpoint, return response
            return await client.RequestTokenAsync(new TokenRequest
            {
                Address = disco.TokenEndpoint,
                GrantType = "delegation",

                ClientId = DelegationClientId,
                ClientSecret = DelegationClientSecret,

                Parameters =
                {
                    { "scope", DelegationScope },
                    { "token", userToken}
                }
            });
        }

        /// <summary>
        /// OpenApiSecurityRequirement for OpenApiOperation
        /// </summary>
        /// <value>OpenApiSecurityRequirement</value>
        internal static IList<OpenApiSecurityRequirement> Requirements { get; set; }

        internal static string Authority { get; set; }

        internal static string Audience { get; set; }

        internal static string DelegationClientId { get; set; }

        internal static string DelegationClientSecret { get; set; }

        internal static string DelegationScope { get; set; }

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
        /// Read configuration data.
        /// </summary>
        /// <param name="configuration">Application configuration.</param>
        internal static void ReadConfiguration(IConfiguration configuration)
        {
            IConfigurationSection bearerConfiguration = configuration.GetSection("Bearer");
            Authority = bearerConfiguration.GetValue<string>("Authority");
            Audience = bearerConfiguration.GetValue<string>("Audience");

            IConfigurationSection delegationConfiguration = configuration.GetSection("Delegation");
            DelegationClientId = delegationConfiguration.GetValue<string>("ClientId");
            DelegationClientSecret = delegationConfiguration.GetValue<string>("ClientSecret");
            DelegationScope = delegationConfiguration.GetValue<string>("Scope");
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