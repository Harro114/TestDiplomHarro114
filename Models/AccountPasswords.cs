using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplom.Models;

public class AccountPasswords
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required] public int AccountId { get; set; }

    [Required] public string PasswordHash { get; set; } // Хранение пароля в хэшиованном формате.

    [ForeignKey("AccountId")] public Accounts Account { get; set; }
}