using System.Text.Json.Serialization;

namespace Diplom.Models.DTO;

public class OrdersResponseDTO
{    
    [JsonPropertyName("orders")]
    public List<OrdersDTO> order { get; set; }

}