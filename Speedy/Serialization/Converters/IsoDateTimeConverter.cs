#region References

using System;
using Newtonsoft.Json;

#endregion

namespace Speedy.Serialization.Converters
{
	/// <summary>
	/// Converter used to deserialize JSON string to IsoDateTime.
	/// </summary>
	public class IsoDateTimeConverter : JsonConverter<IsoDateTime>
	{
		#region Methods

		/// <inheritdoc />
		public override IsoDateTime ReadJson(JsonReader reader, Type objectType, IsoDateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return reader.Value is DateTime dValue
				? new IsoDateTime { DateTime = dValue, Duration = TimeSpan.Zero }
				: IsoDateTime.Parse(reader.Value as string);
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, IsoDateTime value, JsonSerializer serializer)
		{
			writer.WriteRawValue($"\"{value}\"");
		}

		#endregion
	}
}