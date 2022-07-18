#region References

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Speedy.Serialization.Converters
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
			var partialType = typeof(PartialUpdate);
			var isPartialType = objectType.IsSubclassOf(partialType);
			return isPartialType;
		}

		/// <summary>
		/// Convert the JTokenType to a Type.
		/// </summary>
		/// <param name="type"> The type to be converted. </param>
		/// <returns> The converted type value. </returns>
		public static Type ConvertType(JTokenType type)
		{
			return type switch
			{
				JTokenType.Integer => typeof(int),
				JTokenType.Float => typeof(float),
				JTokenType.String => typeof(string),
				JTokenType.Boolean => typeof(bool),
				JTokenType.Date => typeof(DateTime),
				JTokenType.Bytes => typeof(byte[]),
				JTokenType.Guid => typeof(Guid),
				JTokenType.Uri => typeof(Uri),
				JTokenType.TimeSpan => typeof(TimeSpan),
				JTokenType.Raw => typeof(object),
				JTokenType.Null => typeof(object),
				JTokenType.Undefined => typeof(object),
				JTokenType.Object => typeof(object),
				JTokenType.None => typeof(object),
				_ => typeof(object)
			};
		}

		/// <inheritdoc />
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			return objectType == typeof(PartialUpdate) 
				? PartialUpdate.FromJson(reader, new PartialUpdateOptions()) 
				: PartialUpdate.FromJson(reader, objectType, new PartialUpdateOptions());
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value is PartialUpdate update)
			{
				var expando = update.GetExpandoObject();
				serializer.Serialize(writer, expando);
			}
		}

		#endregion
	}
}