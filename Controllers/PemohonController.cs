using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using PsefApiOData.Misc;
using PsefApiOData.Models;
using static Microsoft.AspNetCore.Http.StatusCodes;
using static PsefApiOData.ApiInfo;

namespace PsefApiOData.Controllers
{
    /// <summary>
    /// Represents a RESTful service of Pemohon.
    /// </summary>
    [Authorize]
    [ApiVersion(V0_1)]
    [ODataRoutePrefix(nameof(Pemohon))]
    public class PemohonController : ODataController
    {
        /// <summary>
        /// Pemohon REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="mapper">AutoMapper mapping profile.</param>
        /// <param name="delegateService">Api delegation service.</param>
        /// <param name="identityApi">Identity Api service.</param>
        /// <param name="ossApi">Oss Api service.</param>
        /// <param name="memoryCache">Memory cache.</param>
        /// <param name="options">OSS API configuration options.</param>
        public PemohonController(
            PsefMySqlContext context,
            IMapper mapper,
            IApiDelegateService delegateService,
            IIdentityApiService identityApi,
            IOssApiService ossApi,
            IMemoryCache memoryCache,
            IOptions<OssApiOptions> options)
        {
            _ossApi = ossApi;
            _memoryCache = memoryCache;
            _options = options;
            _context = context;
            _mapper = mapper;
            _pemohonHelper = new PemohonUserInfoHelper(context, delegateService, identityApi);
        }

        /// <summary>
        /// Retrieves Pemohon total count.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>Pemohon total count.</returns>
        /// <response code="200">Total count of Pemohon retrieved.</response>
        [HttpGet]
        [ODataRoute(nameof(TotalCount))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(long), Status200OK)]
        public async Task<long> TotalCount()
        {
            return await _context.Pemohon.LongCountAsync();
        }

        /// <summary>
        /// Retrieves all Pemohon.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <returns>All available Pemohon.</returns>
        /// <response code="200">Pemohon successfully retrieved.</response>
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
        [ProducesResponseType(typeof(ODataValue<IEnumerable<PemohonUserInfo>>), Status200OK)]
        [EnableQuery]
        public async Task<IQueryable<PemohonUserInfo>> Get()
        {
            List<PemohonUserInfo> result = await _pemohonHelper.RetrieveList(HttpContext);
            return result.AsQueryable();
        }

        /// <summary>
        /// Gets a single Pemohon.
        /// </summary>
        /// <remarks>
        /// *Min role: Verifikator*
        /// </remarks>
        /// <param name="id">The requested Pemohon identifier.</param>
        /// <returns>The requested Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully retrieved.</response>
        /// <response code="404">The Pemohon does not exist.</response>
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
        [ProducesResponseType(typeof(PemohonUserInfo), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public async Task<SingleResult<PemohonUserInfo>> Get([FromODataUri] uint id)
        {
            List<PemohonUserInfo> result = new List<PemohonUserInfo>();
            PemohonUserInfo pemohon = await _pemohonHelper.Retrieve(id, HttpContext);

            if (pemohon != null)
            {
                result.Add(pemohon);
            }

            return SingleResult.Create(result.AsQueryable());
        }

        /// <summary>
        /// Creates a new Pemohon.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="create">The Pemohon to create.</param>
        /// <returns>The created Pemohon.</returns>
        /// <response code="201">The Pemohon was successfully created.</response>
        /// <response code="204">The Pemohon was successfully created.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="409">The Pemohon with supplied id already exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
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

            if (await _context.Pemohon.FirstOrDefaultAsync(e => e.UserId == create.UserId) != null)
            {
                return Conflict(create.UserId);
            }

            if (!await CheckNibAndUpdatePemohon(create))
            {
                return InvalidNib();
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
                    return Conflict(create.Id);
                }

                throw;
            }

            return Created(create);
        }

        /// <summary>
        /// Updates an existing Pemohon.
        /// </summary>
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The requested Pemohon identifier.</param>
        /// <param name="delta">The partial Pemohon to update.</param>
        /// <returns>The updated Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully updated.</response>
        /// <response code="204">The Pemohon was successfully updated.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        /// <response code="422">The Pemohon identifier is specified on delta and its value is different from id.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
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

