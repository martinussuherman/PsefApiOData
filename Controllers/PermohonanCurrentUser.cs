using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsefApiOData.Misc;
using PsefApiOData.Models;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static PsefApiOData.ApiInfo;

namespace PsefApiOData.Controllers
{
    /// <summary>
    /// Represents a RESTful service of Permohonan for current user.
    /// </summary>
    [Authorize]
    [ApiVersion(V0_1)]
    [ODataRoutePrefix(nameof(PermohonanCurrentUser))]
    public class PermohonanCurrentUser : ODataController
    {
        /// <summary>
        /// Permohonan for current user REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="delegateService">Api delegation service.</param>
        /// <param name="identityApi">Identity Api service.</param>
        public PermohonanCurrentUser(
            PsefMySqlContext context,
            IApiDelegateService delegateService,
            IIdentityApiService identityApi)
        {
            _identityApi = identityApi;
            _delegateService = delegateService;
            _context = context;
        }

        /// <summary>
        /// Retrieves all Permohonan for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Permohonan for the current user.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [ODataRoute]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [ProducesResponseType(Status403Forbidden)]
        [EnableQuery]
        public IQueryable<Permohonan> Get()
        {
            return _context.Permohonan.Where(e =>
                e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User));
        }

        /// <summary>
        /// Gets a single Permohonan for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="id">The requested Permohonan identifier.</param>
        /// <returns>The requested Permohonan.</returns>
        /// <response code="200">The Permohonan was successfully retrieved.</response>
        /// <response code="404">The Permohonan does not exist.</response>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Permohonan), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<Permohonan> Get([FromODataUri] uint id)
        {
            return SingleResult.Create(
                _context.Permohonan.Where(e =>
                    e.Id == id &&
                    e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User)));
        }

        /// <summary>
        /// Creates a new Permohonan for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="create">The Permohonan to create.</param>
        /// <returns>The created Permohonan.</returns>
        /// <response code="201">The Permohonan was successfully created.</response>
        /// <response code="204">The Permohonan was successfully created.</response>
        /// <response code="400">The Permohonan is invalid.</response>
        /// <response code="409">The Permohonan with supplied id already exist.</response>
        [ODataRoute]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Permohonan), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] Permohonan create)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Pemohon pemohon = await _context.Pemohon
                .FirstOrDefaultAsync(c =>
                    c.UserId == ApiHelper.GetUserId(HttpContext.User));

            if (pemohon == null)
            {
                return BadRequest();
            }

            create.PemohonId = pemohon.Id;
            _context.Permohonan.Add(create);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (Exists(create.Id))
                {
                    return Conflict();
                }

                throw;
            }

            return Created(create);
        }

        /// <summary>
        /// Updates an existing Permohonan for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="id">The requested Permohonan identifier.</param>
        /// <param name="delta">The partial Permohonan to update.</param>
        /// <returns>The updated Permohonan.</returns>
        /// <response code="200">The Permohonan was successfully updated.</response>
        /// <response code="204">The Permohonan was successfully updated.</response>
        /// <response code="400">The Permohonan is invalid.</response>
        /// <response code="404">The Permohonan does not exist.</response>
        /// <response code="422">The Permohonan identifier is specified on delta and its value is different from id.</response>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Permohonan), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status401Unauthorized)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status422UnprocessableEntity)]
        public async Task<IActionResult> PatchCurrentUser(
            [FromODataUri] uint id,
            [FromBody] Delta<Permohonan> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string currentUserId = ApiHelper.GetUserId(HttpContext.User);
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    c.Pemohon.UserId == currentUserId);

            if (update == null)
            {
                return NotFound();
            }

            var oldId = update.Id;
            var oldPemohonId = update.PemohonId;
            delta.Patch(update);

            if (update.PemohonId != oldPemohonId)
            {
                return Unauthorized(update.PemohonId);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
                if (update.Id != oldId)
                {
                    ModelState.AddModelError(nameof(update.Id), DontSetKeyOnPatch);
                    return UnprocessableEntity(ModelState);
                }

                throw;
            }

            return Updated(update);
        }

        private bool Exists(uint id)
        {
            return _context.Permohonan.Any(e => e.Id == id);
        }

        private readonly PsefMySqlContext _context;
        private readonly IApiDelegateService _delegateService;
        private readonly IIdentityApiService _identityApi;
    }
}