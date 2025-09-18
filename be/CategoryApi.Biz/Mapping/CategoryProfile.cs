using AutoMapper;
using CategoryApi.Biz.Model.Categories;
using CategoryApi.Data.Model.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CategoryApi.Biz.Mapping;

public sealed class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, CategoryView>();
        CreateMap<Category, CategoryList>()
            .ForMember(d => d.ChildrenCount, o => o.MapFrom(s => s.Catalogs.Count));

        CreateMap<CategoryNew, Category>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        CreateMap<CategoryEdit, Category>()
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
    }
}
