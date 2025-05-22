using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplom.Models;

public class AccountRole
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int AccountId { get; set; }
    [ForeignKey("AccountId")]
    public Accounts Account { get; set; }
    
    public int RoleId { get; set; }
    [ForeignKey("RoleId")]
    public Roles Role { get; set; }
}