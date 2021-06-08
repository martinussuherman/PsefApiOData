using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PsefApiOData.Models;
using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Barcode;
using Syncfusion.Pdf.Graphics;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// Tanda Daftar helpers.
    /// </summary>
    public class TandaDaftarHelper
    {
        /// <summary>
        /// Tanda Daftar helpers.
        /// </summary>
        /// <param name="environment">Web Host environment.</param>
        /// <param name="httpContext">Http context.</param>
        /// <param name="urlHelper">Url helper.</param>
        /// <param name="options">Electronic signature options.</param>
        public TandaDaftarHelper(
            IWebHostEnvironment environment,
            HttpContext httpContext,
            IUrlHelper urlHelper,
            IOptions<ElectronicSignatureOptions> options)
        {
            _environment = environment;
            _httpContext = httpContext;
            _urlHelper = urlHelper;
            _options = options;
        }

        /// <summary>
        /// Generate Tanda Daftar Pdf.
        /// </summary>
        /// <param name="ossInfo">Oss info.</param>
        /// <param name="permohonan">Permohonan.</param>
        /// <param name="perizinan">Perizinan.</param>
        /// <returns>Pdf url.</returns>
        public string GeneratePdf(
            OssFullInfo ossInfo,
            Permohonan permohonan,
            Perizinan perizinan)
        {
            string fileName = $"{ApiHelper.GetUserId(_httpContext.User)}.pdf";
            string datePath = perizinan.IssuedAt.ToString(
                "yyyy-MM-dd",
                DateTimeFormatInfo.InvariantInfo);
            string filePath = PrepareFileAndFolder(datePath, fileName);
            PdfDocument document = new PdfDocument();
            float top = 0;

            SetupPage(document);
            PdfPage page = document.Pages.Add();
            PdfGraphics graphics = page.Graphics;

            top = DrawLogo(graphics, top);
            top = DrawHeader(perizinan, graphics, top);
            top = DrawContent(permohonan, perizinan, ossInfo, graphics, top);

            top = 440;
            top = DrawSignature(perizinan, page, graphics, top);
            top = DrawFooter(perizinan, graphics, top);

            FileStream fileStream = new FileStream(
                filePath,
                FileMode.CreateNew,
                FileAccess.ReadWrite);

            document.Save(fileStream);
            document.Close(true);

            return _urlHelper.Content($"~/{datePath}/{fileName}");
        }

        private string PrepareFileAndFolder(string datePath, string fileName)
        {
            string folderPath = Path.Combine(_environment.WebRootPath, datePath);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return filePath;
        }

        private void SetupPage(PdfDocument document)
        {
            PdfUnitConverter converter = new PdfUnitConverter();

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
        }

        private float DrawLogo(PdfGraphics graphics, float top)
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
                top,
                drawWidth,
                drawHeight);

            return top + drawHeight;
        }

        private float DrawHeader(Perizinan perizinan, PdfGraphics graphics, float top)
        {
            PdfStringFormat centered = new PdfStringFormat
            {
                Alignment = PdfTextAlignment.Center
            };

            top += 6;
            top = DrawString(
                "KEMENTERIAN KESEHATAN\nREPUBLIK INDONESIA",
                graphics,
                new PdfStandardFont(PdfFontFamily.Helvetica, 11),
                centered,
                new RectangleF(0, top, graphics.ClientSize.Width, 0));
            top += 10;
            top = DrawString(
                "TANDA DAFTAR\nPENYELENGGARA SISTEM ELEKTRONIK FARMASI (PSEF)",
                graphics,
                new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold),
                centered,
                new RectangleF(0, top, graphics.ClientSize.Width, 0));
            top += 10;
            top = DrawString(
                $"NOMOR: {perizinan.PerizinanNumber}",
                graphics,
                new PdfStandardFont(PdfFontFamily.Helvetica, 14, PdfFontStyle.Bold),
                centered,
                new RectangleF(0, top, graphics.ClientSize.Width, 0));

            return top;
        }

        private float DrawContent(
            Permohonan permohonan,
            Perizinan perizinan,
            OssFullInfo ossInfo,
            PdfGraphics graphics,
            float top)
        {
            float leftCol = 5;
            float rightCol = graphics.ClientSize.Width / 2 + 5;
            float leftColTop = top + 14;
            float rightColTop = top + 14;
            float width = (graphics.ClientSize.Width - 20) / 2;
            PdfStandardFont labelFont = new PdfStandardFont(PdfFontFamily.Helvetica, 10, PdfFontStyle.Italic);
            PdfStandardFont contentFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Bold);
            PdfStringFormat leftAlign = new PdfStringFormat
            {
                Alignment = PdfTextAlignment.Left
            };

            leftColTop = DrawString(
                "Nomor Tanda Daftar PSE:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop = DrawString(
                permohonan.PermohonanNumber,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop += 4;

            leftColTop = DrawString(
                "Tanggal Terbit:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop = DrawString(
                perizinan.IssuedAt.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("id")),
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));

            rightColTop = DrawString(
                "Bidang:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop = DrawString(
                "PELAYANAN",
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop += 4;

            rightColTop = DrawString(
                "Tanggal Berakhir:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop = DrawString(
                perizinan.ExpiredAt.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("id")),
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));

            top = leftColTop > rightColTop ? leftColTop : rightColTop;
            leftColTop = rightColTop = top + 12;

            leftColTop = DrawString(
                "Nama Sistem Elektronik:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop = DrawString(
                permohonan.SystemName,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop += 4;

            leftColTop = DrawString(
                "Alamat Domain Sistem Elektronik:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop = DrawString(
                permohonan.Domain,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop += 4;

            leftColTop = DrawString(
                "Apoteker Penanggung Jawab:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop = DrawString(
                permohonan.ApotekerName,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop += 4;

            leftColTop = DrawString(
                "Nomor STRA:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop = DrawString(
                permohonan.StraNumber,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(leftCol, leftColTop, width, 0));
            leftColTop += 4;

            rightColTop = DrawString(
                "Nomor Induk Berusaha:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop = DrawString(
                (ossInfo?.Nib) != null ? ossInfo.Nib : string.Empty,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop += 4;

            rightColTop = DrawString(
                "Diajukan oleh:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop = DrawString(
                (ossInfo?.NamaPerseroan) != null ? ossInfo.NamaPerseroan : string.Empty,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop += 4;

            rightColTop = DrawString(
                "Alamat:",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));
            rightColTop = DrawString(
                (ossInfo?.AlamatPerseroan) != null ? ossInfo.AlamatPerseroan : string.Empty,
                graphics,
                contentFont,
                leftAlign,
                new RectangleF(rightCol, rightColTop, width, 0));

            return leftColTop > rightColTop ? leftColTop : rightColTop;
        }

        private float DrawSignature(
            Perizinan perizinan,
            PdfPage page,
            PdfGraphics graphics,
            float top)
        {
            float leftCol = 20;
            float rightCol = (graphics.ClientSize.Width / 2) - 30;
            float width = (graphics.ClientSize.Width / 2) + 20;
            PdfStringFormat leftAlign = new PdfStringFormat
            {
                Alignment = PdfTextAlignment.Left
            };
            PdfQRBarcode barcode = new PdfQRBarcode
            {
                ErrorCorrectionLevel = PdfErrorCorrectionLevel.High,
                XDimension = 3,
                Text = perizinan.PerizinanNumber
            };

            barcode.Draw(page, new PointF(leftCol, top + 10));

            top = DrawString(
                "A.N. MENTERI KESEHATAN\nDIREKTUR JENDERAL\nKEFARMASIAN DAN ALAT KESEHATAN\n\n\nTTD\n\n\nENGKO SOSIALINE M.",
                graphics,
                new PdfStandardFont(PdfFontFamily.Helvetica, 11),
                leftAlign,
                new RectangleF(rightCol, top, width, 0));

            return top;
        }

        private float DrawFooter(Perizinan perizinan, PdfGraphics graphics, float top)
        {
            float leftCol = 5;
            float width = graphics.ClientSize.Width - 10;
            PdfStandardFont italicFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11, PdfFontStyle.Italic);
            PdfStandardFont labelFont = new PdfStandardFont(PdfFontFamily.Helvetica, 11);
            PdfStringFormat leftAlign = new PdfStringFormat
            {
                Alignment = PdfTextAlignment.Left
            };
            PdfStringFormat justified = new PdfStringFormat
            {
                Alignment = PdfTextAlignment.Justify
            };

            SizeF indentSize = labelFont.MeasureString("-");
            float indentCol = leftCol + indentSize.Width + 2;
            float indentWidth = graphics.ClientSize.Width - indentCol;

            top += 16;
            top = DrawString(
                "Keterangan:",
                graphics,
                italicFont,
                leftAlign,
                new RectangleF(leftCol, top, width, 0));
            top += 4;
            DrawString(
                "-",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, top, indentSize.Width, 0));
            top = DrawString(
                "Tanda Daftar Penyelenggara Sistem Elektronik Farmasi (PSEF) Merupakan Dokumen Pemenuhan Komitmen Dalam Izin Operasional/Komersial Penyelenggara Sistem Elektronik Farmasi (PSEF)",
                graphics,
                labelFont,
                justified,
                new RectangleF(indentCol, top, indentWidth, 0));

            string expired = perizinan.ExpiredAt.ToString(
                "dd MMMM yyyy",
                CultureInfo.CreateSpecificCulture("id"));

            DrawString(
                "-",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, top, indentSize.Width, 0));
            top = DrawString(
                $"Tanda Daftar Penyelenggara Sistem Elektronik Farmasi (PSEF) Berlaku Sejak Tanggal Diterbitkan Sampai Dengan {expired}",
                graphics,
                labelFont,
                justified,
                new RectangleF(indentCol, top, indentWidth, 0));

            DrawString(
                "-",
                graphics,
                labelFont,
                leftAlign,
                new RectangleF(leftCol, top, indentSize.Width, 0));
            top = DrawString(
                "Jika Tidak Dilakukan Pembaruan, Akan Dihapus Dari Tanda Daftar PSEF Kementerian Kesehatan",
                graphics,
                labelFont,
                justified,
                new RectangleF(indentCol, top, indentWidth, 0));

            return top;
        }

        private float DrawString(
            string text,
            PdfGraphics graphics,
            PdfStandardFont font,
            PdfStringFormat format,
            RectangleF bounds)
        {
            SizeF size = font.MeasureString(text, bounds.Width, format);

            bounds.Size = new SizeF(bounds.Width, size.Height);
            graphics.DrawString(text, font, PdfBrushes.Black, bounds, format);
            return bounds.Bottom;
        }

        private readonly IWebHostEnvironment _environment;
        private readonly HttpContext _httpContext;
        private readonly IUrlHelper _urlHelper;
        private readonly IOptions<ElectronicSignatureOptions> _options;
    }
}