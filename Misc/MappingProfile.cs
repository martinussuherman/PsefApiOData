using AutoMapper;
using PsefApiOData.Models;

namespace PsefApiOData.Misc
{
    /// <summary>
    /// AutoMapper mapping profile.
    /// </summary>
    public class MappingProfile : Profile
    {
        /// <summary>
        /// Creates AutoMapper mapping profile.
        /// </summary>
        public MappingProfile()
        {
            CreateMap<Perizinan, PerizinanView>()
                .ForMember(
                    dto => dto.Domain,
                    opt => opt.MapFrom(
                        src => src.Permohonan.Domain))
                .ForMember(
                    dto => dto.CompanyName,
                    opt => opt.MapFrom(
                    src => src.Permohonan.Pemohon.CompanyName));

            CreateMap<Perizinan, PerizinanHalamanMuka>()
                .ForMember(
                    dto => dto.Domain,
                    opt => opt.MapFrom(
                        src => src.Permohonan.Domain))
                .ForMember(
                    dto => dto.CompanyName,
                    opt => opt.MapFrom(
                    src => src.Permohonan.Pemohon.CompanyName));

            CreateMap<PerizinanUpdate, Perizinan>();
            CreateMap<Perizinan, PerizinanUpdate>();
        }
    }
}
