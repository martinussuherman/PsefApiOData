using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        /// <param name="options">OSS API configuration options.</param>
        public OssInfoHelper(
            IOssApiService ossApi,
            IOptions<OssApiOptions> options)
        {
            _ossApi = ossApi;
            _options = options;
        }

        /// <summary>
        /// OSS Izin Status Enumeration.
        /// </summary>
        public enum StatusIzin
        {
            /// <summary>
            /// Status Validasi.
            /// </summary>
            Validasi = 20,

            /// <summary>
            /// Status Validasi.
            /// </summary>
            Disetujui = 50,

            /// <summary>
            /// Status Ditolak.
            /// </summary>
            Ditolak = 90
        }

        /// <summary>
        /// Update OSS Izin Status.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="permohonan">The permohonan information.</param>
        /// <param name="status">The izin status to set.</param>
        /// <returns>OSS response.</returns>
        public async Task<JObject> UpdateLicenseStatusAsync(
            PsefMySqlContext context,
            Permohonan permohonan,
            StatusIzin status)
        {
            Pemohon pemohon = await context.Pemohon
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == permohonan.PemohonId);
            OssFullInfo ossInfo = await RetrieveInfo(pemohon.Nib);
            OssChecklist dataChecklist = ossInfo.DataChecklist
                .FirstOrDefault(c => c.IdIzin == permohonan.IdIzin);
            OssDataPnbpIzinStatus dataPnbp = new OssDataPnbpIzinStatus
            {
                KdAkun = string.Empty,
                KdBilling = string.Empty,
                KdPenerimaan = string.Empty,
                Nominal = string.Empty,
                TglBilling = string.Empty,
                TglExpire = string.Empty,
                UrlDokumen = string.Empty
            };
            OssIzinStatus ossData = new OssIzinStatus
            {
                Nib = pemohon.Nib,
                IdProduk = dataChecklist.IdProduk,
                IdProyek = dataChecklist.IdProyek,
                OssId = ossInfo.OssId,
                IdIzin = permohonan.IdIzin,
                KdIzin = _options.Value.KodeIzin,
                KdStatus = ((int)status).ToString(),
                TglStatus = DateTime.Today.ToString("yyyy-MM-dd"),
                DataPnbp = dataPnbp
            };

            return await SendLicenseStatus(ossData);
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

            Dictionary<string, string> formData = new Dictionary<string, string>();
            formData.Add("nib", id);

            string token = await _ossApi.Authenticate();

            if (token == null)
            {
                return _invalidCredentialInfo;
            }

            string uri = _options.Value.IsStaging ? "/api/stagging/inquery/nib/" : "/api/inquery/nib/";

            JObject response = await _ossApi.CallApiAsync(
                token,
                uri,
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

            return apiData;
        }

        /// <summary>
        /// Send izin final data to OSS.
        /// </summary>
        /// <param name="data">The izin final data.</param>
        /// <returns>OSS response.</returns>
        public async Task<JObject> SendLicense(OssIzinFinal data)
        {
            string token = await _ossApi.Authenticate();

            if (token == null)
            {
                return new JObject();
            }

            string uri = _options.Value.IsStaging ?
                "/api/stagging/send/license/" :
                "/api/send/license/";

            var content = new
            {
                IZINFINAL = data
            };

            string contentString = JsonConvert
                .SerializeObject(content, _snakeSettings)
                .Replace("izinfinal", "IZINFINAL");
            StringContent serializedContent = new StringContent(
                contentString,
                Encoding.UTF8,
                "application/json");
            JObject response = await _ossApi.CallApiAsync(token, uri, serializedContent);

            return response;
        }

        /// <summary>
        /// Send perizinan status data to OSS.
        /// </summary>
        /// <returns>OSS response.</returns>
        public async Task<JObject> SendLicenseStatus(OssIzinStatus data)
        {
            string token = await _ossApi.Authenticate();

            if (token == null)
            {
                return new JObject();
            }

            string uri = _options.Value.IsStaging ? "/api/stagging/send/license-status/" : "/api/send/license-status/";

            var content = new
            {
                IZINSTATUS = data
            };

            string contentString = JsonConvert
                .SerializeObject(content, _snakeSettings)
                .Replace("izinstatus", "IZINSTATUS");
            StringContent serializedContent = new StringContent(
                contentString,
                Encoding.UTF8,
                "application/json");
            JObject response = await _ossApi.CallApiAsync(token, uri, serializedContent);

            return response;
        }

        /// <summary>
        /// Request file izin from OSS.
        /// </summary>
        /// <param name="data">The receive file request data.</param>
        /// <returns>OSS response.</returns>
        public async Task<JObject> ReceiveFile(OssReceiveFileRequest data)
        {
            string token = await _ossApi.Authenticate();

            if (token == null)
            {
                return new JObject();
            }

            string uri = _options.Value.IsStaging ?
                "/api/stagging/get/receive-file-ds/" :
                "/api/get/receive-file-ds/";

            var content = new
            {
                RECEIVEFILEDS = data
            };

            string contentString = JsonConvert
                .SerializeObject(content, _snakeSettings)
                .Replace("receivefileds", "RECEIVEFILEDS");
            StringContent serializedContent = new StringContent(
                contentString,
                Encoding.UTF8,
                "application/json");
            JObject response = await _ossApi.CallApiAsync(token, uri, serializedContent);

            return response;
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
        private readonly IOptions<OssApiOptions> _options;
    }
}