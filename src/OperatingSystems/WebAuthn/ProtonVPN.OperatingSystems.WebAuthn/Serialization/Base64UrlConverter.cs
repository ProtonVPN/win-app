using System.Buffers;
using System.Buffers.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProtonVPN.OperatingSystems.WebAuthn.Serialization;

/// <summary>
/// Custom converter for encoding/decoding byte[] using Base64Url instead of default Base64.
/// </summary>
public sealed class Base64UrlConverter : JsonConverter<byte[]>
{
    public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.HasValueSequence)
        {
            return FromBase64UrlString(reader.GetString());
        }
        else
        {
            return FromBase64UrlString(reader.ValueSpan);
        }
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
    {
        writer?.WriteStringValue(ToBase64UrlString(value));
    }

    /// <summary>
    /// Converts a byte array to a Base64Url encoded string
    /// </summary>
    /// <param name="input">The byte array to convert</param>
    /// <returns>The Base64Url encoded form of the input</returns>
#pragma warning disable CA1055 // URI-like return values should not be strings
    public static string ToBase64UrlString(byte[] input)
#pragma warning restore CA1055 // URI-like return values should not be strings
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return Convert.ToBase64String(input).
            TrimEnd('=').
            Replace('+', '-').
            Replace('/', '_');
    }

    /// <summary>
    /// Converts a Base64Url encoded string to a byte array
    /// </summary>
    /// <param name="input">The Base64Url encoded string</param>
    /// <returns>The byte array represented by the encoded string</returns>
    public static byte[] FromBase64UrlString(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        return Convert.FromBase64String(Pad(input.Replace('-', '+').Replace('_', '/')));
    }

    /// <summary>
    /// Converts a Base64Url encoded string to a byte array
    /// </summary>
    /// <param name="input">The Base64Url encoded string</param>
    /// <returns>The byte array represented by the encoded string</returns>
    public static byte[] FromBase64UrlString(ReadOnlySpan<byte> input)
    {
        if (input == null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        // START temporary workaround for MSFT bug (https://github.com/MichaelGrafnetter/webauthn-interop/issues/21)
        // Checking to see if the last character is a digit and if we removed it, would there be anything left
        if (input.Length > 1 && char.IsDigit((char)input[input.Length - 1]))
        {
            // Looking for last character to be 0, 1, or 2
            char lastChar = (char)input[input.Length - 1];

            // If we removed the last character, calculate the padding required for the remaining string
            int potentialPaddingLength = (input.Length - 1) % 4;

            // If the last character matches the padding length of the remaining string, this is very likely the case we are looking for
            if (lastChar == '0' && potentialPaddingLength == 0 ||
                lastChar == '1' && potentialPaddingLength == 3 ||
                lastChar == '2' && potentialPaddingLength == 2)
            {
                // Update the input to remove the last character
                input = input.Slice(0, input.Length - 1);
            }
        }
        // END temporary workaround

        int paddingLength = (input.Length % 4) switch
        {
            0 => 0, // Padding is not needed
            2 => 2, // "==" missing in Base64Url vs. Base64
            3 => 1, // "=" missing in Base64Url vs. Base64
            _ => throw new ArgumentException("Illegal Base64URL string!", nameof(input))
        };

        // Pad the input to be compatible with BASE64
        int binaryLength = input.Length + paddingLength;
        byte[] result = new byte[binaryLength];
        input.CopyTo(result);

        // Translate Base64Url chars to BASE64 chars
        for (int i = 0; i < binaryLength; i++)
        {
            if ((char)result[i] == '-')
            {
                // Replace '-' with '+'
                result[i] = (byte)'+';
            }
            else if ((char)result[i] == '_')
            {
                // Replace '_' with '/'
                result[i] = (byte)'/';
            }
        }

        // Add padding ("" or "=" or "==")
        for (int i = binaryLength - paddingLength; i < binaryLength; i++)
        {
            result[i] = (byte)'=';
        }

        OperationStatus status = Base64.DecodeFromUtf8InPlace(result, out int bytesWritten);

        if (status != OperationStatus.Done)
        {
            throw new ArgumentException("Illegal Base64URL string!", nameof(input));
        }

        return new Span<byte>(result, 0, bytesWritten).ToArray();
    }

    /// <summary>
    /// Adds padding to the input
    /// </summary>
    /// <param name="input"> the input string </param>
    /// <returns> the padded string </returns>
    private static string Pad(string input)
    {
        if (input.TrimEnd().EndsWith("=", StringComparison.InvariantCulture))
        {
            throw new ArgumentException("Illegal Base64URL string!", nameof(input));
        }

        switch (input.Length % 4)
        {
            case 0:
                // Padding is not needed
                break;
            case 2:
                input += "==";
                break;
            case 3:
                input += "=";
                break;
            default:
                throw new ArgumentException("Illegal Base64URL string!", nameof(input));
        }

        return input;
    }
}
