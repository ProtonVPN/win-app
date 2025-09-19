using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn.Serialization;

public sealed class JsonCustomEnumConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> valuesToNames = CreateValueToNameDictionary();
    private static readonly Dictionary<string, TEnum> namesToValues = valuesToNames.ToDictionary(item => item.Value, item => item.Key);

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                // Support for enum value names
                string text = reader.GetString();
                if (namesToValues.TryGetValue(text, out TEnum value))
                {
                    return value;
                }
                else
                {
                    throw new JsonException($"Invalid enum value = \"{text}\"");
                }

            case JsonTokenType.Number:
                // Support for numeric values
                if (!reader.TryGetInt32(out int number))
                {
                    throw new JsonException($"Invalid enum value = {reader.GetString()}");
                }

                TEnum casted = (TEnum)(object)number; // ints can always be casted to enum, even when the value is not defined
                if (Enum.IsDefined(typeof(TEnum), casted))
                {
                    return casted;
                }
                else
                {
                    throw new JsonException($"Invalid enum value = {number}");
                }

            case JsonTokenType.StartArray:
                // Support for flags
                // TODO: Check if the enum has the Flags attribute

                TEnum result = default;

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    // Combine the flags. All interop flags are defined as uint.
                    TEnum arrayItem = Read(ref reader, typeToConvert, options);
                    result = (TEnum)(object)((uint)(object)result | (uint)(object)arrayItem);
                }

                return result;

            default:
                throw new JsonException($"Invalid enum value ({reader.TokenType})");
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer?.WriteStringValue(valuesToNames[value]);
    }

    private static Dictionary<TEnum, string> CreateValueToNameDictionary()
    {
        Dictionary<TEnum, string> dictionary = new();

        foreach (FieldInfo field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            EnumMemberAttribute description = field.GetCustomAttribute<EnumMemberAttribute>(false);
            TEnum value = (TEnum)field.GetValue(null);
            dictionary[value] = description is not null ? description.Value : value.ToString();
        }

        return dictionary;
    }
}
