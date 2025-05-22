using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Diplom.Models;

public class Accounts
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Username { get; set; }
    public string UserLastName { get; set; }
    public string UserFirstName { get; set; }
    public bool Sex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool? IsBlocked { get; set; }
}