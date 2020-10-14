using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
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
    /// Represents a RESTful service of Dashboard Info.
    /// </summary>
    [Authorize]
    [ApiVersion(V0_1)]
    public class DashboardInfoController : ODataController
    {
        /// <summary>
        /// Permohonan REST service.
        /// </summary>
        /// <param name="context">Database context.</param>
        public DashboardInfoController(
            PsefMySqlContext context)
        {
            _context = context;
            _helper = new PermohonanHelper(context);
        }

        /// <summary>
        /// Retrieves Verifikator Dashboard Info.
        /// </summary>
        /// <remarks>
        /// *Role: Verifikator*
        /// </remarks>
        /// <returns>Verifikator Dashboard Info.</returns>
        /// <response code="200">Verifikator Dashboard Info.</response>
        [MultiRoleAuthorize(ApiRole.Verifikator)]
        [HttpGet]
        [ODataRoute(nameof(Verifikator))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DashboardInfo), Status200OK)]
        public async Task<ActionResult> Verifikator()
        {
            return Ok(new DashboardInfo
            {
                TotalPemohon = await TotalPemohon(),
                TotalPermohonanPending = await _helper.Verifikator().LongCountAsync(),
                TotalPermohonan = await TotalPermohonan(),
                TotalPerizinan = await TotalPerizinan(),
                Aktifitas = await Aktifitas()
            });
        }

        /// <summary>
        /// Retrieves Kepala Seksi Dashboard Info.
        /// </summary>
        /// <remarks>
        /// *Role: Kasi*
        /// </remarks>
        /// <returns>Kepala Seksi Dashboard Info.</returns>
        /// <response code="200">Kepala Seksi Dashboard Info.</response>
        [MultiRoleAuthorize(ApiRole.Kasi)]
        [HttpGet]
        [ODataRoute(nameof(KepalaSeksi))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DashboardInfo), Status200OK)]
        public async Task<ActionResult> KepalaSeksi()
        {
            return Ok(new DashboardInfo
            {
                TotalPemohon = await TotalPemohon(),
                TotalPermohonanPending = await _helper.Kasi().LongCountAsync(),
                TotalPermohonan = await TotalPermohonan(),
                TotalPerizinan = await TotalPerizinan(),
                Aktifitas = await Aktifitas()
            });
        }

        /// <summary>
        /// Retrieves Kepala Sub Direktorat Dashboard Info.
        /// </summary>
        /// <remarks>
        /// *Role: Kasubdit*
        /// </remarks>
        /// <returns>Kepala Sub Direktorat Dashboard Info.</returns>
        /// <response code="200">Kepala Sub Direktorat Dashboard Info.</response>
        [MultiRoleAuthorize(ApiRole.Kasubdit)]
        [HttpGet]
        [ODataRoute(nameof(KepalaSubDirektorat))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DashboardInfo), Status200OK)]
        public async Task<ActionResult> KepalaSubDirektorat()
        {
            return Ok(new DashboardInfo
            {
                TotalPemohon = await TotalPemohon(),
                TotalPermohonanPending = await _helper.Kasubdit().LongCountAsync(),
                TotalPermohonan = await TotalPermohonan(),
                TotalPerizinan = await TotalPerizinan(),
                Aktifitas = await Aktifitas()
            });
        }

        /// <summary>
        /// Retrieves Direktur Pelayanan Farmasi Dashboard Info.
        /// </summary>
        /// <remarks>
        /// *Role: Diryanfar*
        /// </remarks>
        /// <returns>Direktur Pelayanan Farmasi Dashboard Info.</returns>
        /// <response code="200">Direktur Pelayanan Farmasi Dashboard Info.</response>
        [MultiRoleAuthorize(ApiRole.Diryanfar)]
        [HttpGet]
        [ODataRoute(nameof(DirekturPelayananFarmasi))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DashboardInfo), Status200OK)]
        public async Task<ActionResult> DirekturPelayananFarmasi()
        {
            return Ok(new DashboardInfo
            {
                TotalPemohon = await TotalPemohon(),
                TotalPermohonanPending = await _helper.Diryanfar().LongCountAsync(),
                TotalPermohonan = await TotalPermohonan(),
                TotalPerizinan = await TotalPerizinan(),
                Aktifitas = await Aktifitas()
            });
        }

        /// <summary>
        /// Retrieves Direktur Jenderal Dashboard Info.
        /// </summary>
        /// <remarks>
        /// *Role: Dirjen*
        /// </remarks>
        /// <returns>Direktur Jenderal Dashboard Info.</returns>
        /// <response code="200">Direktur Jenderal Dashboard Info.</response>
        [MultiRoleAuthorize(ApiRole.Dirjen)]
        [HttpGet]
        [ODataRoute(nameof(DirekturJenderal))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DashboardInfo), Status200OK)]
        public async Task<ActionResult> DirekturJenderal()
        {
            return Ok(new DashboardInfo
            {
                TotalPemohon = await TotalPemohon(),
                TotalPermohonanPending = await _helper.Dirjen().LongCountAsync(),
                TotalPermohonan = await TotalPermohonan(),
                TotalPerizinan = await TotalPerizinan(),
                Aktifitas = await Aktifitas()
            });
        }

        /// <summary>
        /// Retrieves Validator Sertifikat Dashboard Info.
        /// </summary>
        /// <remarks>
        /// *Role: Validator*
        /// </remarks>
        /// <returns>Validator Sertifikat Dashboard Info.</returns>
        /// <response code="200">Validator Sertifikat Dashboard Info.</response>
        [MultiRoleAuthorize(ApiRole.Validator)]
        [HttpGet]
        [ODataRoute(nameof(ValidatorSertifikat))]
        [Produces(JsonOutput)]
        [ProducesResponseType(typeof(DashboardInfo), Status200OK)]
        public async Task<ActionResult> ValidatorSertifikat()
        {
            return Ok(new DashboardInfo
            {
                TotalPemohon = await TotalPemohon(),
                TotalPermohonanPending = await _helper.Validator().LongCountAsync(),
                TotalPermohonan = await TotalPermohonan(),
                TotalPerizinan = await TotalPerizinan(),
                Aktifitas = await Aktifitas()
            });
        }

        private async Task<long> TotalPemohon()
        {
            return await _context.Pemohon.LongCountAsync();
        }
        private async Task<long> TotalPermohonan()
        {
            return await _context.Permohonan.LongCountAsync();
        }
        private async Task<long> TotalPerizinan()
        {
            return await _context.Perizinan.LongCountAsync();
        }
        private async Task<List<Aktifitas>> Aktifitas()
        {
            List<HistoryPermohonan> historyList = await _context.HistoryPermohonan
                .Include(c => c.Permohonan)
                .Where(c => c.UpdatedBy == ApiHelper.GetUserName(HttpContext.User))
                .OrderByDescending(c => c.UpdatedAt)
                .Take(_aktifitasCount)
                .ToListAsync();
            List<Aktifitas> result = new List<Aktifitas>();

            foreach (HistoryPermohonan history in historyList)
            {
                result.Add(new Aktifitas
                {
                    UserName = history.UpdatedBy,
                    Date = history.UpdatedAt,
                    Action = history.StatusName,
                    Item = history.Permohonan?.PermohonanNumber,
                    ItemId = history.PermohonanId
                });
            }

            return result;
        }

        private const int _aktifitasCount = 20;
        private readonly PsefMySqlContext _context;
        private readonly PermohonanHelper _helper;
    }
}