using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WorkerService1
{
    public class ConditionConverter : JsonConverter<ICondition>
    {
        public override ICondition Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using (var jsonDoc = JsonDocument.ParseValue(ref reader))
            {
                var root = jsonDoc.RootElement;
                var type = root.GetProperty("Type").GetString();

                switch (type)
                {
                    case "LastLineContains":
                        return JsonSerializer.Deserialize<LastLineContains>(root.GetRawText(), options);
                    case "InactiveFor":
                        return JsonSerializer.Deserialize<InactiveFor>(root.GetRawText(), options);
                    default:
                        throw new JsonException("Unknown condition type.");
                }
            }
        }

        public override void Write(Utf8JsonWriter writer, ICondition value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }
    }
}