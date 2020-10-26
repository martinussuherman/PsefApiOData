using System.Linq;
using PsefApiOData.Models;

namespace PsefApiOData.Misc
{
    internal class PermohonanHelper
    {
        public PermohonanHelper(PsefMySqlContext context)
        {
            _context = context;
        }

        public IQueryable<Permohonan> Verifikator()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.Diajukan.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSeksi.Id);
        }

        public IQueryable<Permohonan> Kasi()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiVerifikator.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanKepalaSubDirektorat.Id);
        }

        public IQueryable<Permohonan> Kasubdit()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiKepalaSeksi.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturPelayananFarmasi.Id);
        }

        public IQueryable<Permohonan> Diryanfar()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiKepalaSubDirektorat.Id ||
                    c.StatusId == PermohonanStatus.DikembalikanDirekturJenderal.Id);
        }

        public IQueryable<Permohonan> Dirjen()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiDirekturPelayananFarmasi.Id);
        }

        public IQueryable<Permohonan> Validator()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.DisetujuiDirekturJenderal.Id);
        }

        public IQueryable<Permohonan> NonRumusan()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId != PermohonanStatus.Dibuat.Id &&
                    c.StatusId != PermohonanStatus.DikembalikanVerifikator.Id);
        }

        public IQueryable<Permohonan> DalamProses()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId != PermohonanStatus.Dibuat.Id &&
                    c.StatusId != PermohonanStatus.DikembalikanVerifikator.Id &&
                    c.StatusId != PermohonanStatus.Selesai.Id &&
                    c.StatusId != PermohonanStatus.Ditolak.Id);
        }

        public IQueryable<Permohonan> Ditolak()
        {
            return _context.Permohonan
                .Where(c =>
                    c.StatusId == PermohonanStatus.Ditolak.Id);
        }

        private readonly PsefMySqlContext _context;
    }
}