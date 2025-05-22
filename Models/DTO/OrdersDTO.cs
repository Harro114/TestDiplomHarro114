using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diplom.Models.DTO;

public class CustomDateTimeConverter : JsonConverter<DateTime>
{
    private const string DateFormat = "yyyy-MM-dd HH:mm:ss";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        return DateTime.ParseExact(dateString, DateFormat, System.Globalization.CultureInfo.InvariantCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat));
    }
}

public class OrdersDTO
{
    [JsonPropertyName("account_id")] public int account_id { get; set; }
    [JsonPropertyName("amount")] public float amount { get; set; }

    [JsonPropertyName("order_date")]
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime order_date { get; set; }
}