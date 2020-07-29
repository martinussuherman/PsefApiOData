using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsefApi.Models;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static PsefApi.ApiInfo;

namespace PsefApi.Controllers
{
    /// <summary>
    /// Represents a RESTful service of Pemohon.
    /// </summary>
    [ApiVersion(V1_0)]
    [ODataRoutePrefix(nameof(Pemohon))]
    public class PemohonController : ODataController
    {
        /// <summary>
        /// Pemohon REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        public PemohonController(PsefMySqlContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all Pemohon.
        /// </summary>
        /// <returns>All available Pemohon.</returns>
        /// <response code="200">Pemohon successfully retrieved.</response>
        [ODataRoute]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Pemohon>>), Status200OK)]
        [EnableQuery(
            PageSize = 50,
            MaxTop = 100,
            AllowedQueryOptions = AllowedQueryOptions.All,
            AllowedFunctions = AllowedFunctions.AllFunctions)]
        public IQueryable<Pemohon> Get()
        {
            return _context.Pemohon;
        }

        /// <summary>
        /// Gets a single Pemohon.
        /// </summary>
        /// <param name="id">The requested Pemohon identifier.</param>
        /// <returns>The requested Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully retrieved.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<Pemohon> Get([FromODataUri] uint id)
        {
            return SingleResult.Create(
                _context.Pemohon.Where(e => e.Id == id));
        }

        /// <summary>
        /// Creates a new Pemohon.
        /// </summary>
        /// <param name="create">The Pemohon to create.</param>
        /// <returns>The created Pemohon.</returns>
        /// <response code="201">The Pemohon was successfully created.</response>
        /// <response code="204">The Pemohon was successfully created.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="409">The Pemohon with supplied id already exist.</response>
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] Pemohon create)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Pemohon.Add(create);

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
        /// Updates an existing Pemohon.
        /// </summary>
        /// <param name="id">The requested Pemohon identifier.</param>
        /// <param name="delta">The partial Pemohon to update.</param>
        /// <returns>The updated Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully updated.</response>
        /// <response code="204">The Pemohon was successfully updated.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        /// <response code="422">The Pemohon identifier is specified on delta and its value is different from id.</response>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status422UnprocessableEntity)]
        public async Task<IActionResult> Patch(
            [FromODataUri] uint id,
            [FromBody] Delta<Pemohon> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var update = await _context.Pemohon.FindAsync(id);

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
        /// Deletes a Pemohon.
        /// </summary>
        /// <param name="id">The Pemohon to delete.</param>
        /// <returns>None</returns>
        /// <response code="204">The Pemohon was successfully deleted.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [ODataRoute(IdRoute)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Delete([FromODataUri] uint id)
        {
            var delete = await _context.Pemohon.FindAsync(id);

            if (delete == null)
            {
                return NotFound();
            }

            _context.Pemohon.Remove(delete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Updates an existing Pemohon.
        /// </summary>
        /// <param name="id">The requested Pemohon identifier.</param>
        /// <param name="update">The Pemohon to update.</param>
        /// <returns>The updated Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully updated.</response>
        /// <response code="204">The Pemohon was successfully updated.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [ODataRoute(IdRoute)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Put(
            [FromODataUri] uint id,
            [FromBody] Pemohon update)
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
            return _context.Pemohon.Any(e => e.Id == id);
        }

        private readonly PsefMySqlContext _context;
    }
}
