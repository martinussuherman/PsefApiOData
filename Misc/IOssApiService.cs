using System.Threading.Tasks;
using Microsoft.IdentityModel.JsonWebTokens;
using Newtonsoft.Json.Linq;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// OSS (Online Single Submission) API service interface.
    /// </summary>
    public interface IOssApiService
    {
        /// <summary>
        /// Authenticate to OSS Api.
        /// </summary>
        /// <returns>Token retrieved from the OSS Api authentication endpoint.</returns>
        Task<JsonWebToken> Authenticate();

        /// <summary>
        /// Call OSS Api.
        /// </summary>
        /// <param name="token">Access token for the OSS Api.</param>
        /// <param name="uri">Uri of Api request.</param>
        /// <param name="content">Content of Api request.</param>
        /// <returns>JObject retrieved from the OSS Api.</returns>
        Task<JObject> CallApiAsync(JsonWebToken token, string uri, string content);
    }
}