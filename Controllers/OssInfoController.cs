using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PsefApiOData.Misc;
using PsefApiOData.Models;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static PsefApiOData.ApiInfo;

namespace PsefApiOData.Controllers
{
    /// <summary>
    /// Represents a RESTful service of OSS Information.
    /// </summary>
    [Authorize]
    [ApiVersion(V0_1)]
    [ODataRoutePrefix(nameof(OssInfo))]
    public class OssInfoController : ODataController
    {
        /// <summary>
        /// OSS Information REST service.
        /// </summary>
        /// <param name="ossApi">OSS Api service.</param>
        /// <param name="memoryCache">Memory cache.</param>
        public OssInfoController(IOssApiService ossApi, IMemoryCache memoryCache)
        {
            _ossApi = ossApi;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets a single OSS Information.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="id">The requested OSS Information identifier.</param>
        /// <returns>The requested OSS Information.</returns>
        /// <response code="200">The OSS Information was successfully retrieved.</response>
        /// <response code="404">The OSS Information does not exist.</response>
        /// <example>OssInfo('0000000000000')</example>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(OssInfo), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public async Task<SingleResult<OssInfo>> Get([FromODataUri] string id)
        {
            OssInfoHelper helper = new OssInfoHelper(_ossApi, _memoryCache);
            OssFullInfo fullInfo = await helper.RetrieveInfo(id);

            OssInfo info = new OssInfo
            {
                Nib = fullInfo.Nib,
                Address = fullInfo.AlamatPerseroan,
                Name = fullInfo.NamaPerseroan,
                Npwp = fullInfo.NpwpPerseroan,
                Director = fullInfo.NamaUserProses
            };

            List<OssInfo> result = new List<OssInfo>
            {
                info
            };

            return SingleResult.Create(result.AsQueryable());
        }

        /// <summary>
        /// Gets a single OSS Full Information.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="id">The requested OSS Full Information identifier.</param>
        /// <returns>The requested OSS Full Information.</returns>
        /// <response code="200">The OSS Full Information was successfully retrieved.</response>
        /// <response code="404">The OSS Full Information does not exist.</response>
        /// <example>OssFullInfo('0000000000000')</example>
        [HttpGet]
        [ODataRoute(nameof(OssFullInfo))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(OssFullInfo), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status503ServiceUnavailable)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public async Task<SingleResult<OssFullInfo>> OssFullInfo([FromQuery] string id)
        {
            OssInfoHelper helper = new OssInfoHelper(_ossApi, _memoryCache);
            List<OssFullInfo> list = new List<OssFullInfo>
            {
                await helper.RetrieveInfo(id)
            };

            return SingleResult.Create(list.AsQueryable());
        }

        private readonly IOssApiService _ossApi;
        private readonly IMemoryCache _memoryCache;
    }
}
