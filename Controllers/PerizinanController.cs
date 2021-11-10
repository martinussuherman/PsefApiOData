using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
    /// Represents a RESTful service of Perizinan.
    /// </summary>
    [Authorize]
    [ApiVersion(V0_1)]
    [ODataRoutePrefix(nameof(Perizinan))]
    public class PerizinanController : ODataController
    {
        /// <summary>
        /// Perizinan REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="mapper"></param>
        public PerizinanController(PsefMySqlContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves all Perizinan.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Perizinan.</returns>
        /// <response code="200">Perizinan successfully retrieved.</response>
        [ODataRoute]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PerizinanView>>), Status200OK)]
        [EnableQuery]
        public IQueryable<PerizinanView> Get()
        {
            if (string.IsNullOrEmpty(ApiHelper.GetUserRole(HttpContext.User)))
            {
                return _context.Perizinan
                    .AsNoTracking()
                    .Where(e =>
                        e.Permohonan.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                        e.IssuedAt != _invalidPerizinan)
                    .ProjectTo<PerizinanView>(_mapper.ConfigurationProvider);
            }

            return _context.Perizinan
                .AsNoTracking()
                .Where(e => e.IssuedAt != _invalidPerizinan)
                .ProjectTo<PerizinanView>(_mapper.ConfigurationProvider);
        }

        /// <summary>
        /// Gets a single Perizinan.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="id">The requested Perizinan identifier.</param>
        /// <returns>The requested Perizinan.</returns>
        /// <response code="200">The Perizinan was successfully retrieved.</response>
        /// <response code="404">The Perizinan does not exist.</response>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(PerizinanView), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<PerizinanView> Get([FromODataUri] uint id)
        {
            if (string.IsNullOrEmpty(ApiHelper.GetUserRole(HttpContext.User)))
            {
                return SingleResult.Create(
                    _context.Perizinan
                        .Where(e =>
                            e.Id == id &&
                            e.Permohonan.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                            e.IssuedAt != _invalidPerizinan)
                        .ProjectTo<PerizinanView>(_mapper.ConfigurationProvider));
            }

            return SingleResult.Create(
                _context.Perizinan
                    .Where(e =>
                        e.Id == id &&
                        e.IssuedAt != _invalidPerizinan)
                    .ProjectTo<PerizinanView>(_mapper.ConfigurationProvider));
        }

        /// <summary>
        /// Creates a new Perizinan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="create">The Perizinan to create.</param>
        /// <returns>The created Perizinan.</returns>
        /// <response code="201">The Perizinan was successfully created.</response>
        /// <response code="204">The Perizinan was successfully created.</response>
        /// <response code="400">The Perizinan is invalid.</response>
        /// <response code="409">The Perizinan with supplied id already exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Perizinan), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] Perizinan create)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Perizinan.Add(create);

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
        /// Updates an existing Perizinan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The requested Perizinan identifier.</param>
        /// <param name="delta">The partial Perizinan to update.</param>
        /// <returns>The updated Perizinan.</returns>
        /// <response code="200">The Perizinan was successfully updated.</response>
        /// <response code="204">The Perizinan was successfully updated.</response>
        /// <response code="400">The Perizinan is invalid.</response>
        /// <response code="404">The Perizinan does not exist.</response>
        /// <response code="422">The Perizinan identifier is specified on delta and its value is different from id.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Perizinan), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status422UnprocessableEntity)]
        public async Task<IActionResult> Patch(
            [FromODataUri] uint id,
            [FromBody] Delta<Perizinan> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var update = await _context.Perizinan.FindAsync(id);

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
        /// Deletes a Perizinan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The Perizinan to delete.</param>
        /// <returns>None</returns>
        /// <response code="204">The Perizinan was successfully deleted.</response>
        /// <response code="404">The Perizinan does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Delete([FromODataUri] uint id)
        {
            var delete = await _context.Perizinan.FindAsync(id);

            if (delete == null)
            {
                return NotFound();
            }

            _context.Perizinan.Remove(delete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Updates an existing Perizinan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The requested Perizinan identifier.</param>
        /// <param name="update">The Perizinan to update.</param>
        /// <returns>The updated Perizinan.</returns>
        /// <response code="200">The Perizinan was successfully updated.</response>
        /// <response code="204">The Perizinan was successfully updated.</response>
        /// <response code="400">The Perizinan is invalid.</response>
        /// <response code="404">The Perizinan does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Perizinan), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Put(
            [FromODataUri] uint id,
            [FromBody] Perizinan update)
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

        /// <summary>
        /// Retrieves list of Perizinan Halaman Muka information.
        /// </summary>
        /// <remarks>
        /// *Anonymous Access*
        /// </remarks>
        /// <returns>All available Perizinan Halaman Muka information.</returns>
        /// <response code="200">Perizinan Halaman Muka successfully retrieved.</response>
        [AllowAnonymous]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PerizinanHalamanMuka>>), Status200OK)]
        public async Task<IEnumerable<PerizinanHalamanMuka>> HalamanMuka()
        {
            return await _context.Perizinan
                .Where(c => c.IssuedAt != _invalidPerizinan)
                .OrderByDescending(c => c.IssuedAt)
                .Take(20)
                .ProjectTo<PerizinanHalamanMuka>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        private bool Exists(uint id)
        {
            return _context.Perizinan.Any(e => e.Id == id);
        }

        private readonly DateTime _invalidPerizinan = new DateTime(1901, 1, 1);
        private readonly PsefMySqlContext _context;
        private readonly IMapper _mapper;
    }
}