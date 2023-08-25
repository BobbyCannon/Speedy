#region References

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Speedy.Extensions;

#endregion

namespace Speedy.Serialization
{
	/// <summary>
	/// The contract resolver used for ToJson and GetSerializerSettings.
	/// </summary>
	public class JsonContractResolver : DefaultContractResolver
	{
		#region Fields

		private readonly Func<string, bool> _ignoreMember;
		private readonly Func<Type, HashSet<string>> _getIgnoredProperties;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a contract resolver for serializing.
		/// </summary>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="getIgnoredProperties"> </param>
		/// <param name="ignoreMember"> </param>
		public JsonContractResolver(bool camelCase, Func<Type, HashSet<string>> getIgnoredProperties, Func<string, bool> ignoreMember)
		{
			NamingStrategy = camelCase ? new CamelCaseNamingStrategy() : new DefaultNamingStrategy();
			NamingStrategy.OverrideSpecifiedNames = false;
			NamingStrategy.ProcessDictionaryKeys = true;
			NamingStrategy.ProcessExtensionDataNames = false;

			_getIgnoredProperties = getIgnoredProperties;
			_ignoreMember = ignoreMember;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var typeIgnoredProperties = _getIgnoredProperties(type.GetRealTypeUsingReflection());
			var properties = base.CreateProperties(type, memberSerialization);
			var response = properties
				.Where(p => !typeIgnoredProperties.Contains(p.PropertyName) && !_ignoreMember(p.PropertyName))
				.OrderBy(p => p.PropertyName)
				.ToList();
			return response;
		}

		#endregion
	}
}