﻿#region References

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
			if (reader.TokenType != JsonToken.StartObject)
			{
				return default;
			}
			
			var item = JObject.Load(reader);
			var major = int.TryParse(item["Major"]?.ToString(), out var m0) ? m0 : 0;
			var minor = int.TryParse(item["Minor"]?.ToString(), out var m1) ? m1 : 0;
			var build = int.TryParse(item["Build"]?.ToString(), out var b) ? b : 0;
			var revision = int.TryParse(item["Revision"]?.ToString(), out var r) ? r : 0;
			return new Version(major, minor, build, revision);
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
		{
			writer.WriteStartObject();
			writer.WritePropertyName("Major");
			writer.WriteValue(value.Major);
			writer.WritePropertyName("Minor");
			writer.WriteValue(value.Minor);
			writer.WritePropertyName("Build");
			writer.WriteValue(value.Build);
			writer.WritePropertyName("Revision");
			writer.WriteValue(value.Revision);
			writer.WriteEndObject();
			//writer.WriteRaw($"{{\"Build\":{value.Build},\"Major\":{value.Major},\"MajorRevision\":{value.MajorRevision},\"Minor\":{value.Minor},\"MinorRevision\":{value.MinorRevision},\"Revision\":{value.Revision}}}");
		}

		#endregion
	}
}