namespace Diplom.Models.DTO;

public class ExchangeDiscountsDTO
{
    public int? Id { get; set; }
    public int DiscountId { get; set; }
    public int DiscountExchangeOneId { get; set; }
    public int DiscountExchangeTwoId { get; set; }
}