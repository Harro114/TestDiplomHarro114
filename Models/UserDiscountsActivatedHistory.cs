using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplom.Models;

public class UserDiscountsActivatedHistory
{
   
    public int Id { get; set; }
    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Accounts Accounts { get; set; }
    public int DiscountId { get; set; }
    [ForeignKey("DiscountId")]
    public Discounts Discounts { get; set; }
    public DateTime DateActivateDiscount { get; set; }
    public DateTime DateDelete { get; set; } = DateTime.UtcNow;
}