#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using Speedy.Protocols.Osc;
using Speedy.Sync;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for all the things.
/// </summary>
public static class ReflectionExtensions
{
	#region Constants

	/// <summary>
	/// Default flags for cached access.
	/// </summary>
	public const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public;

	/// <summary>
	/// Default event flags for cached access.
	/// </summary>
	public const BindingFlags DefaultPrivateFlags = BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic;

	/// <summary>
	/// Flags for direct member only.
	/// </summary>
	public const BindingFlags DirectMemberFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

	#endregion

	#region Fields

	private static readonly byte[] _buffer;
	private static readonly ConcurrentDictionary<string, MethodInfo> _genericMethods;
	private static readonly ConcurrentDictionary<string, Type> _makeGenericTypes;
	private static readonly ConcurrentDictionary<string, ParameterInfo[]> _methodParameters;
	private static readonly ConcurrentDictionary<string, Type[]> _methodsGenericArgumentInfos;
	private static readonly ConcurrentDictionary<string, MethodInfo[]> _propertyGetAccessors;
	private static readonly RandomNumberGenerator _random;
	private static readonly ConcurrentDictionary<Type, string> _typeAssemblyNames;
	private static readonly ConcurrentDictionary<string, Attribute[]> _typeAttributes;
	private static readonly ConcurrentDictionary<string, List<FieldInfo>> _typeEventFieldInfos;
	private static readonly ConcurrentDictionary<string, FieldInfo[]> _typeFieldInfos;
	private static readonly ConcurrentDictionary<string, MethodInfo> _typeMethodInfos;
	private static readonly ConcurrentDictionary<string, MethodInfo[]> _typeMethodsInfos;
	private static readonly ConcurrentDictionary<string, Dictionary<string, PropertyInfo>> _typePropertyInfoDictionaries;
	private static readonly ConcurrentDictionary<string, PropertyInfo[]> _typePropertyInfos;
	private static readonly ConcurrentDictionary<string, PropertyInfo[]> _typeVirtualPropertyInfos;

	#endregion

	#region Constructors

