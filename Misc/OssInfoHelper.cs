using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using PsefApiOData.Models;
using static Microsoft.AspNetCore.Http.StatusCodes;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Oss Information helpers.
    /// </summary>
    public class OssInfoHelper
    {
        /// <summary>
        /// Oss Information helpers.
        /// </summary>
        /// <param name="ossApi">OSS Api service.</param>
        /// <param name="memoryCache">Memory cache.</param>
        public OssInfoHelper(IOssApiService ossApi, IMemoryCache memoryCache)
        {
            _ossApi = ossApi;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets a single OSS Full Information.
        /// </summary>
        /// <param name="id">The requested OSS Full Information identifier.</param>
        /// <returns>The requested OSS Full Information.</returns>
        public async Task<OssFullInfo> RetrieveInfo(string id)
        {
            id = id.Trim('\'');

            if (id.Length != 13)
            {
                return _invalidNibInfo;
            }

            if (id == "0000000000000")
            {
                return _dummyInfo;
            }

            if (_memoryCache.TryGetValue(
                nameof(OssFullInfo) + id,
                out OssFullInfo cachedInfo))
            {
                return cachedInfo;
            }

            var content = new
            {
                INQUERYNIB = new
                {
                    nib = id
                }
            };

            JObject response = await _ossApi.CallApiAsync(
                await _ossApi.Authenticate(),
                "/KEMKES_inqueryNIB",
                JsonConvert.SerializeObject(content));

            if (response == null)
            {
                return _connectionErrorInfo;
            }

            int status = response["responinqueryNIB"]["kode"].ToObject<int>();

            if (status != Status200OK)
            {
                return new OssFullInfo
                {
                    Keterangan = response["responinqueryNIB"]["keterangan"].ToObject<string>()
                };
            }

            OssFullInfo apiData = response["responinqueryNIB"]["dataNIB"]
                .ToObject<OssFullInfo>(JsonSerializer.CreateDefault(_snakeSettings));

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(new TimeSpan(ApiHelper.OssCacheHour, 0, 0));

            _memoryCache.Set(nameof(OssFullInfo) + id, apiData, cacheEntryOptions);

            return apiData;
        }

        internal class UnderscorePropertyNamesContractResolver : DefaultContractResolver
        {
            internal UnderscorePropertyNamesContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy();
            }
        }

        private static readonly JsonSerializerSettings _snakeSettings = new JsonSerializerSettings()
        {
            ContractResolver = new UnderscorePropertyNamesContractResolver()
        };

        private static readonly OssFullInfo _dummyInfo = new OssFullInfo
        {
            Keterangan = "***Data dummy untuk test***",
            Nib = "0000000000000",
            NamaPerseroan = "PT. Angin Ribut",
            AlamatPerseroan = "Jl. Buntu Nan Sempit No. 123",
            RtRwPerseroan = "00/00",
            KelurahanPerseroan = "Kelurahan Fiktif",
            KodePosPerseroan = "12345",
            NpwpPerseroan = "123456780123000",
            NomorTelponPerseroan = "021-56789012",
            PemegangSaham = new List<OssPemegangSaham>
            {
                new OssPemegangSaham
                {
                    NamaPemegangSaham = "Badu",
                    AlamatPemegangSaham = "Jl. Kemana Saja No. 987",
                    NpwpPemegangSaham = "987654320123000",
                    EmailPemegangSaham = "badu@fiktif.com"
                }
            },
            PenanggungJwb = new List<OssPenanggungJawab>
            {
                new OssPenanggungJawab()
            },
            Legalitas = new List<OssLegalitas>
            {
                new OssLegalitas()
            },
            DataChecklist = new List<OssChecklist>
            {
                new OssChecklist
                {
                    DataPersyaratan = new List<OssChecklistPersyaratan>
                    {
                        new OssChecklistPersyaratan()
                    }
                }
            }
        };
        private static readonly OssFullInfo _connectionErrorInfo = new OssFullInfo
        {
            Keterangan = "Terdapat masalah dalam koneksi ke API OSS."
        };
        private static readonly OssFullInfo _invalidNibInfo = new OssFullInfo
        {
            Keterangan = "NIB harus 13 karakter."
        };
        private readonly IOssApiService _ossApi;
        private readonly IMemoryCache _memoryCache;
    }
}