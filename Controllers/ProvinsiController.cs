using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsefApi.Model;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace PsefApi.Controllers
{
    /// <summary>
    /// Provinsi REST service.
    /// </summary>
    [ApiVersion(ApiInfo.V1_0)]
    [ApiVersion(ApiInfo.V1_1)]
    [ODataRoutePrefix(nameof(Provinsi))]
    public class ProvinsiController : ODataController
    {
        private readonly PsefMySqlContext _context;


        /// <summary>
        /// Provinsi REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        public ProvinsiController(PsefMySqlContext context)
        {
            _context = context;
        }

        /// <summary>
        /// GET item.
        /// </summary>
        /// <returns>List item.</returns>
        [ODataRoute]
        [Produces(ApiInfo.JsonOutput)]
        [ProducesResponseType(
            typeof(ODataValue<IEnumerable<Provinsi>>),
            Status200OK)]
        [EnableQuery(
            PageSize = 50,
            MaxTop = 100,
            AllowedQueryOptions = AllowedQueryOptions.All,
            AllowedFunctions = AllowedFunctions.AllFunctions)]
        public IQueryable<Provinsi> Get()
        {
            return _context.Provinsi;
        }

        /// <summary>
        /// GET item.
        /// </summary>
        /// <param name="id">Id item.</param>
        /// <returns>Item.</returns>
        [ODataRoute(ApiInfo.IdRoute)]
        [Produces(ApiInfo.JsonOutput)]
        [ProducesResponseType(typeof(Provinsi), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<Provinsi> Get([FromODataUri] byte id)
        {
            return SingleResult.Create(
                _context.Provinsi.Where(e => e.Id == id));
        }

        /// <summary>
        /// POST item.
        /// </summary>
        /// <param name="create">Item.</param>
        /// <returns>Status.</returns>
        [Produces(ApiInfo.JsonOutput)]
        [ProducesResponseType(typeof(Provinsi), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status409Conflict)]
        public async Task<IActionResult> Post([FromBody] Provinsi create)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Provinsi.Add(create);

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
        /// PATCH item.
        /// </summary>
        /// <param name="id">Id item</param>
        /// <param name="delta">Item.</param>
        /// <returns></returns>
        [ODataRoute(ApiInfo.IdRoute)]
        [Produces(ApiInfo.JsonOutput)]
        [ProducesResponseType(typeof(Provinsi), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status422UnprocessableEntity)]
        public async Task<IActionResult> Patch(
            [FromODataUri] byte id,
            [FromBody] Delta<Provinsi> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var update = await _context.Provinsi.FindAsync(id);

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
                    ModelState.AddModelError(nameof(update.Id), "must not set on patch.");
                    return UnprocessableEntity(ModelState);
                }

                throw;
            }

            return Updated(update);
        }

        /// <summary>
        /// DELETE item.
        /// </summary>
        /// <param name="id">Id item</param>
        /// <returns>Status.</returns>
        [ODataRoute(ApiInfo.IdRoute)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Delete([FromODataUri] byte id)
        {
            var delete = await _context.Provinsi.FindAsync(id);

            if (delete == null)
            {
                return NotFound();
            }

            _context.Provinsi.Remove(delete);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// PUT item.
        /// </summary>
        /// <param name="id">Id item</param>
        /// <param name="update">Item.</param>
        /// <returns>Status.</returns>
        [ODataRoute(ApiInfo.IdRoute)]
        [Produces(ApiInfo.JsonOutput)]
        [ProducesResponseType(typeof(Provinsi), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        public async Task<IActionResult> Put(
            [FromODataUri] byte id,
            [FromBody] Provinsi update)
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

        private bool Exists(byte id)
        {
            return _context.Provinsi.Any(e => e.Id == id);
        }
    }
}
