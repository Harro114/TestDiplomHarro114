using System.Text.Json.Serialization;

namespace Diplom.Models.DTO;

public class CategoriesDTO
{
    [JsonPropertyName("id")]
    public int id { get; set; }
    [JsonPropertyName("name")]
    public string name { get; set; }
    [JsonPropertyName("is_active")]
    public bool is_active { get; set; }
}