#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Speedy.Collections;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Internal static class for caching items that are internal to Speedy.
/// </summary>
public static class Cache
{
	#region Constructors

	static Cache()
	{
		SettableProperties = new ConcurrentDictionary<Type, ReadOnlySet<PropertyInfo>>();
		SettablePropertiesPublicOnly = new ConcurrentDictionary<Type, ReadOnlySet<PropertyInfo>>();
		PropertyDictionaryForType = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Property dictionary for a sync object, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> PropertyDictionaryForType { get; }

	/// <summary>
	/// Property dictionary for a sync object, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<Type, ReadOnlySet<PropertyInfo>> SettableProperties { get; }

	/// <summary>
	/// Property dictionary for a sync object, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<Type, ReadOnlySet<PropertyInfo>> SettablePropertiesPublicOnly { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Get the property dictionary for a provided type.
	/// </summary>
	/// <param name="type"> The type to get the properties for. </param>
	/// <returns> The properties in a dictionary. The key is the property name. </returns>
	public static IReadOnlyDictionary<string, PropertyInfo> GetPropertyDictionary(Type type)
	{
		return PropertyDictionaryForType.GetOrAdd(type, x =>
		{
			var properties = x.GetCachedProperties().OrderBy(p => p.Name).ToArray();
			return properties.ToDictionary(p => p.Name, p => p);
		});
	}

	/// <summary>
	/// Get settable properties for an object.
	/// </summary>
	/// <param name="value"> The value to process. </param>
	/// <returns> The settable properties information. </returns>
	public static ReadOnlySet<PropertyInfo> GetSettableProperties(object value)
	{
		var realType = value is Notifiable notifiable
			? notifiable.GetRealType()
			: value.GetRealTypeUsingReflection();

		return SettableProperties
			.GetOrAdd(realType, t => t
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToHashSet()
				.AsReadOnly()
			);
	}

	/// <summary>
	/// Get public settable properties for an object.
	/// </summary>
	/// <param name="value"> The value to process. </param>
	/// <returns> The public settable properties information. </returns>
	public static ReadOnlySet<PropertyInfo> GetSettablePropertiesPublicOnly(object value)
	{
		var realType = value is Notifiable notifiable
			? notifiable.GetRealType()
			: value.GetRealTypeUsingReflection();

		return SettablePropertiesPublicOnly
			.GetOrAdd(realType, t => t
				.GetCachedProperties()
				.Where(x => x.CanWrite
					&& (x.SetMethod != null)
					&& x.SetMethod.IsPublic)
				.ToHashSet()
				.AsReadOnly()
			);
	}

	#endregion
}