#region References

using System;
using Newtonsoft.Json;

#endregion

namespace Speedy.Serialization.Converters
{
	/// <summary>
	/// Converter for Version to convert to/from string format of "0.0.0.0".
	/// </summary>
	public class VersionStringConverter : JsonConverter<Version>
	{
		#region Properties

		/// <inheritdoc />
		public override bool CanRead => true;

		/// <inheritdoc />
		public override bool CanWrite => true;

		#endregion

		#region Methods

		/// <inheritdoc />
		public override Version ReadJson(JsonReader reader, Type objectType, Version existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var data = reader.Value.ToString();

			if (Version.TryParse(data, out var version))
			{
				return version;
			}

			return hasExistingValue ? existingValue : default;
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToString(4));
		}

		#endregion
	}
}