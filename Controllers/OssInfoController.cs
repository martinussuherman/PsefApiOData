using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public OssInfoController()
        {
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
        /// <example>OssInfo('123456789')</example>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(OssInfo), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<OssInfo> Get([FromODataUri] string id)
        {
            return SingleResult.Create(dummy.Where(e => e.Nib == id).AsQueryable());
        }

        private bool Exists(string id)
        {
            return dummy.Any(e => e.Nib == id);
        }

        private static readonly List<OssInfo> dummy = new List<OssInfo>
        {
            new OssInfo
            {
                Nib = "123456789",
                Name = "Dummy OSS",
                Address = "Alamat dummy",
                Npwp = "0123456789",
                Siup = "123456789",
                Director = "Badu",
                CapitalSourceType = 0,
                CompanyType = 0,
                LegalEntityType = 0
            },
            new OssInfo
            {
                Nib = "987654321",
                Name = "OSS Dummy ",
                Address = "Alamat dummy",
                Npwp = "0000000000",
                Siup = "111111111",
                Director = "Budi",
                CapitalSourceType = 1,
                CompanyType = 1,
                LegalEntityType = 1
            }
        };
    }
}
