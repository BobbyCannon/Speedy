#region References

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for the Type object.
/// </summary>
public static class TypeExtensions
{
	#region Fields

	private static readonly Type _enumerableType;
	private static readonly Type _stringType;

	#endregion

	#region Constructors

	static TypeExtensions()
	{
		_enumerableType = typeof(IEnumerable);
		_stringType = typeof(string);
	}

	#endregion

	#region Methods

	/// <summary>
	/// Get direct implemented interfaces for a type.
	/// </summary>
	/// <param name="type"> The type to get the interfaces for. </param>
	/// <returns> The direct interfaces for a type. </returns>
	public static IEnumerable<Type> GetDirectInterfaces(this Type type)
	{
		var interfaces = type.GetInterfaces();
		var baseType = type.BaseType;
		var result = new HashSet<Type>(interfaces);
		foreach (var i in interfaces)
		{
			result.ExceptWith(i.GetInterfaces());
			if (baseType != null)
			{
				result.ExceptWith(baseType.GetInterfaces());
				baseType = baseType.BaseType;
			}
		}
		return result;
	}

	/// <summary>
	/// Returns true if the object is a descendent of the provided generic type
	/// </summary>
	/// <typeparam name="T"> The parent type. </typeparam>
	/// <param name="value"> The value to check. </param>
	/// <returns> </returns>
	/// <exception cref="ArgumentNullException"> The value was null and could not be processed. </exception>
	public static bool IsDescendantOf<T>(this object value)
	{
		return value is Type vType
			? IsDescendantOf<T>(vType)
			: IsDescendantOf<T>(value.GetType());
	}

	/// <summary>
	/// Returns true if the object is a descendent of the provided generic type
	/// </summary>
	/// <typeparam name="T"> The parent type. </typeparam>
	/// <param name="value"> The value to check. </param>
	/// <returns> </returns>
	/// <exception cref="ArgumentNullException"> The value was null and could not be processed. </exception>
	public static bool IsDescendantOf<T>(this Type value)
	{
		if (value == null)
		{
			throw new ArgumentNullException(nameof(value), "The value was null and could not be processed.");
		}

		return value.IsDescendantOf(typeof(T));
	}

	/// <summary>
	/// Returns true if the object is a descendent of the provided generic type
	/// </summary>
	/// <param name="parent"> The parent type. </param>
	/// <param name="valueType"> The value type to check. </param>
	/// <returns> </returns>
	/// <exception cref="ArgumentNullException"> The valueType was null and could not be processed. </exception>
	public static bool IsDescendantOf(this Type valueType, Type parent)
	{
		if (valueType == null)
		{
			throw new ArgumentNullException(nameof(valueType), "The valueType was null and could not be processed.");
		}

		if (parent.IsInterface)
		{
			return parent.IsAssignableFrom(valueType);
		}

		return parent.IsGenericType
			? valueType.IsSubClassOfGeneric(parent, false)
			: valueType.IsSubclassOf(parent);
	}

	/// <summary>
	/// Returns true if the object is a direct descendent of the provided generic type
	/// </summary>
	/// <param name="parent"> The parent type. </param>
	/// <param name="valueType"> The value type to check. </param>
	/// <returns> </returns>
	/// <exception cref="ArgumentNullException"> The valueType was null and could not be processed. </exception>
	public static bool IsDirectDescendantOf(this Type valueType, Type parent)
	{
		if (valueType == null)
		{
			throw new ArgumentNullException(nameof(valueType), "The valueType was null and could not be processed.");
		}

		if (parent.IsInterface)
		{
			return GetDirectInterfaces(valueType).Contains(parent);
		}

		return parent.IsGenericType
			? valueType.IsSubClassOfGeneric(parent, true)
			: valueType.BaseType == parent;
	}

	/// <summary>
	/// Determine if the provided type is an IEnumerable type.
	/// </summary>
	/// <param name="type"> The type to be checked. </param>
	/// <returns> Returns true if the type is an IEnumerable false otherwise. </returns>
	/// <remarks> Ignores the following types "string". </remarks>
	public static bool IsEnumerable(this Type type)
	{
		return _enumerableType.IsAssignableFrom(type)
			&& (_stringType != type);
	}

	/// <summary>
	/// Determines if a type is nullable.
	/// </summary>
	/// <param name="type"> The type to be tested. </param>
	/// <returns> True if the type is nullable otherwise false. </returns>
	public static bool IsNullable(this Type type)
	{
		return type.IsClass
			|| !type.IsValueType
			|| (Nullable.GetUnderlyingType(type) != null);
	}

	private static Type GetFullTypeDefinition(Type type)
	{
		return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
	}

	/// <summary>
	/// Determines if the child is a subclass of the parent.
	/// </summary>
	/// <param name="child"> The type to be tested. </param>
	/// <param name="parent"> The type of the parent. </param>
	/// <param name="directDescendantOnly"> Direct base class only. </param>
	/// <returns> True if the child implements the parent otherwise false. </returns>
	private static bool IsSubClassOfGeneric(this Type child, Type parent, bool directDescendantOnly)
	{
		if (child == parent)
		{
			return false;
		}

		if (child.IsSubclassOf(parent))
		{
			return true;
		}

		var parameters = parent.GetGenericArguments();
		var isParameterLessGeneric = !((parameters.Length > 0)
			&& ((parameters[0].Attributes & TypeAttributes.BeforeFieldInit) == TypeAttributes.BeforeFieldInit));

		while ((child != null) && (child != typeof(object)))
		{
			if (child.BaseType == null)
			{
				return false;
			}

			var cur = GetFullTypeDefinition(child.BaseType);
			if ((parent == cur) || (isParameterLessGeneric && cur.GetInterfaces().Select(GetFullTypeDefinition).Contains(GetFullTypeDefinition(parent))))
			{
				return true;
			}
			if (!isParameterLessGeneric)
			{
				if ((GetFullTypeDefinition(parent) == cur) && !cur.IsInterface)
				{
					if (VerifyGenericArguments(GetFullTypeDefinition(parent), cur))
					{
						if (VerifyGenericArguments(parent, child))
						{
							return true;
						}
					}
				}
				else
				{
					if (child.GetInterfaces()
						.Where(i => GetFullTypeDefinition(parent) == GetFullTypeDefinition(i))
						.Any(item => VerifyGenericArguments(parent, item)))
					{
						return true;
					}
				}
			}

			if (directDescendantOnly)
			{
				return false;
			}

			child = child.BaseType;
		}

		return false;
	}

	private static bool VerifyGenericArguments(Type parent, Type child)
	{
		var childArguments = child.GetGenericArguments();
		var parentArguments = parent.GetGenericArguments();

		if (childArguments.Length != parentArguments.Length)
		{
			return true;
		}

		return !childArguments
			.Where((t, i) => ((t.Assembly != parentArguments[i].Assembly)
					|| (t.Name != parentArguments[i].Name)
					|| (t.Namespace != parentArguments[i].Namespace))
				&& !t.IsSubclassOf(parentArguments[i])
			).Any();
	}

	#endregion
}