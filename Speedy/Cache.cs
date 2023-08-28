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
		NotifiableWriteables = new ConcurrentDictionary<Type, ReadOnlySet<string>>();
		PropertyDictionaryForType = new ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>>();
	}

	#endregion

	#region Properties

	/// <summary>
	/// Property dictionary for a sync object, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<Type, ReadOnlySet<string>> NotifiableWriteables { get; }

	/// <summary>
	/// Property dictionary for a sync object, this is for optimization
	/// </summary>
	private static ConcurrentDictionary<Type, IReadOnlyDictionary<string, PropertyInfo>> PropertyDictionaryForType { get; }

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

	internal static ReadOnlySet<string> GetWriteables(Notifiable value)
	{
		var realType = value.GetRealType();
		return GetWriteables(realType);
	}

	internal static ReadOnlySet<string> GetWriteables(Type realType)
	{
		return NotifiableWriteables
			.GetOrAdd(realType, t => t
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.Select(x => x.Name)
				.ToHashSet()
				.AsReadOnly()
			);
	}

	#endregion
}