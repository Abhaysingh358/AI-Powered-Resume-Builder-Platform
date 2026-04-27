using AutoMapper;
using ResumeAI.Template.DTOs.Response;
using ResumeAI.Template.Entities;

namespace ResumeAI.Template.Configuration;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Full response including HtmlLayout and CssStyles
        CreateMap<ResumeTemplate, TemplateResponse>()
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()));

        // Lightweight list response — no layout/styles
        CreateMap<ResumeTemplate, TemplateListResponse>()
            .ForMember(d => d.Category, o => o.MapFrom(s => s.Category.ToString()));
    }
}
