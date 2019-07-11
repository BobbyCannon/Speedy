#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace Speedy.Storage
{
	internal class SerializeContractResolver : DefaultContractResolver
	{
		#region Fields

		private readonly bool _ignoreReadOnly;

		private readonly Dictionary<string, HashSet<string>> _ignores;

		#endregion

		#region Constructors

		public SerializeContractResolver(bool camelCase, bool ignoreReadOnly)
		{
			_ignores = new Dictionary<string, HashSet<string>>();
			_ignoreReadOnly = ignoreReadOnly;

			NamingStrategy = camelCase ? (NamingStrategy) new CamelCaseNamingStrategy() : new DefaultNamingStrategy();
			NamingStrategy.OverrideSpecifiedNames = false;
			NamingStrategy.ProcessDictionaryKeys = false;
			NamingStrategy.ProcessExtensionDataNames = false;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Explicitly ignore the given property(s) for the given type
		/// </summary>
		/// <param name="type"> The type full name to ignore some properties on. </param>
		/// <param name="propertyNames"> One or more properties to ignore. Leave empty to ignore the type entirely. </param>
		public void Ignore(string type, params string[] propertyNames)
		{
			if (!_ignores.ContainsKey(type))
			{
				_ignores[type] = new HashSet<string>();
			}

			foreach (var prop in propertyNames)
			{
				_ignores[type].Add(prop);
			}
		}

		/// <summary>
		/// Is the given property for the given type ignored?
		/// </summary>
		/// <param name="type"> </param>
		/// <param name="propertyName"> </param>
		/// <returns> </returns>
		public bool IsIgnored(string type, string propertyName)
		{
			return _ignores.ContainsKey(type) && (_ignores[type].Contains(propertyName) || _ignores[type].Count <= 0);
		}

		/// <summary>
		/// Reset the list of ignored values to the provided values.
		/// </summary>
		/// <param name="values"> The collection of types and values to ignore. </param>
		public void ResetIgnores(params KeyValuePair<string, string[]>[] values)
		{
			_ignores.Clear();
			values.ForEach(x => Ignore(x.Key, x.Value));
		}

		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var properties = base.CreateProperties(type, memberSerialization);
			var response = _ignoreReadOnly ? properties.Where(p => p.Writable).ToList() : properties.ToList();
			return response.OrderBy(x => x.PropertyName).ToList();
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			var realType = member.DeclaringType.GetRealType();

			if (IsIgnored(realType.FullName, property.PropertyName))
			{
				property.ShouldSerialize = instance => false;
			}

			return property;
		}

		#endregion
	}
}