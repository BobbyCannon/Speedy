#region References

using System;
using System.Collections;
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

	/// <summary>
	/// Determines if the child is a subclass of the parent.
	/// </summary>
	/// <param name="child"> The type to be tested. </param>
	/// <param name="parent"> The type of the parent. </param>
	/// <returns> True if the child implements the parent otherwise false. </returns>
	public static bool IsSubClassOfGeneric(this Type child, Type parent)
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
			var cur = GetFullTypeDefinition(child);
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

			child = child.BaseType;
		}

		return false;
	}

	private static Type GetFullTypeDefinition(Type type)
	{
		return type.IsGenericType ? type.GetGenericTypeDefinition() : type;
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