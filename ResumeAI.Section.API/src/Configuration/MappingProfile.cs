using AutoMapper;
using ResumeAI.Section.DTOs.Response;
using ResumeAI.Section.Entities;

namespace ResumeAI.Section.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ResumeSection, SectionResponse>()
            .ForMember(d => d.SectionType, o => o.MapFrom(s => s.SectionType.ToString()));
    }
}
