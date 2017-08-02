#region References

using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#endregion

namespace Speedy.Storage
{
	internal class SpeedySerializeContractResolver : DefaultContractResolver
	{
		#region Fields

		private readonly Dictionary<Type, HashSet<string>> _ignores;

		#endregion

		#region Constructors

		public SpeedySerializeContractResolver()
		{
			_ignores = new Dictionary<Type, HashSet<string>>();
		}

		#endregion

		#region Properties

		public bool UseCamelCase
		{
			get => NamingStrategy is CamelCaseNamingStrategy;
			set
			{
				if (value && (NamingStrategy == null || NamingStrategy is DefaultNamingStrategy))
				{
					NamingStrategy = new CamelCaseNamingStrategy(true, true);
				}

				if (!value && (NamingStrategy == null || NamingStrategy is CamelCaseNamingStrategy))
				{
					NamingStrategy = new DefaultNamingStrategy();
				}
			}
		}

		#endregion

		#region Methods

		/// <summary>
		/// Explicitly ignore the given property(s) for the given type
		/// </summary>
		/// <param name="type"> The type to ignore some properties on. </param>
		/// <param name="propertyNames"> One or more properties to ignore. Leave empty to ignore the type entirely. </param>
		public void Ignore(Type type, params string[] propertyNames)
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
		public bool IsIgnored(Type type, string propertyName)
		{
			return _ignores.ContainsKey(type) && (_ignores[type].Contains(propertyName) || _ignores[type].Count <= 0);
		}

		/// <summary>
		/// Reset the list of ignored values to the provided values.
		/// </summary>
		/// <param name="values"> The collection of types and values to ignore. </param>
		public void ResetIgnores(params KeyValuePair<Type, string[]>[] values)
		{
			_ignores.Clear();

			values.ForEach(x => Ignore(x.Key, x.Value));
		}

		protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
		{
			var property = base.CreateProperty(member, memberSerialization);
			var type = member.DeclaringType.GetRealType();

			if (IsIgnored(type, property.PropertyName))
			{
				property.ShouldSerialize = instance => false;
			}

			return property;
		}

		#endregion
	}
}