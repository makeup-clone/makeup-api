using Application.Models;
using AutoMapper;
using Domain.Entities;

namespace Application.Mappers;

public class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<CategoryEntity, CategoryModel>()
            .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children));

        CreateMap<CategoryModel, CategoryEntity>()
            .ForMember(dest => dest.Children, opt => opt.Ignore());

        
    }
}