	static ReflectionExtensions()
	{
		_buffer = new byte[32];
		_random = RandomNumberGenerator.Create();
		_genericMethods = new ConcurrentDictionary<string, MethodInfo>();
		_makeGenericTypes = new ConcurrentDictionary<string, Type>();
		_methodParameters = new ConcurrentDictionary<string, ParameterInfo[]>();
		_methodsGenericArgumentInfos = new ConcurrentDictionary<string, Type[]>();
		_propertyGetAccessors = new ConcurrentDictionary<string, MethodInfo[]>();
		_typeAssemblyNames = new ConcurrentDictionary<Type, string>();
		_typeAttributes = new ConcurrentDictionary<string, Attribute[]>();
		_typeEventFieldInfos = new ConcurrentDictionary<string, List<FieldInfo>>();
		_typeFieldInfos = new ConcurrentDictionary<string, FieldInfo[]>();
		_typeMethodInfos = new ConcurrentDictionary<string, MethodInfo>();
		_typeMethodsInfos = new ConcurrentDictionary<string, MethodInfo[]>();
		_typePropertyInfoDictionaries = new ConcurrentDictionary<string, Dictionary<string, PropertyInfo>>();
		_typePropertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
		_typeVirtualPropertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Create an instance for a given property.
	/// </summary>
	/// <param name="propertyInfo"> The property type for which to get an instance of. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the property type. </returns>
	public static object CreateInstance(this PropertyInfo propertyInfo, params object[] arguments)
	{
		return Activator.CreateInstance(propertyInfo.PropertyType, arguments);
	}

	/// <summary>
	/// Create an instance for a given property.
	/// </summary>
	/// <param name="propertyInfo"> The property type for which to get an instance of. </param>
	/// <param name="update"> An action to update the new instance. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the property type. </returns>
	public static object CreateInstance(this PropertyInfo propertyInfo, Action<object> update = null, params object[] arguments)
	{
		return Activator.CreateInstance(propertyInfo.PropertyType, update, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstance(this Type type, params object[] arguments)
	{
		return Activator.CreateInstance(type, null, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="update"> An action to update the new instance. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstance(this Type type, Action<object> update = null, params object[] arguments)
	{
		return Activator.CreateInstance(type, update, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceOfGeneric(this Type type, params object[] arguments)
	{
		return Activator.CreateInstanceOfGeneric(type, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="generics"> The Types the generic is for. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceOfGeneric(this Type type, Type[] generics, params object[] arguments)
	{
		return Activator.CreateInstanceOfGeneric(type, generics, arguments);
	}

	/// <summary>
	/// Get a non default value for a property.
	/// </summary>
	/// <param name="propertyInfo"> The property info. </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceWithNonDefaultValue(this PropertyInfo propertyInfo, params object[] arguments)
	{
		var propertyType = propertyInfo.PropertyType;
		return CreateInstanceWithNonDefaultValue(propertyType, arguments);
	}

	/// <summary>
	/// Get a non default value for data type.
	/// </summary>
	/// <param name="type"> The type of object to get the value for. </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceWithNonDefaultValue(this Type type, params object[] arguments)
	{
		if (type.IsEnum)
		{
			var details = EnumExtensions.GetAllEnumDetails(type).Values.ToList();
			return details.Count >= 2
				? details[1].Value
				: details.FirstOrDefault().Value;
		}
		if ((type == typeof(bool)) || (type == typeof(bool?)))
		{
			return true;
		}
		if ((type == typeof(byte)) || (type == typeof(byte?)))
		{
			_random.GetBytes(_buffer, 0, 1);
			return _buffer[0];
		}
		if ((type == typeof(char)) || (type == typeof(char?)))
		{
			var value = RandomGenerator.NextInteger(char.MinValue, char.MaxValue);
			return (char) (value == 0 ? 1 : value);
		}
		if ((type == typeof(DateTime)) || (type == typeof(DateTime?)))
		{
			return TimeService.UtcNow;
		}
		if ((type == typeof(DateTimeOffset)) || (type == typeof(DateTimeOffset?)))
		{
			return DateTimeOffset.UtcNow;
		}
		if ((type == typeof(OscTimeTag)) || (type == typeof(OscTimeTag?)))
		{
			return OscTimeTag.UtcNow;
		}
		if ((type == typeof(TimeSpan)) || (type == typeof(TimeSpan?)))
		{
			var ticks = RandomGenerator.NextLong(1, TimeSpan.FromDays(30).Ticks);
			return TimeSpan.FromTicks(ticks == 0 ? 1 : ticks);
		}
		if ((type == typeof(double)) || (type == typeof(double?)))
		{
			return RandomGenerator.NextDouble(-100000, 100000);
		}
		if ((type == typeof(float)) || (type == typeof(float?)))
		{
			return (float) RandomGenerator.NextDouble(-100000, 100000);
		}
		if ((type == typeof(decimal)) || (type == typeof(decimal?)))
		{
			return RandomGenerator.NextDecimal(-100000, 100000);
		}
		if ((type == typeof(Guid)) || (type == typeof(Guid?)))
		{
			return Guid.NewGuid();
		}
		if ((type == typeof(ShortGuid)) || (type == typeof(ShortGuid?)))
		{
			return ShortGuid.NewGuid();
		}
		if ((type == typeof(byte)) || (type == typeof(byte?)))
		{
			return (byte) RandomGenerator.NextInteger(1, byte.MaxValue);
		}
		if ((type == typeof(sbyte)) || (type == typeof(sbyte?)))
		{
			var value = RandomGenerator.NextInteger(sbyte.MinValue, sbyte.MaxValue);
			return (sbyte) (value == 0 ? 1 : value);
		}
		if ((type == typeof(IntPtr)) || (type == typeof(IntPtr?)))
		{
			var value = RandomGenerator.NextInteger(int.MinValue + 1, int.MaxValue - 1);
			return new IntPtr(value == 0 ? 1 : value);
		}
		if ((type == typeof(UIntPtr)) || (type == typeof(UIntPtr?)))
		{
			var value = (ulong) RandomGenerator.NextLong(uint.MinValue + 1, uint.MaxValue - 1);
			return new UIntPtr(value == 0 ? 1u : value);
		}
		if ((type == typeof(int)) || (type == typeof(int?)))
		{
			var value = RandomGenerator.NextInteger(int.MinValue + 1, int.MaxValue - 1);
			return value == 0 ? 1 : value;
		}
		if ((type == typeof(uint)) || (type == typeof(uint?)))
		{
			return (uint) RandomGenerator.NextInteger(1, int.MaxValue / 4);
		}
		if ((type == typeof(long)) || (type == typeof(long?)))
		{
			var value = RandomGenerator.NextLong(long.MinValue / 4, long.MaxValue / 4);
			return value == 0 ? 1 : value;
		}
		if ((type == typeof(ulong)) || (type == typeof(ulong?)))
		{
			return (ulong) RandomGenerator.NextLong(1, long.MaxValue / 4);
		}
		if ((type == typeof(short)) || (type == typeof(short?)))
		{
			var value = RandomGenerator.NextInteger(short.MinValue / 4, short.MaxValue / 4);
			return (short) (value == 0 ? 1 : value);
		}
		if ((type == typeof(ushort)) || (type == typeof(ushort?)))
		{
			return (ushort) RandomGenerator.NextInteger(1, ushort.MaxValue / 4);
		}
		if (type == typeof(string))
		{
			return Guid.NewGuid().ToString();
		}

		if (type == typeof(Rectangle))
		{
			return new Rectangle(1, 2, 3, 4);
		}

		return Activator.CreateInstance(type, arguments);
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The instance of the type with non default values. </returns>
	public static T CreateInstanceWithNonDefaultValues<T>(params object[] arguments)
	{
		return (T) Activator.CreateInstanceWithNonDefaultValues(typeof(T), arguments);
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <param name="type"> The type to create an instance of. </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	/// <returns> The instance of the type with non default values. </returns>
	public static object CreateInstanceWithNonDefaultValues(this Type type, object[] arguments, params string[] exclusions)
	{
		var response = Activator.CreateInstance(type, null, arguments);
		var notifiable = response as Notifiable;
		notifiable?.DisablePropertyChangeNotifications();
		response.UpdateWithNonDefaultValues(exclusions);
		notifiable?.EnablePropertyChangeNotifications();
		return response;
	}

	/// <summary>
	/// Gets a list of generic arguments for the provided property information. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="info"> The property information to get the generic arguments for. </param>
	/// <returns> The list of generic arguments for the property information of the value. </returns>
	public static IList<MethodInfo> GetCachedAccessors(this PropertyInfo info)
	{
		if (info == null)
		{
			throw new InvalidOperationException();
		}

		var key = $"{info.ReflectedType?.FullName}.{info.Name}";
		return _propertyGetAccessors.GetOrAdd(key, _ => info.GetAccessors());
	}

	/// <summary>
	/// Get attributes for an enum.
	/// </summary>
	/// <typeparam name="T"> The type value of the attribute. </typeparam>
	/// <param name="value"> The value to get attributes for. </param>
	/// <returns> The attributes if found. </returns>
	public static T[] GetCachedAttributes<T>(this Enum value) where T : Attribute
	{
		var attributeType = typeof(T);
		var typeKey = GetCacheKey(value?.GetType() ?? throw new InvalidOperationException(), DefaultFlags);
		var key = typeKey + value + attributeType.FullName;

		return _typeAttributes.GetOrAdd(key, _ =>
			{
				var type = value.GetType();
				var memberInfo = type.GetMember(value.ToString());
				var attributes = memberInfo[0]
					.GetCustomAttributes(attributeType, false)
					.Cast<Attribute>()
					.ToArray();

				return attributes;
			})
			.Cast<T>()
			.ToArray();
	}

	/// <summary>
	/// Gets a list of event information for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="value"> The value to get the events for. </param>
	/// <param name="name"> The type event field name to locate. </param>
	/// <param name="flags"> The flags to find events by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of field info of the events for the type. </returns>
	public static FieldInfo GetCachedEventField(this object value, string name, BindingFlags? flags = null)
	{
		return GetCachedEventField(value.GetType(), name, flags);
	}

	/// <summary>
	/// Gets a list of event information for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the events for. </param>
	/// <param name="name"> The type event field name to locate. </param>
	/// <param name="flags"> The flags to find events by. Defaults to Public, Non Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of field info of the events for the type. </returns>
	public static FieldInfo GetCachedEventField(this Type type, string name, BindingFlags? flags = null)
	{
		return type.GetCachedEventFields(flags).FirstOrDefault(x => x.Name == name);
	}

	/// <summary>
	/// Gets a list of event information for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="value"> The value to get the events for. </param>
	/// <param name="flags"> The flags to find events by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of field info of the events for the type. </returns>
	public static IList<FieldInfo> GetCachedEventFields(this object value, BindingFlags? flags = null)
	{
		var type = value.GetType();
		return GetCachedEventFields(type, flags);
	}

	/// <summary>
	/// Gets a list of event information for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the events for. </param>
	/// <param name="flags"> The flags to find events by. Defaults to Public, Non Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of field info of the events for the type. </returns>
	public static IList<FieldInfo> GetCachedEventFields(this Type type, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultPrivateFlags;
		var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);

		return _typeEventFieldInfos.GetOrAdd(key, _ => type
			.GetEvents(typeFlags)
			.Where(x => x.DeclaringType != null)
			.Select(x => x.DeclaringType.GetField(x.Name, typeFlags))
			.Where(x => x != null)
			.ToList()
		);
	}

	/// <summary>
	/// Gets a field by name for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="item"> The item to get the field for. </param>
	/// <param name="name"> The type field name to locate. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The field information for the type. </returns>
	public static FieldInfo GetCachedField(this object item, string name, BindingFlags? flags = null)
	{
		return GetCachedField(item.GetType(), name, flags);
	}

	/// <summary>
	/// Gets a field by name for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the fields for. </param>
	/// <param name="name"> The type field name to locate. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The field information for the type. </returns>
	public static FieldInfo GetCachedField(this Type type, string name, BindingFlags? flags = null)
	{
		return type.GetCachedFields(flags).FirstOrDefault(x => x.Name == name);
	}

	/// <summary>
	/// Gets a list of fields for the provided item. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="item"> The item to get the fields for. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The list of field infos for the item. </returns>
	public static IList<FieldInfo> GetCachedFields(this object item, BindingFlags? flags = null)
	{
		return item.GetType().GetCachedFields(flags);
	}

	/// <summary>
	/// Gets a list of fields for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the fields for. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The list of field infos for the type. </returns>
	public static IList<FieldInfo> GetCachedFields(this Type type, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultFlags;
		var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);
		return _typeFieldInfos.GetOrAdd(key, _ => type.GetFields(typeFlags));
	}

	/// <summary>
	/// Gets a list of generic arguments for the provided method information. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="info"> The method information to get the generic arguments for. </param>
	/// <returns> The list of generic arguments for the method information of the value. </returns>
	public static IList<Type> GetCachedGenericArguments(this MethodInfo info)
	{
		var fullName = $"{info.ReflectedType?.FullName}.{info.Name}";
		var key = info.ToString()?.Replace(info.Name, fullName);
		return _methodsGenericArgumentInfos.GetOrAdd(key, _ => info.GetGenericArguments());
	}

	/// <summary>
	/// Gets a list of generic arguments for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the generic arguments for. </param>
	/// <returns> The list of generic arguments for the type of the value. </returns>
	public static IList<Type> GetCachedGenericArguments(this Type type)
	{
		return _methodsGenericArgumentInfos.GetOrAdd(type.FullName ?? throw new InvalidOperationException(),
			_ => type.GetGenericArguments());
	}

	/// <summary>
	/// Substitutes the elements of an array of types for the type parameters of the current generic method definition, and returns a
	/// MethodInfo object representing the resulting constructed method. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="info"> The property information to get the generic arguments for. </param>
	/// <param name="arguments"> An array of types to be substituted for the type parameters of the current generic method definition. </param>
	/// <returns> The method information with generics. </returns>
	public static MethodInfo GetCachedMakeGenericMethod(this MethodInfo info, Type[] arguments)
	{
		var fullName = $"{info.ReflectedType?.FullName}.{info.Name}";
		var key = info.ToString()?.Replace(info.Name, fullName) + string.Join(", ", arguments.Select(x => x.FullName));
		return _genericMethods.GetOrAdd(key, _ => info.MakeGenericMethod(arguments));
	}

	/// <summary>
	/// Get the type of a generic with the provided types. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the generic arguments for. </param>
	/// <param name="genericTypes"> An array of types to create the generic type with. </param>
	/// <returns> The method information with generics. </returns>
	public static Type GetCachedMakeGenericType(this Type type, Type[] genericTypes)
	{
		var key = $"{type.FullName}:{string.Join(", ", genericTypes.Select(x => x.FullName))}";
		return _makeGenericTypes.GetOrAdd(key, _ => type.MakeGenericType(genericTypes));
	}

	/// <summary>
	/// Searches for the specified public method whose parameters match the specified argument types.
	/// The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the method for. </param>
	/// <param name="name"> The string containing the name of the public method to get. </param>
	/// <param name="types"> An array of type objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of type objects (as provided by the EmptyTypes field) to get a method that takes no parameters. </param>
	/// <returns> An object representing the public method whose parameters match the specified argument types, if found; otherwise null. </returns>
	public static MethodInfo GetCachedMethod(this Type type, string name, params Type[] types)
	{
		var typeKey = GetCacheKey(type, DefaultFlags);
		var methodKey = typeKey + name;
		return _typeMethodInfos.GetOrAdd(methodKey, _ => types.Any() ? type.GetMethod(name, types) : type.GetMethod(name));
	}

	/// <summary>
	/// Gets the method info from the provided type by the name provided.
	/// The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="value"> The value to get the methods for. </param>
	/// <param name="name"> The name of the method to be queried. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The list of method infos for the type. </returns>
	public static MethodInfo GetCachedMethod(this object value, string name, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultFlags;
		var typeKey = GetCacheKey(value?.GetType() ?? throw new InvalidOperationException(), typeFlags);
		var methodKey = typeKey + name;
		return _typeMethodInfos.GetOrAdd(methodKey, GetCachedMethods(value, flags).FirstOrDefault(x => x.Name == name));
	}

	/// <summary>
	/// Gets a list of methods for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="value"> The value to get the methods for. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The list of method infos for the type. </returns>
	public static IList<MethodInfo> GetCachedMethods(this object value, BindingFlags? flags = null)
	{
		return GetCachedMethods(value?.GetType(), flags);
	}

	/// <summary>
	/// Gets a list of methods for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the methods for. </param>
	/// <param name="flags"> The flags used to query with. </param>
	/// <returns> The list of method infos for the type. </returns>
	public static IList<MethodInfo> GetCachedMethods(this Type type, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultFlags;
		var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);
		return _typeMethodsInfos.GetOrAdd(key, type.GetMethods(typeFlags));
	}

	/// <summary>
	/// Gets a list of parameter infos for the provided method info. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="info"> The method info to get the parameters for. </param>
	/// <returns> The list of parameter infos for the type. </returns>
	public static IList<ParameterInfo> GetCachedParameters(this MethodInfo info)
	{
		if (info == null)
		{
			throw new InvalidOperationException();
		}

		var reflectedName = info.ReflectedType?.FullName;
		var fullName = reflectedName != null ? $"{reflectedName}.{info.Name}" : info.Name;
		var key = info.ToString()?.Replace(info.Name, fullName);

		return _methodParameters.GetOrAdd(key, _ => info.GetParameters());
	}

	/// <summary>
	/// Gets a list of property types for the provided object type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="value"> The value to get the properties for. </param>
	/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of properties for the type of the value. </returns>
	public static IList<PropertyInfo> GetCachedProperties(this object value, BindingFlags? flags = null)
	{
		return value.GetType().GetCachedProperties(flags);
	}

	/// <summary>
	/// Gets a list of property information for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the properties for. </param>
	/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of properties for the type. </returns>
	public static IList<PropertyInfo> GetCachedProperties(this Type type, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultFlags;
		var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);
		return _typePropertyInfos.GetOrAdd(key, _ =>
		{
			return type.GetProperties(typeFlags)
				.OrderBy(x => x.Name)
				.ToArray();
		});
	}

	/// <summary>
	/// Gets the information for the provided type and property name. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the property for. </param>
	/// <param name="name"> The name of the property to be queried. </param>
	/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of properties for the type. </returns>
	public static PropertyInfo GetCachedProperty(this Type type, string name, BindingFlags? flags = null)
	{
		return GetCachedProperties(type, flags).FirstOrDefault(x => x.Name == name);
	}

	/// <summary>
	/// Gets a list of property information for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the properties for. </param>
	/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of properties for the type. </returns>
	public static IDictionary<string, PropertyInfo> GetCachedPropertyDictionary(this Type type, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultFlags;
		var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);
		return _typePropertyInfoDictionaries.GetOrAdd(key, _ =>
		{
			var properties = type.GetProperties(typeFlags);
			return properties.ToDictionary(p => p.Name, p => p, StringComparer.InvariantCultureIgnoreCase);
		});
	}

	/// <summary>
	/// Gets a list of virtual property types for the provided type. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The type to get the properties for. </param>
	/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of properties for the type. </returns>
	public static IList<PropertyInfo> GetCachedVirtualProperties(this Type type, BindingFlags? flags = null)
	{
		var typeFlags = flags ?? DefaultFlags;
		var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);

		return _typeVirtualPropertyInfos.GetOrAdd(key, _ =>
		{
			return type
				.GetCachedProperties(typeFlags)
				.Where(p => p.IsVirtual())
				.OrderBy(p => p.Name)
				.ToArray();
		});
	}

	/// <summary>
	/// Get a default value for a property.
	/// </summary>
	/// <param name="propertyInfo"> The property info. </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The new instances of the type. </returns>
	public static object GetDefaultValue(this PropertyInfo propertyInfo, params object[] arguments)
	{
		return propertyInfo.PropertyType.GetDefaultValue(arguments);
	}

	/// <summary>
	/// Retrieves the default value for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get the default value </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The new instances of the type. </returns>
	/// <remarks>
	/// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type
	/// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an
	/// exception.
	/// </remarks>
	public static object GetDefaultValue(this Type type, params object[] arguments)
	{
		if ((type == null) || (type == typeof(string)) || (type == typeof(void)))
		{
			return null;
		}

		if (type.IsNullable()
			&& !type.IsArray
			&& !type.ContainsGenericParameters
			&& (type.GenericTypeArguments.Length <= 0))
		{
			return null;
		}

		var isDictionary = type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
		if (isDictionary)
		{
			var collectionType = typeof(Dictionary<,>);
			var constructedListType = collectionType.MakeGenericType(type.GenericTypeArguments);
			return Activator.CreateInstance(constructedListType);
		}

		var isEnumerable = type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		if (isEnumerable)
		{
			var collectionType = typeof(Collection<>);
			var constructedListType = collectionType.MakeGenericType(type.GenericTypeArguments);
			return Activator.CreateInstance(constructedListType);
		}

		var isCollection = type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ICollection<>));
		if (isCollection)
		{
			var collectionType = typeof(Collection<>);
			var constructedListType = collectionType.MakeGenericType(type.GenericTypeArguments);
			return Activator.CreateInstance(constructedListType);
		}

		var isList = type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IList<>));
		if (isList)
		{
			var collectionType = typeof(List<>);
			var constructedListType = collectionType.MakeGenericType(type.GenericTypeArguments);
			return Activator.CreateInstance(constructedListType);
		}

		if (type.IsArray)
		{
			return Array.CreateInstance(type.GetElementType() ?? typeof(object), 0);
		}

		// If the supplied Type has generic parameters, its default value cannot be determined
		if (type.ContainsGenericParameters)
		{
			throw new ArgumentException(
				"{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
				"> contains generic parameters, so the default value cannot be retrieved");
		}

		// If the Type is a primitive type, or if it is another publicly-visible value type (i.e. struct/enum), return a 
		//  default instance of the value type
		if (type.IsPrimitive || !type.IsNotPublic)
		{
			try
			{
				return Activator.CreateInstance(type, arguments);
			}
			catch (Exception e)
			{
				throw new ArgumentException(
					"{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe Activator.CreateInstance method could not " +
					"create a default instance of the supplied value type <" + type +
					"> (Inner Exception message: \"" + e.Message + "\")", e);
			}
		}

		// Fail with exception
		throw new ArgumentException("{" + MethodBase.GetCurrentMethod() + "} Error:\n\nThe supplied value type <" + type +
			"> is not a publicly-visible type, so the default value cannot be retrieved");
	}

	/// <summary>
	/// Get a default value for a property.
	/// </summary>
	/// <param name="propertyInfo"> The property info. </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The new instances of the type. </returns>
	public static object GetDefaultValueNotNull(this PropertyInfo propertyInfo, params object[] arguments)
	{
		var response = propertyInfo?.PropertyType.GetDefaultValue()
			?? Activator.CreateInstance(propertyInfo?.PropertyType, arguments);

		if (response != null)
		{
			return response;
		}

		if (propertyInfo?.PropertyType == typeof(string))
		{
			return string.Empty;
		}

		return Activator.CreateInstance(propertyInfo?.PropertyType ?? typeof(object));
	}

	/// <summary>
	/// Get the name of the expression.
	/// </summary>
	/// <param name="expression"> The expression to process. </param>
	/// <returns> The name of the expression. </returns>
	public static string GetExpressionName(this LambdaExpression expression)
	{
		if (expression.Body is UnaryExpression unaryExpression)
		{
			return ((dynamic) unaryExpression.Operand).Member?.Name;
		}

		return ((dynamic) expression).Body.Member.Name;
	}

	/// <summary>
	/// Get the types for the generic.
	/// </summary>
	/// <param name="value"> The value to get the types for. </param>
	/// <returns> The type values for the generic object. </returns>
	public static Type[] GetGenericTypes(this object value)
	{
		var type = value.GetType();

		while (true)
		{
			if (type == null)
			{
				return null;
			}

			#if NET6_0_OR_GREATER
			var arg = type.GenericTypeArguments.ToArray();
			#else
			var arg = type.GenericTypeArguments?.ToArray();
			#endif

			if (arg is not { Length: > 0 })
			{
				type = type.BaseType;
				continue;
			}

			return arg;
		}
	}

	/// <summary>
	/// Gets the public or private member using reflection.
	/// </summary>
	/// <param name="value"> The value that contains the member. </param>
	/// <param name="memberName"> The name of the field or property to get the value of. </param>
	/// <returns> The value of member. </returns>
	public static object GetMemberValue(this object value, string memberName)
	{
		var memberInfo = GetCachedMember(value, memberName);

		if (memberInfo == null)
		{
			throw new InvalidOperationException();
		}

		return memberInfo switch
		{
			PropertyInfo propertyInfo => propertyInfo.GetValue(value, null),
			FieldInfo fieldInfo => fieldInfo.GetValue(value),
			_ => throw new Exception()
		};
	}

	/// <summary>
	/// Gets the member value of an object using the provider info.
	/// </summary>
	/// <param name="memberInfo"> The info for the member. </param>
	/// <param name="value"> </param>
	/// <returns> The value of the value member. </returns>
	/// <exception cref="NotImplementedException"> The member info is not a field or property. </exception>
	public static object GetMemberValue(this MemberInfo memberInfo, object value)
	{
		return memberInfo.MemberType switch
		{
			MemberTypes.Field => ((FieldInfo) memberInfo).GetValue(value),
			MemberTypes.Property => ((PropertyInfo) memberInfo).GetValue(value),
			_ => throw new NotImplementedException()
		};
	}

	/// <summary>
	/// Gets the real type of the entity. For use with proxy entities.
	/// </summary>
	/// <param name="item"> The object to process. </param>
	/// <returns> The real base type for the proxy or just the initial type if it is not a proxy. </returns>
	public static Type GetRealTypeUsingReflection(this object item)
	{
		var type = item.GetType();
		return GetRealTypeUsingReflection(type);
	}

	/// <summary>
	/// Gets the real type of the entity. For use with proxy entities.
	/// </summary>
	/// <param name="type"> The type to process. </param>
	/// <returns> The real base type for the proxy or just the initial type if it is not a proxy. </returns>
	public static Type GetRealTypeUsingReflection(this Type type)
	{
		var isProxy = (type.FullName?.Contains("System.Data.Entity.DynamicProxies") == true)
			|| (type.FullName?.Contains("Castle.Proxies") == true);

		return isProxy ? type.BaseType : type;
	}

	/// <summary>
	/// Gets a list of virtual property names. The results are cached so the next query is much faster.
	/// </summary>
	/// <param name="type"> The value to get the property names for. </param>
	/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
	/// <returns> The list of virtual property names for the type. </returns>
	public static IEnumerable<string> GetVirtualPropertyNames(this Type type, BindingFlags? flags = null)
	{
		return GetCachedVirtualProperties(type, flags)
			.Select(x => x.Name)
			.ToArray();
	}

	/// <summary>
	/// Determine if the property is a abstract property.
	/// </summary>
	/// <param name="info"> The info to process. </param>
	/// <returns> True if the accessor is abstract. </returns>
	public static bool IsAbstract(this PropertyInfo info)
	{
		return (info.CanRead
				&& (info.GetMethod != null)
				&& info.GetMethod.IsAbstract)
			|| (info.CanWrite
				&& (info.SetMethod != null)
				&& info.SetMethod.IsAbstract);
	}

	/// <summary>
	/// Determine if the property is a virtual property.
	/// </summary>
	/// <param name="info"> The info to process. </param>
	/// <returns> True if the accessor is virtual. </returns>
	public static bool IsVirtual(this PropertyInfo info)
	{
		return (info.CanRead
				&& (info.GetMethod != null)
				&& info.GetMethod.IsVirtual
				&& !info.GetMethod.IsFinal
				&& info.GetMethod.Attributes.HasFlag(MethodAttributes.VtableLayoutMask))
			|| (info.CanWrite
				&& (info.SetMethod != null)
				&& info.SetMethod.IsVirtual
				&& !info.SetMethod.IsFinal
				&& info.SetMethod.Attributes.HasFlag(MethodAttributes.VtableLayoutMask));
	}

	/// <summary>
	/// Gets the public or private member using reflection.
	/// </summary>
	/// <param name="obj"> The target object. </param>
	/// <param name="memberName"> Name of the field or property. </param>
	/// <param name="newValue"> The new value to be set. </param>
	/// <returns> Old Value </returns>
	public static object SetMemberValue(this object obj, string memberName, object newValue)
	{
		var memInf = GetCachedMember(obj, memberName);

		if (memInf == null)
		{
			throw new Exception("memberName");
		}

		var oldValue = obj.GetMemberValue(memberName);

		switch (memInf)
		{
			case PropertyInfo propertyInfo:
			{
				propertyInfo.SetValue(obj, newValue, null);
				break;
			}
			case FieldInfo fieldInfo:
			{
				fieldInfo.SetValue(obj, newValue);
				break;
			}
			default:
			{
				throw new Exception();
			}
		}

		return oldValue;
	}

	/// <summary>
	/// Converts the type to an assembly name. Does not include version. Ex. System.String,mscorlib
	/// </summary>
	/// <param name="type"> The type to get the assembly name for. </param>
	/// <returns> The assembly name for the provided type. </returns>
	public static string ToAssemblyName(this Type type)
	{
		return _typeAssemblyNames.GetOrAdd(type, $"{type.FullName},{type.Assembly.GetName().Name}");
	}

	/// <summary>
	/// Update the provided object with non default values.
	/// </summary>
	/// <typeparam name="T"> The type of the value. </typeparam>
	/// <param name="model"> The value to update all properties for. </param>
	/// <param name="exclusions"> An optional set of properties to exclude. </param>
	/// <returns> The type updated with non default values. </returns>
	public static T UpdateWithNonDefaultValues<T>(this T model, params string[] exclusions)
	{
		var notifiable = model as INotifiable;
		notifiable?.DisablePropertyChangeNotifications();

		var allExclusions = GetExclusions(model);
		allExclusions.AddRange(exclusions);

		try
		{
			var properties = Cache.GetSettablePropertiesPublicOnly(model)
				.Where(x => !allExclusions.Contains(x.Name))
				.OrderBy(x => x.Name)
				.ToList();

			foreach (var property in properties)
			{
				var nonDefaultValue = CreateInstanceWithNonDefaultValue(property);
				property.SetValue(model, nonDefaultValue);
			}

			return model;
		}
		finally
		{
			notifiable?.EnablePropertyChangeNotifications();
		}
	}

	/// <summary>
	/// Validates that all values are not default value.
	/// </summary>
	/// <typeparam name="T"> The type of the model. </typeparam>
	/// <param name="model"> The model to be validated. </param>
	/// <param name="exclusions"> An optional set of exclusions. </param>
	public static void ValidateAllValuesAreNotDefault<T>(this T model, params string[] exclusions)
	{
		var allExclusions = GetExclusions(model);
		allExclusions.AddRange(exclusions);

		var properties = Cache
			.GetSettablePropertiesPublicOnly(model)
			.Where(x => !allExclusions.Contains(x.Name))
			.ToList();

		foreach (var property in properties)
		{
			if ((exclusions.Length > 0) && exclusions.Contains(property.Name))
			{
				continue;
			}

			var propertyValue = property.GetValue(model);
			var defaultValue = property.PropertyType.GetDefaultValue();

			if (propertyValue?.Equals(defaultValue) == true)
			{
				continue;
			}

			if (Equals(propertyValue, defaultValue))
			{
				throw new Exception($"Property {model.GetType().Name}.{property.Name} should have been set but was not.");
			}
		}
	}

	internal static string GetCacheKey(Type type, BindingFlags flags)
	{
		return type.FullName + flags;
	}

	private static MemberInfo FindField(Type type, string memberName, BindingFlags flags)
	{
		var field = type.GetCachedField(memberName, flags);
		if (field != null)
		{
			return field;
		}

		if (type.BaseType == typeof(object))
		{
			return null;
		}

		return FindField(type.BaseType, memberName, flags);
	}

	private static MemberInfo GetCachedMember(object obj, string memberName)
	{
		var type = obj?.GetType() ?? throw new ArgumentNullException(nameof(obj));
		var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
		var info = (MemberInfo) type.GetCachedProperty(memberName, flags);

		if (info != null)
		{
			return info;
		}

		info = FindField(obj.GetType(), memberName, flags);
		return info;
	}

	/// <summary>
	/// Get exclusions for the provided type. Currently this returns all sync exclusions.
	/// todo: add an "exclusion" interface so we can exclude on any model.
	/// </summary>
	/// <typeparam name="T"> The type of the model. </typeparam>
	/// <param name="model"> The model to get exclusions for. </param>
	/// <returns> The exclusions. </returns>
	private static HashSet<string> GetExclusions<T>(this T model)
	{
		return model switch
		{
			ISyncEntity t => t.GetSyncExclusions(true, true, true),
			IUpdateableExclusions t => t.GetUpdatableExclusions(),
			_ => new HashSet<string>()
		};
	}

	#endregion
}