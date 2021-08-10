#region References

using System;
using Newtonsoft.Json;

#endregion

namespace Speedy
{
	/// <summary>
	/// Converter used to deserialize JSON string to partial updates.
	/// </summary>
	public class PartialUpdateConverter : JsonConverter
	{
		#region Methods

		/// <inheritdoc />
		public override bool CanConvert(Type objectType)
		{
			var partialType = typeof(PartialUpdate<>);
			var isPartialType = objectType.IsSubclassOf(partialType);
			return isPartialType;
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return PartialUpdate.FromJson(reader, objectType, serializer);
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}