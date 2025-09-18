using System.ComponentModel.DataAnnotations;

namespace CategoryApi.Data.Model.Entities;

public class Catalog
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Slug { get; set; } = "";
    public int CategoryId { get; set; }
    public decimal Price { get; set; }
    [MaxLength(1024)] public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Category? Category { get; set; }
}
