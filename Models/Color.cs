using System.Text.Json;
using System.Text.Json.Serialization;

namespace Canvas.Net.Models;

public class CanvasColor
{
    public CanvasColor(string value)
    {
        color = value;
    }

    public CanvasColor(int preset)
    {
        color = preset.ToString();
    }

    [JsonInclude]
    public string color;   
}
public class CanvasColorJsonConverter : JsonConverter<CanvasColor>
{
    public override CanvasColor Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return new CanvasColor(reader.GetString()!);
    }

    public override void Write(
        Utf8JsonWriter writer,
        CanvasColor canvasColor,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(canvasColor.color);
    }
}