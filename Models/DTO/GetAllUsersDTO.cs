namespace Diplom.Models.DTO;

public class GetAllUsersDTO
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string UserLastName { get; set; }
    public string UserFirstName { get; set; }
    public bool Sex { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsBlocked { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
}