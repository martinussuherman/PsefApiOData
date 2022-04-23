using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PsefApiOData.Misc;
using PsefApiOData.Models;
using PsefApiOData.Models.ViewModels;
using WorkDaysCalculator;
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
        /// <param name="delegateService">API delegation service.</param>
        /// <param name="identityApi">Identity API service.</param>
        /// <param name="ossApi">Oss API service.</param>
        /// <param name="smtpEmailService">SMTP email service.</param>
        /// <param name="environment">Web Host environment.</param>
        /// <param name="signatureOptions">Electronic signature options.</param>
        /// <param name="ossOptions">OSS API configuration options.</param>
        /// <param name="emailOptions">SMTP email options.</param>
        public PermohonanController(
            PsefMySqlContext context,
            IApiDelegateService delegateService,
            IIdentityApiService identityApi,
            IOssApiService ossApi,
            SmtpEmailService smtpEmailService,
            IWebHostEnvironment environment,
            IOptions<ElectronicSignatureOptions> signatureOptions,
            IOptions<OssApiOptions> ossOptions,
            IOptions<PermohonanEmailOptions> emailOptions)
        {
            _smtpEmailService = smtpEmailService;
            _environment = environment;
            _signatureOptions = signatureOptions;
            _context = context;
            _pemohonHelper = new PemohonUserInfoHelper(context, delegateService, identityApi);
            _ossHelper = new OssInfoHelper(ossApi, ossOptions);
            _helper = new PermohonanHelper(context);
            _defaultCc = new MailAddressCollection
            {
                new MailAddress(emailOptions.Value.To, emailOptions.Value.ToDisplay ?? string.Empty)
            };
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
            ApiRole.Supervisor,
            ApiRole.Timja,
            ApiRole.Dirpenyanfar,
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
            if (string.IsNullOrEmpty(ApiHelper.GetUserRole(HttpContext.User)))
            {
                return SingleResult.Create(_context.Permohonan
                    .Where(e => e.Id == id &&
                    e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User)));
            }

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

            if (diajukanCount >= _maxPermohonanDiajukan)
            {
                return await ProcessPermohonan(
                    data,
                    _helper.Verifikator(),
                    PermohonanStatus.Ditolak,
                    SendEmailPermohonanDitolakAsync);
            }

            return await ProcessPermohonan(
                data,
                _helper.Verifikator(),
                PermohonanStatus.DikembalikanVerifikator,
                SendEmailPermohonanDikembalikanAsync);
        }

        /// <summary>
        /// Accept an existing Permohonan by Kepala Seksi.
        /// </summary>
        /// <remarks>
        /// *Role: Kasi*
        /// </remarks>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Supervisor)]
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
        [MultiRoleAuthorize(ApiRole.Supervisor)]
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
        [MultiRoleAuthorize(ApiRole.Timja)]
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
        [MultiRoleAuthorize(ApiRole.Timja)]
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
        [MultiRoleAuthorize(ApiRole.Dirpenyanfar)]
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
        [MultiRoleAuthorize(ApiRole.Dirpenyanfar)]
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
        /// Finish an existing Permohonan by Direktur Jenderal.
        /// </summary>
        /// <remarks>
        /// *Role: Dirjen*
        /// </remarks>
        /// <param name="options">Injected Perizinan configuration options.</param>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Dirjen)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> DirekturJenderalSelesaikan(
            [FromServices] IOptions<PerizinanOptions> options,
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id);

            return await SelesaikanPermohonan(options, data, update);
        }

        /// <summary>
        /// Finish an existing Permohonan by Validator.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <param name="options">Injected Perizinan configuration options.</param>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> ValidatorSelesaikan(
            [FromServices] IOptions<PerizinanOptions> options,
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.StatusId == PermohonanStatus.DisetujuiDirekturJenderal.Id);

            return await SelesaikanPermohonan(options, data, update);
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
            var result = _helper.GenerateAndSignPdf(
                new TandaDaftarHelper(_environment, HttpContext, Url, _signatureOptions),
                await _ossHelper.RetrieveInfo(pemohon.Nib),
                pemohon,
                permohonan,
                perizinan);

            if (!result.SignResult.IsSuccess)
            {
                return BadRequest(result.SignResult);
            }

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
            ApiRole.Supervisor,
            ApiRole.Timja,
            ApiRole.Dirpenyanfar,
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
        [MultiRoleAuthorize(ApiRole.Supervisor)]
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
        [MultiRoleAuthorize(ApiRole.Timja)]
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
        [MultiRoleAuthorize(ApiRole.Dirpenyanfar)]
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
        [MultiRoleAuthorize(ApiRole.Supervisor)]
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
        [MultiRoleAuthorize(ApiRole.Timja)]
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
        [MultiRoleAuthorize(ApiRole.Dirpenyanfar)]
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
            ApiRole.Supervisor,
            ApiRole.Timja,
            ApiRole.Dirpenyanfar,
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
            Func<PemohonUserInfo, Task> delegateAction = null)
        {
            Permohonan update = await query
                .FirstOrDefaultAsync(c => c.Id == data.PermohonanId);

            if (update == null)
            {
                return NotFound();
            }

            if (status.Id == PermohonanStatus.Ditolak.Id)
            {
                Pemohon pemohon = await _context.Pemohon
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == update.PemohonId);
                OssFullInfo ossInfo = await _ossHelper.RetrieveInfo(pemohon.Nib);
                OssIzinFinal izinFinal = new OssIzinFinal();
                OssSendLicenseResponse response = await _ossHelper.UpdateLicenseAsync(
                    izinFinal,
                    ossInfo,
                    pemohon,
                    update,
                    OssInfoHelper.StatusIzin.Ditolak);

                // Status 422 is for pre-OSS request
                if (!response.IsSuccess && response.StatusCode != Status422UnprocessableEntity)
                {
                    return BadRequest();
                }
            }

            update.StatusId = status.Id;
            _context.HistoryPermohonan.Add(_helper.CreateHistory(update, data, HttpContext));

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            if (delegateAction != null)
            {
                await delegateAction(await _pemohonHelper.Retrieve((uint)update.PemohonId, HttpContext));
            }

            return NoContent();
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
                DateTime.Today :
                totalTimeData.UpdatedAt.Date;
            DateTime userStartDate = userTimeData?.UpdatedAt == null ?
                DateTime.Today :
                userTimeData.UpdatedAt.Date;

            return new PermohonanPemohon
            {
                PermohonanId = permohonan.Id,
                SubmittedAt = permohonan.SubmittedAt,
                PermohonanNumber = permohonan.PermohonanNumber,
                Domain = permohonan.Domain,
                StatusName = permohonan.StatusName,
                TypeName = permohonan.TypeName,
                LastUpdate = permohonan.LastUpdate,
                CompanyName = pemohon?.CompanyName,
                PenanggungJawab = pemohon?.PenanggungJawab,
                Email = pemohon?.Email,
                Name = pemohon?.Name,
                Nib = pemohon?.Nib,
                TotalDays = _calculator.GetWorkingDays(totalStartDate, DateTime.Today),
                UserLevelDays = _calculator.GetWorkingDays(userStartDate, DateTime.Today)
            };
        }
        private bool Exists(uint id)
        {
            return _context.Permohonan.Any(e => e.Id == id);
        }
        private string MonthToRomanNumber(DateTime date)
        {
            return RomanNumberHelper.ToRomanNumber(date.Month);
        }
        private async Task<IActionResult> SelesaikanPermohonan(
            IOptions<PerizinanOptions> options,
            PermohonanSystemUpdate data,
            Permohonan update)
        {
            if (update == null)
            {
                return NotFound();
            }

            DateTime maxExpiry = DateTime.Today.AddYears(options.Value.ExpiryInYears);
            DateTime expiry = maxExpiry;
            Perizinan perizinan;

            // PerizinanId == null -> never generated before, PerizinanId != null -> regenerated
            if (update.PerizinanId == null)
            {
                perizinan = new Perizinan
                {
                    PermohonanId = update.Id,
                    PreviousId = update.PreviousPerizinanId
                };

                CounterHelper counterHelper = new CounterHelper(_context);
                perizinan.PerizinanNumber = await counterHelper.GetFormNumber(CounterType.Perizinan);
            }
            else
            {
                perizinan = await _context.Perizinan
                    .Where(c => c.PermohonanId == update.Id)
                    .FirstOrDefaultAsync();
            }

            perizinan.ExpiredAt = expiry;
            perizinan.IssuedAt = DateTime.Today;

            Pemohon pemohon = await _context.Pemohon
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == update.PemohonId);
            OssFullInfo ossInfo = await _ossHelper.RetrieveInfo(pemohon.Nib);
            OssIzinFinal izinFinal = new OssIzinFinal
            {
                TglTerbitIzin = perizinan.IssuedAt.ToString("yyyy-MM-dd"),
                TglBerlakuIzin = perizinan.ExpiredAt.ToString("yyyy-MM-dd"),
            };
            OssSendLicenseResponse getIzinNumberResponse = await _ossHelper.UpdateLicenseAsync(
                izinFinal,
                ossInfo,
                pemohon,
                update,
                OssInfoHelper.StatusIzin.Disetujui);

            izinFinal = getIzinNumberResponse.IzinFinal;
            izinFinal.NomorIzin = perizinan.PerizinanNumber = getIzinNumberResponse.LicenseNumber;
            GeneratePdfResult result = _helper.GenerateAndSignPdf(
                new TandaDaftarHelper(_environment, HttpContext, Url, _signatureOptions),
                ossInfo,
                pemohon,
                update,
                perizinan);

            if (!getIzinNumberResponse.IsSuccess || !result.SignResult.IsSuccess)
            {
                perizinan.ExpiredAt = _invalidPerizinan;
                perizinan.IssuedAt = _invalidPerizinan;
            }

            if (result.SignResult.IsSuccess)
            {
                perizinan.TandaDaftarUrl = result.FullPath;
            }

            bool sendLicenseSuccess = true;

            if (getIzinNumberResponse.IsSuccess)
            {
                izinFinal.FileIzin = izinFinal.FileLampiran =
                    $"https://{HttpContext.Request.Host.Value}{result.FullPath}";

                OssSendLicenseResponse sendIzinResponse = await _ossHelper.SendLicense(izinFinal);
                sendLicenseSuccess = sendIzinResponse.IsSuccess;
            }

            if (update.PerizinanId == null)
            {
                _context.Perizinan.Add(perizinan);
            }
            else
            {
                _context.Entry(perizinan).State = EntityState.Modified;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            update.PerizinanId = perizinan.Id;
            _context.Entry(update).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            if (!getIzinNumberResponse.IsSuccess || !sendLicenseSuccess || !result.SignResult.IsSuccess)
            {
                return BadRequest($"OSS Izin Number: {getIzinNumberResponse.IsSuccess}, OSS Send License: {sendLicenseSuccess}, ESign: {result.SignResult.IsSuccess}");
            }

            update.StatusId = PermohonanStatus.Selesai.Id;
            _context.Entry(update).State = EntityState.Modified;
            _context.HistoryPermohonan.Add(_helper.CreateHistory(update, data, HttpContext));

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            await SendEmailPerizinanDiterbitkanAsync(
                await _pemohonHelper.Retrieve((uint)update.PemohonId, HttpContext));
            return NoContent();
        }
        private async Task SendEmailPermohonanDikembalikanAsync(PemohonUserInfo info)
        {
            await _smtpEmailService.SendEmailAsync(
                new MailAddress(info.Email, info.CompanyName),
                _defaultCc,
                "Permohonan Dikembalikan",
                "Permohonan anda dikembalikan, silahkan login ke dalam aplikasi PSEF untuk melihatnya.");
        }
        private async Task SendEmailPermohonanDitolakAsync(PemohonUserInfo info)
        {
            await _smtpEmailService.SendEmailAsync(
                new MailAddress(info.Email, info.CompanyName),
                _defaultCc,
                "Permohonan Ditolak",
                "Mohon maaf, Permohonan anda ditolak, silahkan login ke dalam aplikasi PSEF untuk melihatnya.");
        }
        private async Task SendEmailPerizinanDiterbitkanAsync(PemohonUserInfo info)
        {
            await _smtpEmailService.SendEmailAsync(
                new MailAddress(info.Email, info.CompanyName),
                _defaultCc,
                "Perizinan Telah Terbit",
                "Permohonan anda telah diterbitkan perizinannya, silahkan login ke dalam aplikasi PSEF untuk melihatnya.");
        }
        private async Task SendEmailPerizinanHampirKadaluarsaAsync(PemohonUserInfo info)
        {
            await _smtpEmailService.SendEmailAsync(
                new MailAddress(info.Email, info.CompanyName),
                _defaultCc,
                "Perizinan Hampir Habis",
                "Perizinan anda sudah hampir habis masa berlakunya, mohon persiapkan untuk mengurus perpanjangannya.");
        }

        private readonly static Calculator _calculator = new Calculator();
        private readonly MailAddressCollection _defaultCc;
        private readonly PemohonUserInfoHelper _pemohonHelper;
        private readonly PermohonanHelper _helper;
        private readonly OssInfoHelper _ossHelper;
        private readonly PsefMySqlContext _context;
        private readonly SmtpEmailService _smtpEmailService;
        private readonly IWebHostEnvironment _environment;
        private readonly IOptions<ElectronicSignatureOptions> _signatureOptions;
        private readonly DateTime _invalidPerizinan = new DateTime(1901, 1, 1);
        private const int _maxPermohonanDiajukan = 3;
    }
}
