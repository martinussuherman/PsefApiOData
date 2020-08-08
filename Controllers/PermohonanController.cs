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
    /// Represents a RESTful service of Permohonan.
    /// </summary>
    [Authorize]
    [ApiVersion(V0_1)]
    [ODataRoutePrefix(nameof(Permohonan))]
    public class PermohonanController : ODataController
    {
        /// <summary>
        /// Permohonan REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="delegateService">Api delegation service.</param>
        /// <param name="identityApi">Identity Api service.</param>
        public PermohonanController(
            PsefMySqlContext context,
            IApiDelegateService delegateService,
            IIdentityApiService identityApi)
        {
            _identityApi = identityApi;
            _delegateService = delegateService;
            _context = context;
        }

        /// <summary>
        /// Retrieves Permohonan total count.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>Permohonan total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [HttpGet]
        [ODataRoute(nameof(TotalCount))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> TotalCount()
        {
            return await _context.Permohonan.LongCountAsync();
        }

        /// <summary>
        /// Retrieves all Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <returns>All available Permohonan.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(
            ApiRole.Verifikator,
            ApiRole.Kasi,
            ApiRole.Kasubdit,
            ApiRole.Diryanfar,
            ApiRole.Dirjen,
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [ProducesResponseType(Status403Forbidden)]
        [EnableQuery]
        public IQueryable<Permohonan> Get()
        {
            return _context.Permohonan;
        }

        /// <summary>
        /// Gets a single Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <param name="id">The requested Permohonan identifier.</param>
        /// <returns>The requested Permohonan.</returns>
        /// <response code="200">The Permohonan was successfully retrieved.</response>
        /// <response code="404">The Permohonan does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Verifikator,
            ApiRole.Kasi,
            ApiRole.Kasubdit,
            ApiRole.Diryanfar,
            ApiRole.Dirjen,
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Permohonan), Status200OK)]
        [ProducesResponseType(Status403Forbidden)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<Permohonan> Get([FromODataUri] uint id)
        {
            return SingleResult.Create(
                _context.Permohonan.Where(e => e.Id == id));
        }

        /// <summary>
        /// Creates a new Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="create">The Permohonan to create.</param>
        /// <returns>The created Permohonan.</returns>
        /// <response code="201">The Permohonan was successfully created.</response>
        /// <response code="204">The Permohonan was successfully created.</response>
        /// <response code="400">The Permohonan is invalid.</response>
        /// <response code="409">The Permohonan with supplied id already exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
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
        /// Updates an existing Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The requested Permohonan identifier.</param>
        /// <param name="delta">The partial Permohonan to update.</param>
        /// <returns>The updated Permohonan.</returns>
        /// <response code="200">The Permohonan was successfully updated.</response>
        /// <response code="204">The Permohonan was successfully updated.</response>
        /// <response code="400">The Permohonan is invalid.</response>
        /// <response code="404">The Permohonan does not exist.</response>
        /// <response code="422">The Permohonan identifier is specified on delta and its value is different from id.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Permohonan), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status422UnprocessableEntity)]
        public async Task<IActionResult> Patch(
            [FromODataUri] uint id,
            [FromBody] Delta<Permohonan> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var update = await _context.Permohonan.FindAsync(id);

            if (update == null)
            {
                return NotFound();
            }

            delta.Patch(update);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException)
            {
                if (update.Id != id)
                {
                    ModelState.AddModelError(nameof(update.Id), DontSetKeyOnPatch);
                    return UnprocessableEntity(ModelState);
                }

                throw;
            }

            return Updated(update);
        }

        /// <summary>
        /// Deletes a Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The Permohonan to delete.</param>
        /// <returns>None</returns>
        /// <response code="204">The Permohonan was successfully deleted.</response>
        /// <response code="404">The Permohonan does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Delete([FromODataUri] uint id)
        {
            var delete = await _context.Permohonan.FindAsync(id);

            if (delete == null)
            {
                return NotFound();
            }

            _context.Permohonan.Remove(delete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Updates an existing Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The requested Permohonan identifier.</param>
        /// <param name="update">The Permohonan to update.</param>
        /// <returns>The updated Permohonan.</returns>
        /// <response code="200">The Permohonan was successfully updated.</response>
        /// <response code="204">The Permohonan was successfully updated.</response>
        /// <response code="400">The Permohonan is invalid.</response>
        /// <response code="404">The Permohonan does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Permohonan), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Put(
            [FromODataUri] uint id,
            [FromBody] Permohonan update)
        {
            if (id != update.Id)
            {
                return BadRequest();
            }

            _context.Entry(update).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id))
                {
                    return NotFound();
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
