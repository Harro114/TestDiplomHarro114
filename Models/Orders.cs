using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Diplom.Models;

public class Orders
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Accounts Accounts { get; set; }
    public float Amounts { get; set; }
    public DateTime DateLastOrder { get; set; }
}