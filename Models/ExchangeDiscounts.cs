using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Diplom.Models;

public class ExchangeDiscounts
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } 
    public int DiscountId { get; set; }
    public Discounts Discount { get; set; }
    public int DiscountExchangeOneId { get; set; }
    public Discounts DiscountOne { get; set; }
    [ForeignKey("DiscountExchangeOneId")]
    public int DiscountExchangeTwoId { get; set; }
    [ForeignKey("DiscountExchangeTwoId")]
    public Discounts DiscountTwo { get; set; }
}