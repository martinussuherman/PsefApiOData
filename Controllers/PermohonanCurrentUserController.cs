using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    [ODataRoutePrefix(nameof(Permohonan) + CurrentUser)]
    public class PermohonanCurrentUserController : ODataController
    {
        /// <summary>
        /// Permohonan for current user REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        public PermohonanCurrentUserController(PsefMySqlContext context)
        {
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
                return BadRequest("Pemohon not found");
            }

            // TODO : automatically set type as New or Extension based on whether
            // there is already existing Perizinan (also auto link to that Perizinan)
            create.PermohonanNumber = string.Empty;
            create.PemohonId = pemohon.Id;
            create.StatusId = PermohonanStatus.Dibuat.Id;
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

            HistoryPermohonan createHistory = new HistoryPermohonan
            {
                PermohonanId = create.Id,
                StatusId = PermohonanStatus.Dibuat.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(createHistory);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
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
            var oldPermohonanNumber = update.PermohonanNumber;
            var oldPemohonId = update.PemohonId;
            delta.Patch(update);

            if (update.PemohonId != oldPemohonId)
            {
                return BadRequest(update.PemohonId);
            }

            update.PermohonanNumber = oldPermohonanNumber;

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

        /// <summary>
        /// Submit an existing Permohonan for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="service">SMTP email service.</param>
        /// <param name="options">Permohonan email options</param>
        /// <param name="ossApi">OSS API service</param>
        /// <param name="ossOptions">OSS API options</param>
        /// <param name="delegateService">Api delegation service.</param>
        /// <param name="identityApi">Identity Api service.</param>
        /// <param name="data">Permohonan by system update data.</param>
        /// <returns>None.</returns>
        [HttpPost]
        [Produces(JsonOutput)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        public async Task<IActionResult> Ajukan(
            [FromServices] SmtpEmailService service,
            [FromServices] IOptions<PermohonanEmailOptions> options,
            [FromServices] IOssApiService ossApi,
            [FromServices] IOptions<OssApiOptions> ossOptions,
            [FromServices] IApiDelegateService delegateService,
            [FromServices] IIdentityApiService identityApi,
            [FromBody] PermohonanSystemUpdate data)
        {
            Permohonan update = await _context.Permohonan
                .Include(c => c.Pemohon)
                .FirstOrDefaultAsync(c =>
                    c.Id == data.PermohonanId &&
                    c.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                    (c.StatusId == PermohonanStatus.Dibuat.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanVerifikator.Id));

            if (update == null)
            {
                return NotFound();
            }

            if (update.StatusId == PermohonanStatus.Dibuat.Id)
            {
                CounterHelper counterHelper = new CounterHelper(_context);
                update.PermohonanNumber = await counterHelper.GetFormNumber(CounterType.Permohonan);
            }

            update.StatusId = PermohonanStatus.Diajukan.Id;
            update.SubmittedAt = DateTime.Today;

            HistoryPermohonan submitHistory = new HistoryPermohonan
            {
                PermohonanId = update.Id,
                StatusId = PermohonanStatus.Diajukan.Id,
                UpdatedAt = DateTime.Now,
                UpdatedBy = ApiHelper.GetUserName(HttpContext.User)
            };

            _context.HistoryPermohonan.Add(submitHistory);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }

            PemohonUserInfoHelper helper = new PemohonUserInfoHelper(_context, delegateService, identityApi);
            OssInfoHelper ossHelper = new OssInfoHelper(ossApi, ossOptions);
            await ossHelper.UpdateLicenseStatusAsync(_context, update, OssInfoHelper.StatusIzin.Validasi);
            await SendEmailPermohonanDiajukanAsync(
                service,
                options,
                await helper.Retrieve((uint)update.PemohonId, HttpContext));
            return NoContent();
        }

        /// <summary>
        /// Retrieves all Permohonan for the current user with status Rumusan.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Permohonan for the current user with status Rumusan.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> Rumusan()
        {
            return _context.Permohonan.Where(e =>
                e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                e.StatusId == PermohonanStatus.Dibuat.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan for the current user with status Dikembalikan.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Permohonan for the current user with status Dikembalikan.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> Dikembalikan()
        {
            return _context.Permohonan.Where(e =>
                e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                e.StatusId == PermohonanStatus.DikembalikanVerifikator.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan for the current user with status Progress.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Permohonan for the current user with status Progress.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> Progress()
        {
            return _context.Permohonan.Where(e =>
                e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                e.StatusId != PermohonanStatus.Dibuat.Id &&
                e.StatusId != PermohonanStatus.DikembalikanVerifikator.Id &&
                e.StatusId != PermohonanStatus.Selesai.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan for the current user with status Selesai.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Permohonan for the current user with status Selesai.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> Selesai()
        {
            return _context.Permohonan.Where(e =>
                e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                e.StatusId == PermohonanStatus.Selesai.Id);
        }

        /// <summary>
        /// Retrieves all Permohonan for the current user with status Ditolak.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>All available Permohonan for the current user with status Ditolak.</returns>
        /// <response code="200">Permohonan successfully retrieved.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(ODataValue<IEnumerable<Permohonan>>), Status200OK)]
        [EnableQuery]
        public IQueryable<Permohonan> Ditolak()
        {
            return _context.Permohonan.Where(e =>
                e.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                e.StatusId == PermohonanStatus.Ditolak.Id);
        }

        /// <summary>
        /// Gets a single PermohonanSystemUpdate for the current user Permohonan with status Dikembalikan.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="permohonanId">The requested Permohonan identifier.</param>
        /// <returns>PermohonanSystemUpdate for the current user Permohonan with status Dikembalikan.</returns>
        /// <response code="200">PermohonanSystemUpdate successfully retrieved.</response>
        /// <response code="400">PermohonanSystemUpdate not found.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(PermohonanSystemUpdate), Status200OK)]
        [EnableQuery]
        public async Task<ActionResult> AlasanDikembalikan(uint permohonanId)
        {
            HistoryPermohonan history = await _context.HistoryPermohonan
                .OrderByDescending(e => e.UpdatedAt)
                .FirstOrDefaultAsync(e =>
                    e.PermohonanId == permohonanId &&
                    e.Permohonan.Pemohon.UserId == ApiHelper.GetUserId(HttpContext.User) &&
                    (e.StatusId == PermohonanStatus.DikembalikanVerifikator.Id ||
                    e.StatusId == PermohonanStatus.Ditolak.Id));

            if (history == null)
            {
                return NotFound();
            }

            return Ok(new PermohonanSystemUpdate
            {
                PermohonanId = (uint)history.PermohonanId,
                Reason = history.Reason
            });
        }

        private bool Exists(uint id)
        {
            return _context.Permohonan.Any(e => e.Id == id);
        }
        private async Task SendEmailPermohonanDiajukanAsync(
            SmtpEmailService service,
            IOptions<PermohonanEmailOptions> options,
            PemohonUserInfo info)
        {
            await service.SendEmailAsync(
                new MailAddress(options.Value.To, options.Value.ToDisplay ?? string.Empty),
                new MailAddressCollection(),
                "Permohonan Diajukan",
                $"Pemohon {info.CompanyName} telah mengajukan Permohonan baru, silahkan login ke dalam aplikasi PSEF untuk melihatnya.");
        }

        private readonly PsefMySqlContext _context;
    }
}