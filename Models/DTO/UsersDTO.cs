using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diplom.Models.DTO;

public class UsersDTO
{
    [JsonPropertyName("id")]
    public int id { get; set; }
    [JsonPropertyName("username")]
    public string username { get; set; }
    [JsonPropertyName("last_name")]
    public string last_name { get; set; }
    [JsonPropertyName("first_name")]
    public string first_name { get; set; }
    [JsonPropertyName("sex")]
    public bool sex { get; set; }
    [JsonPropertyName("is_blocked")]
    public bool is_blocked { get; set; }
    [JsonPropertyName("password")]
    public string password { get; set; }
    [JsonPropertyName("created_at")]
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime created_at { get; set; }
}

