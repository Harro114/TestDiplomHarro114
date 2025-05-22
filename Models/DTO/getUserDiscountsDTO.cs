namespace Diplom.Models.DTO;

public class getUserDiscountsDTO
{
    public List<usrDiscountsDTO> Discounts { get; set; }
}

public class usrDiscountsDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int DiscountSize { get; set; }
    public bool isActivated { get; set; }
}