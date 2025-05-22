using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Diplom.Models;

public class UserDiscountsHistory
{
    [Key] // Указывает, что это первичный ключ
    [DatabaseGenerated(DatabaseGeneratedOption.None)]

    public int Id { get; set; }
    
    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Accounts Account { get; set; }
    
    
    public int DiscountId { get; set; }
    [ForeignKey("DiscountId")]
    public Discounts Discount { get; set; }
    
  
    public DateTime DateAccruals { get; set; }
    public DateTime DateDelete { get; set; } = DateTime.UtcNow;
}