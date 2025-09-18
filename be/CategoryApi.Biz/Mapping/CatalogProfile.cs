using AutoMapper;
using CategoryApi.Data.Model.Entities;
using CategoryApi.Biz.Model.Catalogs;

namespace CategoryApi.Biz.Mapping;

public sealed class CatalogProfile : Profile
{
    public CatalogProfile()
    {
        CreateMap<Catalog, CatalogView>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null));
        CreateMap<Catalog, CatalogList>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category != null ? s.Category.Name : null));


        CreateMap<CatalogNew, Catalog>()
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.MapFrom(_ => DateTime.UtcNow));

        CreateMap<CatalogEdit, Catalog>()
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(_ => DateTime.UtcNow));
    }
}
