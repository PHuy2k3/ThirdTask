// CatalogDtos.cs
namespace CategoryApi.Biz.Model.Catalogs
{
    public class CatalogNew
    {
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class CatalogEdit
    {
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CatalogView
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Slug { get; set; } = "";
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class CatalogList
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public string Slug { get; set; } = "";
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CatalogFilter
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string? Q { get; set; }
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsActive { get; set; }
    }
}
