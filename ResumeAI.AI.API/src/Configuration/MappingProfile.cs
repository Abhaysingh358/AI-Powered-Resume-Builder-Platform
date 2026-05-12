using AutoMapper;
using ResumeAI.AI.DTOs.Response;
using ResumeAI.AI.Entities;

namespace ResumeAI.AI.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<AiRequest, AiRequestResponse>()
            .ForMember(d => d.RequestType, o => o.MapFrom(s => s.RequestType.ToString()))
            .ForMember(d => d.Model,       o => o.MapFrom(s => s.Model.ToString()))
            .ForMember(d => d.Status,      o => o.MapFrom(s => s.Status.ToString()));
    }
}
