#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Speedy.Data;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Activator help when creating new instances of types.
/// </summary>
public static class Activator
{
	#region Fields

	private static readonly ConcurrentDictionary<Type, TypeActivator> _typeActivators;

	#endregion

	#region Constructors

	static Activator()
	{
		_typeActivators = new ConcurrentDictionary<Type, TypeActivator>();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static T CreateInstance<T>(params object[] arguments)
	{
		return (T) CreateInstance(typeof(T), null, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="update"> An action to update the new instance. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static T CreateInstance<T>(Action<T> update, params object[] arguments)
	{
		return (T) CreateInstance(typeof(T), x => update((T) x), arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstance(Type type, params object[] arguments)
	{
		return CreateInstance(type, null, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="update"> An action to update the new instance. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstance(Type type, Action<object> update = null, params object[] arguments)
	{
		return CreateInstanceInternal(true, type, update, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="useCustomActivators"> True if we can use custom activators. </param>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="update"> An action to update the new instance. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	internal static object CreateInstanceInternal(bool useCustomActivators, Type type, Action<object> update = null, params object[] arguments)
	{
		object response;

		try
		{
			if (useCustomActivators && _typeActivators.TryGetValue(type, out var activator))
			{
				response = activator.CreateInstance(arguments);
			}
			else if (type.IsArray)
			{
				response = Array.CreateInstance(type.GetElementType() ?? type, 0);
			}
			else if (type.IsGenericTypeDefinition)
			{
				response = CreateInstanceOfGeneric(type, type.GenericTypeArguments, arguments);
			}
			else if (type.IsGenericType)
			{
				response = CreateInstanceOfGeneric(type, arguments);
			}
			else
			{
				if (type == typeof(string))
				{
					return null;
				}

				response = arguments is { Length: > 0 }
						? System.Activator.CreateInstance(type, arguments)
						: System.Activator.CreateInstance(type);
			}
		}
		catch (MissingMethodException ex)
		{
			// Add a bit more information.
			throw new MissingMethodException($"{type.FullName} missing requested constructor.", ex);
		}

		update?.Invoke(response);

		return response;
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="generics"> The Types the generic is for. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceOfGeneric(Type type, params Type[] generics)
	{
		return CreateInstanceOfGeneric(type, generics, Array.Empty<object>());
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="generics"> The Types the generic is for. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceOfGeneric(Type type, Type[] generics, params object[] arguments)
	{
		if (!type.IsGenericType || !type.IsGenericTypeDefinition)
		{
			throw new ArgumentException("The type provided is not a generic type definition.");
		}

		var genericType = type.GetCachedMakeGenericType(generics);
		return CreateInstanceOfGeneric(genericType, arguments);
	}

	/// <summary>
	/// Create an instance for a given Type.
	/// </summary>
	/// <param name="type"> The Type for which to get an instance of. </param>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public static object CreateInstanceOfGeneric(Type type, object[] arguments)
	{
		if (!type.IsGenericType || type.IsGenericTypeDefinition)
		{
			throw new ArgumentException("The type provided is not a generic type or is a generic type definition.");
		}

		if ((type.GetGenericTypeDefinition() == typeof(ICollection<>))
			||(type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			||(type.GetGenericTypeDefinition() == typeof(IList<>)))
		{
			type = typeof(List<>).GetCachedMakeGenericType(type.GenericTypeArguments);
		}
		else if (type.GetGenericTypeDefinition() == typeof(ISet<>))
		{
			type = typeof(HashSet<>).GetCachedMakeGenericType(type.GenericTypeArguments);
		}
		#if NET6_0_OR_GREATER
		else if (type.GetGenericTypeDefinition() == typeof(IReadOnlySet<>))
		{
			type = typeof(IReadOnlySet<>).GetCachedMakeGenericType(type.GenericTypeArguments);
		}
		#endif
		else if (type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
		{
			type = typeof(Dictionary<,>).GetCachedMakeGenericType(type.GenericTypeArguments);
		}
		else if (type.GetGenericTypeDefinition() == typeof(IReadOnlyDictionary<,>))
		{
			type = typeof(IReadOnlyDictionary<,>).GetCachedMakeGenericType(type.GenericTypeArguments);
		}
		else if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			type = typeof(Nullable<>).GetCachedMakeGenericType(type.GenericTypeArguments);
		}

		return arguments is { Length: > 0 }
			? System.Activator.CreateInstance(type, arguments)
			: System.Activator.CreateInstance(type);
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <typeparam name="T"> The type to create an instance of. </typeparam>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The instance of the type with non default values. </returns>
	public static T CreateInstanceWithNonDefaultValues<T>(params object[] arguments)
	{
		return (T) typeof(T).CreateInstanceWithNonDefaultValues(arguments);
	}

	/// <summary>
	/// Create a new instance of the type then update the object with non default values.
	/// </summary>
	/// <param name="type"> The type to create an instance of. </param>
	/// <param name="arguments"> The arguments for the constructing the instance. </param>
	/// <returns> The instance of the type with non default values. </returns>
	public static object CreateInstanceWithNonDefaultValues(Type type, params object[] arguments)
	{
		return type.CreateInstanceWithNonDefaultValues(arguments);
	}

	/// <summary>
	/// Register a type activator.
	/// </summary>
	/// <param name="activators"> The activators to provide type creation. </param>
	public static void RegisterTypeActivator(params TypeActivator[] activators)
	{
		foreach (var activator in activators)
		{
			_typeActivators.GetOrAdd(activator.Type, _ => activator);
		}
	}

	/// <summary>
	/// Reset the type activators.
	/// </summary>
	public static void ResetTypeActivators()
	{
		_typeActivators.Clear();
	}

	#endregion
}