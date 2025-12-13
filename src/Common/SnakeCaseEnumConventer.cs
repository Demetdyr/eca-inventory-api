using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EcaInventoryApi.Common
{
    public class SnakeCaseEnumConverter<T> : ValueConverter<T, string> where T : struct, Enum
    {
        public SnakeCaseEnumConverter() : base(
            v => EnumNamingHelper.ToSnakeCase(v),
            v => EnumNamingHelper.FromSnakeCase<T>(v))
        { }
    }

    public static class EnumNamingHelper
    {
        public static string ToSnakeCase<T>(T value) where T : struct, Enum
        {
            var s = value.ToString()!;
            return Regex
                .Replace(s, "([a-z])([A-Z])", "$1_$2")
                .ToLowerInvariant();
        }

        public static T FromSnakeCase<T>(string snake) where T : struct, Enum
        {
            var pascal = string.Concat(snake.Split('_')
                .Select(w => char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant()));
            if (Enum.TryParse<T>(pascal, ignoreCase: true, out var result))
                return result;
            throw new ArgumentException($"Invalid enum value '{snake}' for {typeof(T).Name}");
        }
    }

    public class SnakeCaseEnumJsonConverter<T> : JsonConverter<T> where T : struct, Enum
    {
        public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            if (value == null) return default;

            var pascal = Regex.Replace(value, "_([a-z])", m => m.Groups[1].Value.ToUpper());
            pascal = char.ToUpper(pascal[0]) + pascal.Substring(1);

            return Enum.Parse<T>(pascal);
        }

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            string snake = Regex.Replace(value.ToString(), "([a-z])([A-Z])", "$1_$2").ToLower();
            writer.WriteStringValue(snake);
        }
    }
}