using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PsefApiOData.Misc;
using PsefApiOData.Models;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;
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
        /// <param name="environment">Web Host environment.</param>
        public PermohonanController(
            PsefMySqlContext context,
            IApiDelegateService delegateService,
            IIdentityApiService identityApi,
            IWebHostEnvironment environment)
        {
            _identityApi = identityApi;
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

            CounterHelper helper = new CounterHelper(_context);
            perizinan.PerizinanNumber = await helper.GetFormNumber(
                CounterType.Perizinan,
                monthFunc: MonthToRomanNumber);
            Pemohon pemohon = await _context.Pemohon
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == update.PemohonId);
            // TODO : read from OSS Api
            OssInfo ossInfo = OssInfoController.dummy.FirstOrDefault(e => e.Nib == pemohon.Nib);

            perizinan.TandaDaftarUrl = TandDaftarPdf(pemohon, update, perizinan, ossInfo);
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
            
            // TODO : read from OSS Api
            OssInfo ossInfo = OssInfoController.dummy.FirstOrDefault(e => e.Nib == pemohon.Nib);

            string path = TandDaftarPdf(pemohon, permohonan, perizinan, ossInfo);

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
        /// Retrieves all Apotek for the specified Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <param name="permohonanId">The requested Permohonan identifier.</param>
        /// <returns>All available Apotek for the specified Permohonan.</returns>
        /// <response code="200">List of Apotek successfully retrieved.</response>
        /// <response code="404">The list of Apotek does not exist.</response>
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Apotek>>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery]
        public IQueryable<Apotek> ListApotek(uint permohonanId)
        {
            return _context.Apotek
                .Include(e => e.Provinsi)
                .Where(e => e.PermohonanId == permohonanId);
        }

        /// <summary>
        /// Retrieves all History Permohonan for the specified Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <param name="permohonanId">The requested Permohonan identifier.</param>
        /// <returns>All available History Permohonan for the specified Permohonan.</returns>
        /// <response code="200">List of History Permohonan successfully retrieved.</response>
        /// <response code="404">The list of History Permohonan does not exist.</response>
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<HistoryPermohonan>>), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery]
        public IQueryable<HistoryPermohonan> ListHistory(uint permohonanId)
        {
            return _context.HistoryPermohonan.Where(e => e.PermohonanId == permohonanId);
        }


        private bool Exists(uint id)
        {
            return _context.Permohonan.Any(e => e.Id == id);
        }

        private string MonthToRomanNumber(DateTime date)
        {
            return RomanNumberHelper.ToRomanNumber(date.Month);
        }

        private string TandDaftarPdf(
            Pemohon pemohon,
            Permohonan permohonan,
            Perizinan perizinan,
            OssInfo ossInfo)
        {
            string datePath = perizinan.IssuedAt.ToString(
                "yyyy-MM-dd",
                DateTimeFormatInfo.InvariantInfo);
            string folderPath = Path.Combine(_environment.WebRootPath, datePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{ApiHelper.GetUserId(HttpContext.User)}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            PdfUnitConverter converter = new PdfUnitConverter();
            PdfDocument document = new PdfDocument();

            document.PageSettings.Size = new SizeF(
                converter.ConvertUnits(
                    8.5f,
                    PdfGraphicsUnit.Inch,
                    PdfGraphicsUnit.Point),
                converter.ConvertUnits(
                    11,
                    PdfGraphicsUnit.Inch,
                    PdfGraphicsUnit.Point));
            document.PageSettings.SetMargins(
                converter.ConvertUnits(
                    3,
                    PdfGraphicsUnit.Centimeter,
                    PdfGraphicsUnit.Point),
                converter.ConvertUnits(
                    2,
                    PdfGraphicsUnit.Centimeter,
                    PdfGraphicsUnit.Point));

            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;
            float top;

            DrawLogo(graphics);
            top = DrawHeader(perizinan, graphics);
            top = DrawContent(pemohon, permohonan, perizinan, ossInfo, graphics, top);
            top = DrawSignature(perizinan, page, graphics, top);
            DrawFooter(perizinan, graphics, top);

            FileStream fileStream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.ReadWrite);

            document.Save(fileStream);
            document.Close(true);

            return Url.Content($"~/{datePath}/{fileName}");
        }

        private void DrawLogo(PdfGraphics graphics)
        {
            PdfBitmap logo = new PdfBitmap(new FileStream(
                "logo-kemkes.jpg",
                FileMode.Open,
                FileAccess.Read));
            float drawWidth = logo.Width / 4;
            float drawHeight = logo.Height / 4;

            graphics.DrawImage(
                logo,
                (graphics.ClientSize.Width - drawWidth) / 2,
                0,
                drawWidth,
                drawHeight);
        }

        private float DrawHeader(Perizinan perizinan, PdfGraphics graphics)
        {
            PdfStringFormat centered = new PdfStringFormat
            {
                Alignment = PdfTextAlignment.Center
            };

            graphics.DrawString(
                "KEMENTERIAN KESEHATAN\nREPUBLIK INDONESIA",
                new PdfStandardFont(PdfFontFamily.Helvetica, 10),
                PdfBrushes.Black,
                new RectangleF(0, 60, graphics.ClientSize.Width, 24),
                centered);
            graphics.DrawString(
                $"TANDA DAFTAR\nPENYELENGGARA SISTEM ELEKTRONIK FARMASI (PSEF)",
                new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold),
                PdfBrushes.Black,
                new RectangleF(0, 94, graphics.ClientSize.Width, 32),
                centered);
            graphics.DrawString(
                $"NOMOR: {perizinan.PerizinanNumber}",
                new PdfStandardFont(PdfFontFamily.Helvetica, 12, PdfFontStyle.Bold),
                PdfBrushes.Black,
                new RectangleF(0, 136, graphics.ClientSize.Width, 16),
                centered);

            return 136 + 16;
        }

        private float DrawContent(
            Pemohon pemohon,
            Permohonan permohonan,
            Perizinan perizinan,
            OssInfo ossInfo,
            PdfGraphics graphics,
            float top)
        {
            float leftColX = 5;
            float rightColX = graphics.ClientSize.Width / 2 + 5;
            float firstContentTop = top + 10;
            float secondContentTop;

            ContentItemDraw(
                graphics,
                "Nomor Tanda Daftar PSE:",
                permohonan.PermohonanNumber,
                1,
                new PointF(leftColX, firstContentTop));

            secondContentTop = ContentItemDraw(
                graphics,
                "Bidang:",
                "PELAYANAN",
                1,
                new PointF(rightColX, firstContentTop));

            top = ContentItemDraw(
                graphics,
                "Nama Sistem Elektronik:",
                permohonan.SystemName,
                1,
                new PointF(leftColX, secondContentTop));
            top = ContentItemDraw(
                graphics,
                "Alamat Domain Sistem Elektronik:",
                permohonan.Domain,
                1,
                new PointF(leftColX, top));
            top = ContentItemDraw(
                graphics,
                "Apoteker Penanggung Jawab:",
                permohonan.ApotekerName,
                2,
                new PointF(leftColX, top));
            ContentItemDraw(
                graphics,
                "Nomor STRA:",
                permohonan.StraNumber,
                1,
                new PointF(leftColX, top));

            top = ContentItemDraw(
                graphics,
                "Nomor Induk Berusaha:",
                pemohon.Nib,
                1,
                new PointF(rightColX, secondContentTop));
            top = ContentItemDraw(
                graphics,
                "Diajukan oleh:",
                (ossInfo?.Name) != null ? ossInfo.Name : string.Empty,
                2,
                new PointF(rightColX, top));
            top = ContentItemDraw(
                graphics,
                "Alamat:",
                (ossInfo?.Address) != null ? ossInfo.Address : string.Empty,
                3,
                new PointF(rightColX, top));
            top = ContentItemDraw(
                graphics,
                "Tanggal Terbit:",
                perizinan.IssuedAt.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("id")),
                1,
                new PointF(rightColX, top));

            return top;
        }

        private float ContentItemDraw(
            PdfGraphics graphics,
            string label,
            string content,
            int contentLine,
            PointF location)
        {
            float width = (graphics.ClientSize.Width - 20) / 2;
            PdfFont labelFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Italic);
            PdfFont contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);

            graphics.DrawString(
                label,
                labelFont,
                PdfBrushes.Black,
                new RectangleF(location.X, location.Y, width, 12));
            graphics.DrawString(
                content,
                contentFont,
                PdfBrushes.Black,
                new RectangleF(location.X, location.Y + 12, width, 15 * contentLine));

            return location.Y + 12 + 15 * contentLine + 4;
        }

        private float DrawSignature(Perizinan perizinan, PdfPage page, PdfGraphics graphics, float top)
        {
            float halfWidth = graphics.ClientSize.Width / 2;
            float leftX = 5;
            float rightX = halfWidth - 30;

            PdfQRBarcode barcode = new PdfQRBarcode
            {
                ErrorCorrectionLevel = PdfErrorCorrectionLevel.High,
                XDimension = 3,
                Text = perizinan.PerizinanNumber
            };

            barcode.Draw(page, new PointF(leftX, top + 20));

            graphics.DrawString(
                "A.N. MENTERI KESEHATAN\nDIREKTUR JENDERAL\nKEFARMASIAN DAN ALAT KESEHATAN\n\n\nTTD\n\n\nENGKO SOSIALINE M.",
                new PdfStandardFont(PdfFontFamily.Helvetica, 11),
                PdfBrushes.Black,
                new RectangleF(rightX, top + 18, halfWidth + 30, 130));

            return top + 18 + 130;
        }

        private void DrawFooter(Perizinan perizinan, PdfGraphics graphics, float top)
        {
            float leftColX = 5;
            PdfFont italicFont = new PdfStandardFont(
                PdfFontFamily.Helvetica,
                11, PdfFontStyle.Italic);
            PdfFont labelFont = new PdfStandardFont(
                PdfFontFamily.Helvetica,
                11);
            float width = graphics.ClientSize.Width - 12;

            graphics.DrawString(
                "Keterangan:",
                italicFont,
                PdfBrushes.Black,
                new RectangleF(leftColX, top, width, 15));

            top += 15;
            graphics.DrawString(
                "-",
                labelFont,
                PdfBrushes.Black,
                new RectangleF(leftColX, top, width, 15));
            graphics.DrawString(
                "Tanda Daftar Penyelenggara Sistem Elektronik Farmasi (PSEF) Merupakan Dokumen Pemenuhan Komitmen Dalam Izin Operasional/Komersial Penyelenggara Sistem Elektronik Farmasi (PSEF)",
                labelFont,
                PdfBrushes.Black,
                new RectangleF(leftColX + 8, top, width, 42));

            top += 42;
            string expired = perizinan.ExpiredAt.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("id"));
            graphics.DrawString(
                "-",
                labelFont,
                PdfBrushes.Black,
                new RectangleF(leftColX, top, width, 15));
            graphics.DrawString(
                $"Tanda Daftar Penyelenggara Sistem Elektronik Farmasi (Psef) Berlaku Sejak Tanggal Diterbitkan Sampai Dengan {expired}",
                labelFont,
                PdfBrushes.Black,
                new RectangleF(leftColX + 8, top, width, 28));

            top += 28;
            graphics.DrawString(
                "-",
                labelFont,
                PdfBrushes.Black,
                new RectangleF(leftColX, top, width, 15));
            graphics.DrawString(
                "Jika Tidak Dilakukan Pembaruan, Akan Dihapus Dari Tanda Daftar PSEF Kementerian Kesehatan",
                labelFont,
                PdfBrushes.Black,
                new RectangleF(leftColX + 8, top, width, 28));
        }

        private readonly PsefMySqlContext _context;
        private readonly IApiDelegateService _delegateService;
        private readonly IIdentityApiService _identityApi;
        private readonly IWebHostEnvironment _environment;
    }
}
