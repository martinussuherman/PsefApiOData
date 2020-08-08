using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Api delegation service.
    /// </summary>
    public class ApiDelegateService : IApiDelegateService
    {
        /// <summary>
        /// Api delegation service.
        /// </summary>
        /// <param name="httpClient">Http client to connect to IdentityServer.</param>
        public ApiDelegateService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Retrieve delegation token for calling another API from this API.
        /// </summary>
        /// <param name="userToken">User token to pass to IdentityServer.</param>
        /// <returns>Access token for the other API.</returns>
        public async Task<TokenResponse> DelegateAsync(string userToken)
        {
            var disco = await _httpClient.GetDiscoveryDocumentAsync(
                new DiscoveryDocumentRequest
                {
                    Address = ApiHelper.Authority,
                    Policy =
                    {
                        ValidateIssuerName = false,
                        ValidateEndpoints = false,
                    },
                }
            );

            // send custom grant to token endpoint, return response
            return await _httpClient.RequestTokenAsync(new TokenRequest
            {
                Address = disco.TokenEndpoint,
                GrantType = "delegation",

                ClientId = ApiHelper.DelegationClientId,
                ClientSecret = ApiHelper.DelegationClientSecret,

                Parameters =
                {
                    { "scope", ApiHelper.DelegationScope },
                    { "token", userToken}
                }
            });
        }

        private readonly HttpClient _httpClient;
    }
}