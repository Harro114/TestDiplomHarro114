using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Diplom.Models;

public class ExpChanges
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Accounts Accounts { get; set; }
    public int ExpUserId { get; set; }
    [ForeignKey("ExpUserId")]
    public ExpUsersWallets ExpUsersWallets { get; set; }
    public int Value { get; set; }
    public int CurrentBalance { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Discription { get; set; }
}