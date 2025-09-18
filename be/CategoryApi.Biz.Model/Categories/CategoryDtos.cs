// CategoryDtos.cs
namespace CategoryApi.Biz.Model.Categories
{
    public class CategoryNew
    {
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
        public int SortOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }

    public class CategoryEdit
    {
        public string Name { get; set; } = "";
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
    }

    public class CategoryView
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ChildrenCount { get; set; }
    }

    public class CategoryList
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public int? ParentId { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public int ChildrenCount { get; set; }
    }

    public class CategoryFilter
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string? Q { get; set; }
        public int? ParentId { get; set; }
    }
}
