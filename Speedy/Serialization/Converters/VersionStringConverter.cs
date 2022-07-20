#region References

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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

			var majorValue = item.TryGetValue("major", StringComparison.OrdinalIgnoreCase, out var majorToken) ? majorToken.ToString() : "0";
			var minorValue = item.TryGetValue("minor", StringComparison.OrdinalIgnoreCase, out var minorToken) ? minorToken.ToString() : "0";
			var buildValue = item.TryGetValue("build", StringComparison.OrdinalIgnoreCase, out var buildToken) ? buildToken.ToString() : "0";
			var revisionValue = item.TryGetValue("revision", StringComparison.OrdinalIgnoreCase, out var revisionToken) ? revisionToken.ToString() : "0";

			var major = int.TryParse(majorValue, out var m0) ? m0 : 0;
			var minor = int.TryParse(minorValue, out var m1) ? m1 : 0;
			var build = int.TryParse(buildValue, out var b) ? b : 0;
			var revision = int.TryParse(revisionValue, out var r) ? r : 0;

			return new Version(major, minor, build, revision);
		}

		/// <inheritdoc />
		public override void WriteJson(JsonWriter writer, Version value, JsonSerializer serializer)
		{
			var camelcase = serializer.ContractResolver is JsonContractResolver jcr 
				&& jcr.NamingStrategy is CamelCaseNamingStrategy;

			writer.WriteStartObject();
			writer.WritePropertyName(camelcase ? "major" : "Major");
			writer.WriteValue(value.Major);
			writer.WritePropertyName(camelcase ? "minor" :"Minor");
			writer.WriteValue(value.Minor);
			writer.WritePropertyName(camelcase ? "build" :"Build");
			writer.WriteValue(value.Build);
			writer.WritePropertyName(camelcase ? "revision" :"Revision");
			writer.WriteValue(value.Revision);
			writer.WriteEndObject();
			//writer.WriteRaw($"{{\"Build\":{value.Build},\"Major\":{value.Major},\"MajorRevision\":{value.MajorRevision},\"Minor\":{value.Minor},\"MinorRevision\":{value.MinorRevision},\"Revision\":{value.Revision}}}");
		}

		#endregion
	}
}