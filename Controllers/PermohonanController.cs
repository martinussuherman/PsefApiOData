using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
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
        /// <param name="ossApi">Oss Api service.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="environment">Web Host environment.</param>
        public PermohonanController(
            PsefMySqlContext context,
            IApiDelegateService delegateService,
            IIdentityApiService identityApi,
            IOssApiService ossApi,
            IMemoryCache memoryCache,
            IWebHostEnvironment environment)
        {
            _identityApi = identityApi;
            _ossApi = ossApi;
            _memoryCache = memoryCache;
            _environment = environment;
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
            ApiRole.Validator,
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
            ApiRole.Validator,
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

        /// <summary>
        /// Accept an existing Permohonan by Verifikator.
        /// </summary>
        /// <remarks>
        /// *Role: Verifikator*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Verifikator)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> VerifikatorSetujui(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.Diajukan.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSeksi.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DisetujuiVerifikator.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DisetujuiVerifikator.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Return an existing Permohonan by Verifikator.
        /// </summary>
        /// <remarks>
        /// *Role: Verifikator*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Verifikator)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> VerifikatorKembalikan(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.Diajukan.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSeksi.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DikembalikanVerifikator.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DikembalikanVerifikator.Id,
                Reason = data.Reason,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            // TODO : notify Pemohon via email?
            return NoContent();
        }

        /// <summary>
        /// Accept an existing Permohonan by Kepala Seksi.
        /// </summary>
        /// <remarks>
        /// *Role: Kasi*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Kasi)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> KepalaSeksiSetujui(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSubDirektorat.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DisetujuiKepalaSeksi.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DisetujuiKepalaSeksi.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Return an existing Permohonan by Kepala Seksi.
        /// </summary>
        /// <remarks>
        /// *Role: Kasi*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Kasi)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> KepalaSeksiKembalikan(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSubDirektorat.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DikembalikanKepalaSeksi.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DikembalikanKepalaSeksi.Id,
                Reason = data.Reason,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Accept an existing Permohonan by Kepala Sub Direktorat.
        /// </summary>
        /// <remarks>
        /// *Role: Kasubdit*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Kasubdit)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> KepalaSubDirektoratSetujui(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.DisetujuiKepalaSeksi.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DisetujuiKepalaSubDirektorat.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DisetujuiKepalaSubDirektorat.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Return an existing Permohonan by Kepala Sub Direktorat.
        /// </summary>
        /// <remarks>
        /// *Role: Kasubdit*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Kasubdit)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> KepalaSubDirektoratKembalikan(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.DisetujuiKepalaSeksi.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DikembalikanKepalaSubDirektorat.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DikembalikanKepalaSubDirektorat.Id,
                Reason = data.Reason,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Accept an existing Permohonan by Direktur Pelayanan Farmasi.
        /// </summary>
        /// <remarks>
        /// *Role: Diryanfar*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Diryanfar)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> DirekturPelayananFarmasiSetujui(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.DisetujuiKepalaSubDirektorat.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturJenderal.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Return an existing Permohonan by Direktur Pelayanan Farmasi.
        /// </summary>
        /// <remarks>
        /// *Role: Diryanfar*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Diryanfar)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> DirekturPelayananFarmasiKembalikan(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    (c.StatusId == PermohonanStatus.DisetujuiKepalaSubDirektorat.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturJenderal.Id));

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id,
                Reason = data.Reason,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Accept an existing Permohonan by Direktur Jenderal.
        /// </summary>
        /// <remarks>
        /// *Role: Dirjen*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Dirjen)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> DirekturJenderalSetujui(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id);

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DisetujuiDirekturJenderal.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DisetujuiDirekturJenderal.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Return an existing Permohonan by Direktur Jenderal.
        /// </summary>
        /// <remarks>
        /// *Role: Dirjen*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Dirjen)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> DirekturJenderalKembalikan(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id);

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.DikembalikanDirekturJenderal.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.DikembalikanDirekturJenderal.Id,
                Reason = data.Reason,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Finish an existing Permohonan by Validator.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> ValidatorSelesaikan(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.DisetujuiDirekturJenderal.Id);

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = PermohonanStatus.Selesai.Id;

            HistoryPermohonan history = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.Selesai.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(history);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            // TODO : use const/read from setting
            DateTime maxExpiry = DateTime.Today.AddYears(5);
            DateTime expiry = maxExpiry.CompareTo(update.StraExpiry) > 0 ?
                update.StraExpiry :
                maxExpiry;

            Perizinan perizinan = new Perizinan
            {
                PermohonanId = update.Id,
                ExpiredAt = expiry,
                IssuedAt = DateTime.Today,
                PreviousId = update.PreviousPerizinanId
            };

            CounterHelper counterHelper = new CounterHelper(_context);
            perizinan.PerizinanNumber = await counterHelper.GetFormNumber(
                CounterType.Perizinan,
                monthFunc: MonthToRomanNumber);

            Pemohon pemohon = await _context.Pemohon
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == update.PemohonId);
            OssInfoHelper ossInfoHelper = new OssInfoHelper(_ossApi, _memoryCache);
            OssFullInfo ossFullInfo = await ossInfoHelper.RetrieveInfo(pemohon.Nib);
            TandaDaftarHelper helper = new TandaDaftarHelper(_environment, HttpContext, Url);
            perizinan.TandaDaftarUrl = helper.GeneratePdf(ossFullInfo, update, perizinan);

            _context.Perizinan.Add(perizinan);
            await _context.SaveChangesAsync();

            update.PerizinanId = perizinan.Id;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Re-generate Tanda Daftar Pdf by Validator.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> ValidatorRegenerateTandaDaftar(
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan permohonan = await _context.Permohonan
                .AsNoTracking()
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.Selesai.Id);

            if (permohonan == null)
            {
                return NotFound();
            }

            Perizinan perizinan = await _context.Perizinan
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == permohonan.PerizinanId);
            Pemohon pemohon = await _context.Pemohon
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == permohonan.PemohonId);
            OssInfoHelper ossInfoHelper = new OssInfoHelper(_ossApi, _memoryCache);
            OssFullInfo ossFullInfo = await ossInfoHelper.RetrieveInfo(pemohon.Nib);
            TandaDaftarHelper helper = new TandaDaftarHelper(_environment, HttpContext, Url);
            string path = helper.GeneratePdf(ossFullInfo, permohonan, perizinan);

            return NoContent();
        }

        /// <summary>
        /// Retrieves all pending Permohonan for Verifikator.
        /// </summary>
        /// <remarks>
        /// *Role: Verifikator*
        /// </remarks>
        /// <returns>All available pending Permohonan for Verifikator.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Verifikator)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> VerifikatorPending()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.Diajukan.Id ||
                c.StatusId == PermohonanStatus.DikembalikanKepalaSeksi.Id);
        }

        /// <summary>
        /// Retrieves all pending Permohonan for Kepala Seksi.
        /// </summary>
        /// <remarks>
        /// *Role: Kasi*
        /// </remarks>
        /// <returns>All available pending Permohonan for Kepala Seksi.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Kasi)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> KepalaSeksiPending()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id ||
                c.StatusId == PermohonanStatus.DikembalikanKepalaSubDirektorat.Id);
        }

        /// <summary>
        /// Retrieves all pending Permohonan for Kepala Sub Direktorat.
        /// </summary>
        /// <remarks>
        /// *Role: Kasubdit*
        /// </remarks>
        /// <returns>All available pending Permohonan for Kepala Sub Direktorat.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Kasubdit)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> KepalaSubDirektoratPending()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.DisetujuiKepalaSeksi.Id ||
                c.StatusId == PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id);
        }

        /// <summary>
        /// Retrieves all pending Permohonan for Direktur Pelayanan Farmasi.
        /// </summary>
        /// <remarks>
        /// *Role: Diryanfar*
        /// </remarks>
        /// <returns>All available pending Permohonan for Direktur Pelayanan Farmasi.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Diryanfar)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> DirekturPelayananFarmasiPending()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.DisetujuiKepalaSubDirektorat.Id ||
                c.StatusId == PermohonanStatus.DikembalikanDirekturJenderal.Id);
        }

        /// <summary>
        /// Retrieves all pending Permohonan for Direktur Jenderal.
        /// </summary>
        /// <remarks>
        /// *Role: Dirjen*
        /// </remarks>
        /// <returns>All available pending Permohonan for Direktur Jenderal.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Dirjen)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> DirekturJenderalPending()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id);
        }

        /// <summary>
        /// Retrieves all pending Permohonan for Validator Sertifikat.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <returns>All available pending Permohonan for Validator Sertifikat.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> ValidatorSertifikatPending()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.DisetujuiDirekturJenderal.Id);
        }

        /// <summary>
        /// Retrieves all finished Permohonan for Validator Sertifikat.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <returns>All available finished Permohonan for Validator Sertifikat.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> ValidatorSertifikatDone()
        {
            return _context.Permohonan.Where(c =>
                c.StatusId == PermohonanStatus.Selesai.Id);
        }

        /// <summary>
        /// Retrieves pending Permohonan for Verifikator total count.
        /// </summary>
        /// <remarks>
        /// *Role: Verifikator*
        /// </remarks>
        /// <returns>Pending Permohonan for Verifikator total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Verifikator)]
        [HttpGet]
        [ODataRoute(nameof(VerifikatorPendingTotal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> VerifikatorPendingTotal()
        {
            return await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.Diajukan.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSeksi.Id)
                .LongCountAsync();
        }

        /// <summary>
        /// Retrieves pending Permohonan for Kepala Seksi total count.
        /// </summary>
        /// <remarks>
        /// *Role: Kasi*
        /// </remarks>
        /// <returns>Pending Permohonan for Kepala Seksi total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Kasi)]
        [HttpGet]
        [ODataRoute(nameof(KepalaSeksiPendingTotal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> KepalaSeksiPendingTotal()
        {
            return await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSubDirektorat.Id)
                .LongCountAsync();
        }

        /// <summary>
        /// Retrieves pending Permohonan for Kepala Sub Direktorat total count.
        /// </summary>
        /// <remarks>
        /// *Role: Kasubdit*
        /// </remarks>
        /// <returns>Pending Permohonan for Kepala Sub Direktorat total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Kasubdit)]
        [HttpGet]
        [ODataRoute(nameof(KepalaSubDirektoratPendingTotal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> KepalaSubDirektoratPendingTotal()
        {
            return await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiKepalaSeksi.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id)
                .LongCountAsync();
        }

        /// <summary>
        /// Retrieves pending Permohonan for Direktur Pelayanan Farmasi total count.
        /// </summary>
        /// <remarks>
        /// *Role: Diryanfar*
        /// </remarks>
        /// <returns>Pending Permohonan for Direktur Pelayanan Farmasi total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Diryanfar)]
        [HttpGet]
        [ODataRoute(nameof(DirekturPelayananFarmasiPendingTotal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> DirekturPelayananFarmasiPendingTotal()
        {
            return await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiKepalaSubDirektorat.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturJenderal.Id)
                .LongCountAsync();
        }

        /// <summary>
        /// Retrieves pending Permohonan for Direktur Jenderal total count.
        /// </summary>
        /// <remarks>
        /// *Role: Dirjen*
        /// </remarks>
        /// <returns>Pending Permohonan for Direktur Jenderal total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Dirjen)]
        [HttpGet]
        [ODataRoute(nameof(DirekturJenderalPendingTotal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> DirekturJenderalPendingTotal()
        {
            return await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id)
                .LongCountAsync();
        }

        /// <summary>
        /// Retrieves pending Permohonan for Validator Sertifikat total count.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <returns>Pending Permohonan for Validator Sertifikat total count.</returns>
        /// <response code="200">Total count of Permohonan retrieved.</response>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpGet]
        [ODataRoute(nameof(ValidatorSertifikatPendingTotal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> ValidatorSertifikatPendingTotal()
        {
            return await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiDirekturJenderal.Id)
                .LongCountAsync();
        }

        /// <summary>
        /// Retrieves all Permohonan with status Rumusan.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <returns>All available Permohonan with status Rumusan.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [ProducesResponseType(Status403Forbidden)]
        [EnableQuery]
        public IQueryable<Permohonan> Rumusan()
        {
            return _context.Permohonan.Where(e =>
                e.StatusId == PermohonanStatus.Dibuat.Id ||
                e.StatusId == PermohonanStatus.DikembalikanVerifikator.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan with status Progress.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <returns>All available Permohonan with status Progress.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [ProducesResponseType(Status403Forbidden)]
        [EnableQuery]
        public IQueryable<Permohonan> Progress()
        {
            return _context.Permohonan.Where(e =>
                e.StatusId != PermohonanStatus.Dibuat.Id &&
                e.StatusId != PermohonanStatus.DikembalikanVerifikator.Id &&
                e.StatusId != PermohonanStatus.Selesai.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan with status Selesai.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <returns>All available Permohonan with status Selesai.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [ProducesResponseType(Status403Forbidden)]
        [EnableQuery]
        public IQueryable<Permohonan> Selesai()
        {
            return _context.Permohonan.Where(e =>
                e.StatusId == PermohonanStatus.Selesai.Id);
        }

        /// <summary>
        /// Retrieves Permohonan layanan total start time.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <param name="id">The requested Permohonan identifier.</param>
        /// <returns>The start time for Permohonan layanan total.</returns>
        [MultiRoleAuthorize(
            ApiRole.Verifikator,
            ApiRole.Validator,
            ApiRole.Kasi,
            ApiRole.Kasubdit,
            ApiRole.Diryanfar,
            ApiRole.Dirjen,
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DateTime), Status200OK)]
        public async Task<DateTime> LayananTotalStartTime([FromQuery] uint id)
        {
            HistoryPermohonan history = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(
                    c => c.PermohonanId == id &&
                    c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id &&
                    c.Permohonan.StatusId >= PermohonanStatus.DisetujuiVerifikator.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .FirstOrDefaultAsync();

            return history is null ? DateTime.Now : history.UpdatedAt;
        }

        private bool Exists(uint id)
        {
            return _context.Permohonan.Any(e => e.Id == id);
        }

        private string MonthToRomanNumber(DateTime date)
        {
            return RomanNumberHelper.ToRomanNumber(date.Month);
        }

        private readonly PsefMySqlContext _context;
        private readonly IApiDelegateService _delegateService;
        private readonly IIdentityApiService _identityApi;
        private readonly IOssApiService _ossApi;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _environment;
    }
}
