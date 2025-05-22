using System.Text.Json.Serialization;

namespace Diplom.Models.DTO;

public class UsersResponseDTO
{
    [JsonPropertyName("users")]
    public List<UsersDTO> user { get; set; }
}