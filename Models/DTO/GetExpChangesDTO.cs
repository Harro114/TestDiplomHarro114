namespace Diplom.Models.DTO;

public class GetExpChangesDTO
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string UserLastName { get; set; }
    public string UserFirstName { get; set; }
    public int Value { get; set; }
    public int CurrentBalance { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Discription { get; set; }
}