            if (!await CheckNibAndUpdatePemohon(update))
            {
                return InvalidNib();
            }

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
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The Pemohon to delete.</param>
        /// <returns>None</returns>
        /// <response code="204">The Pemohon was successfully deleted.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
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
        /// <remarks>
        /// *Min role: Admin*
        /// </remarks>
        /// <param name="id">The requested Pemohon identifier.</param>
        /// <param name="update">The Pemohon to update.</param>
        /// <returns>The updated Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully updated.</response>
        /// <response code="204">The Pemohon was successfully updated.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [MultiRoleAuthorize(
            ApiRole.Admin,
            ApiRole.SuperAdmin)]
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

            if (!await CheckNibAndUpdatePemohon(update))
            {
                return InvalidNib();
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
        /// Gets a single Pemohon for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>The requested Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully retrieved.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [HttpGet]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(PemohonUserInfo), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public async Task<SingleResult<PemohonUserInfo>> CurrentUserInfo()
        {
            List<PemohonUserInfo> result = new List<PemohonUserInfo>();
            PemohonUserInfo pemohon = await _pemohonHelper.RetrieveCurrentUser(HttpContext);

            if (pemohon != null)
            {
                result.Add(pemohon);
            }

            return SingleResult.Create(result.AsQueryable());
        }

        /// <summary>
        /// Gets a single Pemohon for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>The requested Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully retrieved.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        [ODataRoute(CurrentUser)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status200OK)]
        [ProducesResponseType(Status404NotFound)]
        [EnableQuery(AllowedQueryOptions = AllowedQueryOptions.Select)]
        public SingleResult<Pemohon> GetCurrentUser()
        {
            return SingleResult.Create(
                _context.Pemohon.Where(e => e.UserId == ApiHelper.GetUserId(HttpContext.User)));
        }

        /// <summary>
        /// Creates a new Pemohon for the current user.
        /// </summary>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <param name="create">The Pemohon to create.</param>
        /// <returns>The created Pemohon.</returns>
        /// <response code="201">The Pemohon was successfully created.</response>
        /// <response code="204">The Pemohon was successfully created.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="409">The Pemohon with supplied id already exist.</response>
        [ODataRoute(CurrentUser)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status201Created)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status409Conflict)]
        public async Task<IActionResult> PostCurrentUser([FromBody] Pemohon create)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string userId = ApiHelper.GetUserId(HttpContext.User);

            if (await _context.Pemohon.FirstOrDefaultAsync(e => e.UserId == userId) != null)
            {
                return Conflict(userId);
            }

            if (!await CheckNibAndUpdatePemohon(create))
            {
                return InvalidNib();
            }

            create.UserId = userId;
            _context.Pemohon.Add(create);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (Exists(create.Id))
                {
                    return Conflict(create.Id);
                }

                throw;
            }

            return Created(create);
        }

        /// <summary>
        /// Updates an existing Pemohon for the current user.
        /// </summary>
        /// <param name="delta">The partial Pemohon to update.</param>
        /// <remarks>
        /// *Min role: None*
        /// </remarks>
        /// <returns>The updated Pemohon.</returns>
        /// <response code="200">The Pemohon was successfully updated.</response>
        /// <response code="204">The Pemohon was successfully updated.</response>
        /// <response code="400">The Pemohon is invalid.</response>
        /// <response code="404">The Pemohon does not exist.</response>
        /// <response code="422">The Pemohon identifier is specified on delta and its value is different from id.</response>
        [ODataRoute(CurrentUser)]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(Pemohon), Status200OK)]
        [ProducesResponseType(Status204NoContent)]
        [ProducesResponseType(Status400BadRequest)]
        [ProducesResponseType(Status404NotFound)]
        [ProducesResponseType(Status422UnprocessableEntity)]
        public async Task<IActionResult> PatchCurrentUser([FromBody] Delta<Pemohon> delta)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string currentUserId = ApiHelper.GetUserId(HttpContext.User);
            Pemohon update = await _context.Pemohon
                .FirstOrDefaultAsync(c => c.UserId == currentUserId);

            if (update == null)
            {
                return NotFound();
            }

            var oldId = update.Id;
            delta.Patch(update);

            if (update.UserId != currentUserId)
            {
                return BadRequest(currentUserId);
            }

            if (!await CheckNibAndUpdatePemohon(update))
            {
                return InvalidNib();
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

        private async Task<bool> CheckNibAndUpdatePemohon(Pemohon data)
        {
            OssInfoHelper ossInfoHelper = new OssInfoHelper(_ossApi, _memoryCache, _options);
            OssFullInfo ossFullInfo = await ossInfoHelper.RetrieveInfo(data.Nib);

            if (string.IsNullOrEmpty(ossFullInfo.Nib))
            {
                return false;
            }

            string nama = string.Empty;
            OssPenanggungJawab penanggungJawab = ossFullInfo.PenanggungJwb
                .FirstOrDefault(c => c.JabatanPenanggungJwb.ToUpperInvariant() == "DIREKTUR UTAMA");

            if (penanggungJawab == null)
            {
                penanggungJawab = ossFullInfo.PenanggungJwb
                    .FirstOrDefault();
            }

            if (penanggungJawab != null && penanggungJawab.NamaPenanggungJwb != null)
            {
                nama = penanggungJawab.NamaPenanggungJwb;
            }

            data.CompanyName = ossFullInfo.NamaPerseroan;
            data.PenanggungJawab = nama;
            return true;
        }
        private IActionResult InvalidNib()
        {
            ModelState.AddModelError(nameof(Pemohon.Nib), "NIB not found");
            return BadRequest(ModelState);
        }
        private bool Exists(uint id)
        {
            return _context.Pemohon.Any(e => e.Id == id);
        }

        private readonly PemohonUserInfoHelper _pemohonHelper;
        private readonly PsefMySqlContext _context;
        private readonly IMapper _mapper;
        private readonly IOssApiService _ossApi;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptions<OssApiOptions> _options;
    }
}
