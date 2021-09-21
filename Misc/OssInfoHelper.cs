using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
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
        /// <param name="options">OSS API configuration options.</param>
        public OssInfoHelper(
            IOssApiService ossApi,
            IMemoryCache memoryCache,
            IOptions<OssApiOptions> options)
        {
            _ossApi = ossApi;
            _memoryCache = memoryCache;
            _options = options;
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

            OssFullInfo dummyData = _dummyInfoList.FirstOrDefault(c => c.Nib == id);

            if (dummyData != null)
            {
                return dummyData;
            }

            if (_memoryCache.TryGetValue(
                nameof(OssFullInfo) + id,
                out OssFullInfo cachedInfo))
            {
                return cachedInfo;
            }

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("nib", id);

            string token = await _ossApi.Authenticate();

            if (token == null)
            {
                return _invalidCredentialInfo;
            }

            JObject response = await _ossApi.CallApiAsync(
                token,
                "/api/inquery/nib/",
                new FormUrlEncodedContent(formData));

            if (response == null)
            {
                return _connectionErrorInfo;
            }

            int status = response["responinqueryNIB"]["kode"].ToObject<int>();

            if (status != Status200OK)
            {
                return new OssFullInfo
                {
                    Nib = string.Empty,
                    NamaPerseroan = string.Empty,
                    AlamatPerseroan = string.Empty,
                    NpwpPerseroan = string.Empty,
                    NamaUserProses = string.Empty,
                    Keterangan = response["responinqueryNIB"]["keterangan"].ToObject<string>()
                };
            }

            OssFullInfo apiData = response["responinqueryNIB"]["dataNIB"]
                .ToObject<OssFullInfo>(JsonSerializer.CreateDefault(_snakeSettings));

            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(new TimeSpan(_options.Value.CacheHour, 0, 0));

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
        private static readonly OssPemegangSaham _pemegangSaham = new OssPemegangSaham
        {
            NamaPemegangSaham = "Badu",
            AlamatPemegangSaham = "Jl. Kemana Saja No. 987",
            NpwpPemegangSaham = "987654320123000",
            EmailPemegangSaham = "badu@fiktif.com",
            FaxPemegangSaham = "021-56781234"
        };
        private static readonly OssPenanggungJawab _penanggungJawab = new OssPenanggungJawab
        {
            AlamatPenanggungJwb = "Jl. Kemana Saja No. 987",
            BlokPenanggungJwb = "A",
            DaerahIdPenanggungJwb = string.Empty,
            EmailPenanggungJwb = "badu@fiktif.com",
            FlagAsing = "N",
            FlagPajakPenanggungJwb = string.Empty,
            JabatanPenanggungJwb = "Direktur",
            JalanPenanggungJwb = "Jl. Kemana Saja",
            JnsIdentitasPenanggungJwb = "KTP",
            KebangsaanPenanggungJwb = "WNI",
            KelurahanPenanggungJwb = "Kelurahan X",
            KetPajakPenanggungJwb = string.Empty,
            KodePosPenanggungJwb = "12345",
            NamaPenanggungJwb = "Badu"
        };
        private static readonly OssFullInfo _dummyInfo = new OssFullInfo
        {
            Keterangan = "***Data dummy untuk test***",
            Nib = "0000000000000",
            NamaPerseroan = "PT. Maju Jaya Farmatek",
            AlamatPerseroan = "Jl. Sejahtera Selalu No. 123",
            RtRwPerseroan = "00/00",
            KelurahanPerseroan = "Kelurahan",
            KodePosPerseroan = "12345",
            NpwpPerseroan = "123456780123000",
            NomorTelponPerseroan = "021-56789012",
            PemegangSaham = new List<OssPemegangSaham>
            {
                _pemegangSaham
            },
            PenanggungJwb = new List<OssPenanggungJawab>
            {
                _penanggungJawab
            },
            Legalitas = new List<OssLegalitas>
            {
                new OssLegalitas()
            },
            DataRptka = new OssRptka
            {
                RptkaJabatan = new List<OssRptkaJabatan>
                {
                    new OssRptkaJabatan
                    {
                        RptkaTkiPendamping = new List<OssRptkaTkiPendamping>
                        {
                            new OssRptkaTkiPendamping()
                        }
                    }
                },
                RptkaNegara = new List<OssRptkaNegara>
                {
                    new OssRptkaNegara()
                },
                RptkaLokasi = new List<OssRptkaLokasi>
                {
                    new OssRptkaLokasi()
                }
            },
            DataProyek = new List<OssProyek>
            {
                new OssProyek
                {
                    DataLokasiProyek = new List<OssProyekLokasi>
                    {
                        new OssProyekLokasi
                        {
                            DataLokasiProyek = new List<OssProyekLokasiLokasi>
                            {
                                new OssProyekLokasiLokasi()
                            },
                            DataPosisiProyek = new List<OssProyekPosisi>
                            {
                                new OssProyekPosisi()
                            }
                        }
                    },
                    DataProyekProduk = new List<OssProyekProduk>
                    {
                        new OssProyekProduk()
                    }
                }
            },
            DataDni = new List<OssDni>
            {
                new OssDni()
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
        private static readonly OssFullInfo _dummyInfoB = new OssFullInfo
        {
            Keterangan = "***Data dummy untuk test***",
            Nib = "8120000123456",
            NamaPerseroan = "PT. Maju Jaya Farmatek",
            AlamatPerseroan = "Jl. Sejahtera Selalu No. 123",
            RtRwPerseroan = "00/00",
            KelurahanPerseroan = "Kelurahan",
            KodePosPerseroan = "12345",
            NpwpPerseroan = "123456780123000",
            NomorTelponPerseroan = "021-56789012",
            PemegangSaham = new List<OssPemegangSaham>
            {
                _pemegangSaham
            },
            PenanggungJwb = new List<OssPenanggungJawab>
            {
                _penanggungJawab
            },
            Legalitas = new List<OssLegalitas>
            {
                new OssLegalitas()
            },
            DataRptka = new OssRptka
            {
                RptkaJabatan = new List<OssRptkaJabatan>
                {
                    new OssRptkaJabatan
                    {
                        RptkaTkiPendamping = new List<OssRptkaTkiPendamping>
                        {
                            new OssRptkaTkiPendamping()
                        }
                    }
                },
                RptkaNegara = new List<OssRptkaNegara>
                {
                    new OssRptkaNegara()
                },
                RptkaLokasi = new List<OssRptkaLokasi>
                {
                    new OssRptkaLokasi()
                }
            },
            DataProyek = new List<OssProyek>
            {
                new OssProyek
                {
                    DataLokasiProyek = new List<OssProyekLokasi>
                    {
                        new OssProyekLokasi
                        {
                            DataLokasiProyek = new List<OssProyekLokasiLokasi>
                            {
                                new OssProyekLokasiLokasi()
                            },
                            DataPosisiProyek = new List<OssProyekPosisi>
                            {
                                new OssProyekPosisi()
                            }
                        }
                    },
                    DataProyekProduk = new List<OssProyekProduk>
                    {
                        new OssProyekProduk()
                    }
                }
            },
            DataDni = new List<OssDni>
            {
                new OssDni()
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
        private static readonly List<OssFullInfo> _dummyInfoList = new List<OssFullInfo>
        {
            _dummyInfo,
            _dummyInfoB
        };
        private static readonly OssFullInfo _invalidCredentialInfo = new OssFullInfo
        {
            Keterangan = "Gagal melakukan login ke API OSS."
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
        private readonly IOptions<OssApiOptions> _options;
    }
}