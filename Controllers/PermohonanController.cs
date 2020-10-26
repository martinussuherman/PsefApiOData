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
            _ossApi = ossApi;
            _memoryCache = memoryCache;
            _environment = environment;
            _context = context;
            _pemohonHelper = new PemohonUserInfoHelper(context, delegateService, identityApi);
            _helper = new PermohonanHelper(context);
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
            return await ProcessPermohonan(
                data,
                _helper.Verifikator(),
                PermohonanStatus.DisetujuiVerifikator);
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
            int diajukanCount = await _context.HistoryPermohonan
                .Where(c =>
                    c.PermohonanId == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.Diajukan.Id)
                .CountAsync();

            if (diajukanCount == _maxPermohonanDiajukan)
            {
                return await ProcessPermohonan(
                    data,
                    _helper.Verifikator(),
                    PermohonanStatus.Ditolak);
            }

            return await ProcessPermohonan(
                data,
                _helper.Verifikator(),
                PermohonanStatus.DikembalikanVerifikator);
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
            return await ProcessPermohonan(
                data,
                _helper.Kasi(),
                PermohonanStatus.DisetujuiKepalaSeksi);
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
            return await ProcessPermohonan(
                data,
                _helper.Kasi(),
                PermohonanStatus.DikembalikanKepalaSeksi);
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
            return await ProcessPermohonan(
                data,
                _helper.Kasubdit(),
                PermohonanStatus.DisetujuiKepalaSubDirektorat);
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
            return await ProcessPermohonan(
                data,
                _helper.Kasubdit(),
                PermohonanStatus.DikembalikanKepalaSubDirektorat);
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
            return await ProcessPermohonan(
                data,
                _helper.Diryanfar(),
                PermohonanStatus.DisetujuiDirekturPelayananFarmasi);
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
            return await ProcessPermohonan(
                data,
                _helper.Diryanfar(),
                PermohonanStatus.DikembalikanDirekturPelayananFarmasi);
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
            return await ProcessPermohonan(
                data,
                _helper.Dirjen(),
                PermohonanStatus.DisetujuiDirekturJenderal);
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
            return await ProcessPermohonan(
                data,
                _helper.Dirjen(),
                PermohonanStatus.DikembalikanDirekturJenderal);
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
            _context.HistoryPermohonan.Add(CreateHistory(update, data));

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            DateTime maxExpiry = DateTime.Today.AddYears(_perizinanYears);
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
        /// Retrieves all Permohonan.
        /// </summary>
        /// <remarks>
        /// *Min Role: Verifikator*
        /// </remarks>
        /// <returns>All available pending Permohonan for Verifikator.</returns>
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
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> Semua()
        {
            List<Permohonan> permohonanList = await _context.Permohonan
                .Where(c =>
                    c.StatusId != PermohonanStatus.Dibuat.Id &&
                    c.StatusId != PermohonanStatus.DikembalikanVerifikator.Id)
                .ToListAsync();

            return (await MergeList(permohonanList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> VerifikatorPending()
        {
            List<Permohonan> permohonanList = await _helper.Verifikator().ToListAsync();
            List<HistoryPermohonanTimeData> timeList = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(c =>
                    c.Permohonan.StatusId == PermohonanStatus.Diajukan.Id ||
                    c.Permohonan.StatusId == PermohonanStatus.DikembalikanKepalaSeksi.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new HistoryPermohonanTimeData
                {
                    Id = c.PermohonanId,
                    StatusId = c.StatusId,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return (await MergeList(permohonanList, timeList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> KepalaSeksiPending()
        {
            List<Permohonan> permohonanList = await _helper.Kasi().ToListAsync();
            List<HistoryPermohonanTimeData> timeList = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(c =>
                    c.Permohonan.StatusId == PermohonanStatus.DisetujuiVerifikator.Id ||
                    c.Permohonan.StatusId == PermohonanStatus.DikembalikanKepalaSubDirektorat.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new HistoryPermohonanTimeData
                {
                    Id = c.PermohonanId,
                    StatusId = c.StatusId,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return (await MergeList(permohonanList, timeList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> KepalaSubDirektoratPending()
        {
            List<Permohonan> permohonanList = await _helper.Kasubdit().ToListAsync();
            List<HistoryPermohonanTimeData> timeList = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(c =>
                    c.Permohonan.StatusId == PermohonanStatus.DisetujuiKepalaSeksi.Id ||
                    c.Permohonan.StatusId == PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new HistoryPermohonanTimeData
                {
                    Id = c.PermohonanId,
                    StatusId = c.StatusId,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return (await MergeList(permohonanList, timeList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> DirekturPelayananFarmasiPending()
        {
            List<Permohonan> permohonanList = await _helper.Diryanfar().ToListAsync();
            List<HistoryPermohonanTimeData> timeList = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(c =>
                    c.Permohonan.StatusId == PermohonanStatus.DisetujuiKepalaSubDirektorat.Id ||
                    c.Permohonan.StatusId == PermohonanStatus.DikembalikanDirekturJenderal.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new HistoryPermohonanTimeData
                {
                    Id = c.PermohonanId,
                    StatusId = c.StatusId,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return (await MergeList(permohonanList, timeList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> DirekturJenderalPending()
        {
            List<Permohonan> permohonanList = await _helper.Dirjen().ToListAsync();
            List<HistoryPermohonanTimeData> timeList = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(c =>
                    c.Permohonan.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id)
                .OrderByDescending(c => c.UpdatedAt)
                .Select(c => new HistoryPermohonanTimeData
                {
                    Id = c.PermohonanId,
                    StatusId = c.StatusId,
                    UpdatedAt = c.UpdatedAt
                })
                .ToListAsync();

            return (await MergeList(permohonanList, timeList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> ValidatorSertifikatPending()
        {
            List<Permohonan> permohonanList = await _helper.Validator().ToListAsync();

            return (await MergeList(permohonanList)).AsQueryable();
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PermohonanPemohon>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PermohonanPemohon>> ValidatorSertifikatDone()
        {
            List<Permohonan> permohonanList = await _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.Selesai.Id)
                .ToListAsync();

            return (await MergeList(permohonanList)).AsQueryable();
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
            return await _helper.Verifikator().LongCountAsync();
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
            return await _helper.Kasi().LongCountAsync();
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
            return await _helper.Kasubdit().LongCountAsync();
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
            return await _helper.Diryanfar().LongCountAsync();
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
            return await _helper.Dirjen().LongCountAsync();
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
            return await _helper.Validator().LongCountAsync();
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
        [EnableQuery]
        public IQueryable<Permohonan> Progress()
        {
            return _context.Permohonan.Where(e =>
                e.StatusId != PermohonanStatus.Dibuat.Id &&
                e.StatusId != PermohonanStatus.DikembalikanVerifikator.Id &&
                e.StatusId != PermohonanStatus.Selesai.Id &&
                e.StatusId != PermohonanStatus.Ditolak.Id);
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
        [EnableQuery]
        public IQueryable<Permohonan> Selesai()
        {
            return _context.Permohonan.Where(e =>
                e.StatusId == PermohonanStatus.Selesai.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan with status Ditolak.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <returns>All available Permohonan with status Ditolak.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> Ditolak()
        {
            return _context.Permohonan.Where(e =>
                e.StatusId == PermohonanStatus.Ditolak.Id);
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

        internal class HistoryPermohonanTimeData
        {
            internal uint? Id { get; set; }
            internal byte StatusId { get; set; }
            internal DateTime UpdatedAt { get; set; }
        }

        private async Task<IActionResult> ProcessPermohonan(
            PermohonanSystemUpdate data,
            IQueryable<Permohonan> query,
            PermohonanStatus status,
            object delegateFunction = null)
        {
            Permohonan update = await query
                .FirstOrDefaultAsync(c => c.Id == data.PermohonanId);

            if (update == null)
            {
                return NotFound();
            }

            update.StatusId = status.Id;
            _context.HistoryPermohonan.Add(CreateHistory(update, data));

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            if (delegateFunction != null)
            {
                // call delegate function to do extra processing, ex: send email 
            }

            return NoContent();
        }
        private HistoryPermohonan CreateHistory(Permohonan permohonan, PermohonanSystemUpdate update)
        {
            return new HistoryPermohonan
            {
                PermohonanId = permohonan.Id,
                StatusId = permohonan.StatusId,
                Reason = update.Reason ?? string.Empty,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };
        }
        private async Task<List<PermohonanPemohon>> MergeList(
            List<Permohonan> permohonanList,
            List<HistoryPermohonanTimeData> timeList = null)
        {
            List<PemohonUserInfo> pemohonList = await _pemohonHelper.RetrieveList(HttpContext);
            List<PermohonanPemohon> result = new List<PermohonanPemohon>();

            foreach (Permohonan permohonan in permohonanList)
            {
                PemohonUserInfo pemohon = pemohonList
                    .FirstOrDefault(c => c.Id == permohonan.PemohonId);
                HistoryPermohonanTimeData totalTimeData = timeList?
                    .FirstOrDefault(c =>
                        c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id &&
                        c.Id == permohonan.Id);
                HistoryPermohonanTimeData userTimeData = timeList?
                    .FirstOrDefault(c => c.Id == permohonan.Id);
                result.Add(Merge(pemohon, permohonan, totalTimeData, userTimeData));
            }

            return result;
        }
        private PermohonanPemohon Merge(
            PemohonUserInfo pemohon,
            Permohonan permohonan,
            HistoryPermohonanTimeData totalTimeData,
            HistoryPermohonanTimeData userTimeData)
        {
            DateTime totalStartDate = totalTimeData?.UpdatedAt == null ?
                DateTime.Now :
                totalTimeData.UpdatedAt;
            DateTime userStartDate = userTimeData?.UpdatedAt == null ?
                DateTime.Now :
                userTimeData.UpdatedAt;

            return new PermohonanPemohon
            {
                PermohonanId = permohonan.Id,
                PermohonanNumber = permohonan.PermohonanNumber,
                Domain = permohonan.Domain,
                StatusName = permohonan.StatusName,
                TypeName = permohonan.TypeName,
                LastUpdate = permohonan.LastUpdate,
                CompanyName = pemohon?.CompanyName,
                Email = pemohon?.Email,
                Name = pemohon?.Name,
                Nib = pemohon?.Nib,
                TotalDays = GetWorkingDays(totalStartDate, DateTime.Today),
                UserLevelDays = GetWorkingDays(userStartDate, DateTime.Today)
            };
        }
        // https://stackoverflow.com/questions/1617049/calculate-the-number-of-business-days-between-two-dates
        private int GetWorkingDays(DateTime startDate, DateTime endDate)
        {
            int dayDifference = endDate.Subtract(startDate).Days;
            return Enumerable
                .Range(1, dayDifference)
                .Select(x => startDate.AddDays(x))
                .Count(x => x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday);
        }
        private bool Exists(uint id)
        {
            return _context.Permohonan.Any(e => e.Id == id);
        }
        private string MonthToRomanNumber(DateTime date)
        {
            return RomanNumberHelper.ToRomanNumber(date.Month);
        }

        private readonly PemohonUserInfoHelper _pemohonHelper;
        private readonly PermohonanHelper _helper;
        private readonly PsefMySqlContext _context;
        private readonly IOssApiService _ossApi;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebHostEnvironment _environment;
        private const int _perizinanYears = 5;
        private const int _maxPermohonanDiajukan = 3;
    }
}
