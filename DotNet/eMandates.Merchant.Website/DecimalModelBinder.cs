using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace eMandates.Merchant.Website
{
    internal class DecimalModelBinder : JsonConverter<decimal?>
    {
        public override decimal? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetDecimal();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                string value = reader.GetString();
                value = value.Replace(",", CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator);
                value = value.Replace(".", CultureInfo.InvariantCulture.NumberFormat.CurrencyDecimalSeparator);

                if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                {
                    return result;
                }
            }

            throw new JsonException("Unrecognized JSON token when parsing decimal.");
        }

        public override void Write(Utf8JsonWriter writer, decimal? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
            {
                writer.WriteNumberValue(value.Value);
            }
            else
            {
                writer.WriteNullValue();
            }
        }
    }
}