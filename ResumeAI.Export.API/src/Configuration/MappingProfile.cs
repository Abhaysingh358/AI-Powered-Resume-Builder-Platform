using AutoMapper;
using ResumeAI.Export.DTOs.Response;
using ResumeAI.Export.Entities;

namespace ResumeAI.Export.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ExportJob, ExportJobResponse>()
            .ForMember(d => d.Format, o => o.MapFrom(s => s.Format.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
