namespace Diplom.Models.DTO;

public class GetExchangeDiscountsDTO
{
    public int Id { get; set; }
    public int DiscountId { get; set; }
    public string Name { get; set; }
    public int DiscountExchangeOneId { get; set; }
    public string DiscountExchangeOneName { get; set; }
    public int DiscountExchangeTwoId { get; set; }
    public string DiscountExchangeTwoName { get; set; }
}