using System.ComponentModel.DataAnnotations;

namespace PcClub.Models;

public abstract class EntityBase
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Укажите название"), StringLength(100)]
    [Display(Name = "Название")]
    public string Name { get; set; } = "";
    [Range(0, 9999), Display(Name = "Порядок показа")]
    public int SortOrder { get; set; }
}

public class ServiceCategory : EntityBase
{
    public ICollection<ClubService> Services { get; set; } = new List<ClubService>();
}

public class ClubService : EntityBase
{
    [Required, StringLength(180), Display(Name = "Краткое описание")]
    public string Summary { get; set; } = "";
    [Required, StringLength(3000), Display(Name = "Описание")]
    public string Description { get; set; } = "";
    [Range(0, 1000000), Display(Name = "Цена, ₽")]
    public decimal Price { get; set; }
    [Required, StringLength(40), Display(Name = "Единица тарифа")]
    public string PriceUnit { get; set; } = "час";
    [Required, StringLength(250), Display(Name = "Характеристики (через •)")]
    public string Specs { get; set; } = "";
    [StringLength(250)]
    public string ImagePath { get; set; } = "/images/arena.webp";
    [Range(1, int.MaxValue), Display(Name = "Категория")]
    public int ServiceCategoryId { get; set; }
    public ServiceCategory? Category { get; set; }
}

public record CatalogPage(IReadOnlyList<ServiceCategory> Categories, IReadOnlyList<ClubService> Services, int? CategoryId);

public class ClubOptions
{
    public string Name { get; set; } = "PC CLUB";
    public bool DemoMode { get; set; } = true;
    public string Address { get; set; } = "";
    public string Hours { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
}
