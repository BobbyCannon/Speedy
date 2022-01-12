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
	internal class JsonContractResolver : DefaultContractResolver
	{
		#region Fields

		private readonly Func<string, bool> _ignoreMember;
		private readonly Func<Type, HashSet<string>> _initializeType;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a contract resolver for serializing.
		/// </summary>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="initializeType"> </param>
		/// <param name="ignoreMember"> </param>
		public JsonContractResolver(bool camelCase, Func<Type, HashSet<string>> initializeType, Func<string, bool> ignoreMember)
		{
			NamingStrategy = camelCase ? new CamelCaseNamingStrategy() : new DefaultNamingStrategy();
			NamingStrategy.OverrideSpecifiedNames = false;
			NamingStrategy.ProcessDictionaryKeys = false;
			NamingStrategy.ProcessExtensionDataNames = false;

			_initializeType = initializeType;
			_ignoreMember = ignoreMember;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var typeIgnoredProperties = _initializeType(type.GetRealType());
			var properties = base.CreateProperties(type, memberSerialization);
			var response = properties.Where(p => !typeIgnoredProperties.Contains(p.PropertyName) && !_ignoreMember(p.PropertyName))
				.OrderBy(p => p.PropertyName)
				.ToList();
			return response;
		}

		#endregion
	}
}