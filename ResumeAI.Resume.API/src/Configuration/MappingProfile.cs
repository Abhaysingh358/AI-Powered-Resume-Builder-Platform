using AutoMapper;
using ResumeAI.Resume.DTOs.Response;
using ResumeAI.Resume.Entities;

namespace ResumeAI.Resume.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ResumeEntity, ResumeResponse>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
    }
}
