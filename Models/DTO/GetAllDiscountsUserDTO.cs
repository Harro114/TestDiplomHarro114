namespace Diplom.Models.DTO;

public class GetAllDiscountsUserDTO
{
    public List<DiscountDTO> Discount { get; set; }
}

public class DiscountDTO
{
    public int Id { get; set; }
    public int DiscountId { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool isActive { get; set; }
    public int DiscountSize { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<ProductsStore>? ProductsId { get; set; }
    public ProductsStore? ProductsStore { get; set; }
    public List<CategoriesStore>? CategoriesId { get; set; }
    public CategoriesStore?CategoriesStore { get; set; }
    public int Amount { get; set; }
    public bool isPrimary {get;set;}
}