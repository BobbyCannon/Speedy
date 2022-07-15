#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;

#endregion

namespace Speedy.Extensions
{
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

		#endregion

		#region Fields

		private static readonly ConcurrentDictionary<string, MethodInfo> _genericMethods;
		private static readonly ConcurrentDictionary<string, ParameterInfo[]> _methodParameters;
		private static readonly ConcurrentDictionary<string, Type[]> _methodsGenericArgumentInfos;
		private static readonly ConcurrentDictionary<string, MethodInfo[]> _propertyGetAccessors;
		private static readonly ConcurrentDictionary<Type, string> _typeAssemblyNames;
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
			_genericMethods = new ConcurrentDictionary<string, MethodInfo>();
			_methodParameters = new ConcurrentDictionary<string, ParameterInfo[]>();
			_methodsGenericArgumentInfos = new ConcurrentDictionary<string, Type[]>();
			_propertyGetAccessors = new ConcurrentDictionary<string, MethodInfo[]>();
			_typeAssemblyNames = new ConcurrentDictionary<Type, string>();
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
		/// Substitutes the elements of an array of types for the type parameters of the current generic method definition, and returns a
		/// MethodInfo object representing the resulting constructed method. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="info"> The property information to get the generic arguments for. </param>
		/// <param name="arguments"> An array of types to be substituted for the type parameters of the current generic method definition. </param>
		/// <returns> The method information with generics. </returns>
		public static MethodInfo CachedMakeGenericMethod(this MethodInfo info, Type[] arguments)
		{
			var fullName = info.ReflectedType?.FullName + "." + info.Name;
			var key = info.ToString().Replace(info.Name, fullName) + string.Join(", ", arguments.Select(x => x.FullName));
			return _genericMethods.GetOrAdd(key, _ => info.MakeGenericMethod(arguments));
		}

		/// <summary>
		/// Create an instance for a given Type.
		/// </summary>
		/// <param name="type"> The Type for which to get an instance of. </param>
		/// <param name="arguments"> The value of the arguments. </param>
		/// <returns> The new instances of the type. </returns>
		public static object CreateInstance(this Type type, params object[] arguments)
		{
			// If no type was supplied, return null.
			if (type == null)
			{
				return null;
			}

			var isGeneric = type.IsGenericType && type.GenericTypeArguments.Any();
			if (isGeneric)
			{
				return Activator.CreateInstance(type, arguments);
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
					if (type == typeof(string))
					{
						return string.Empty;
					}

					return arguments.Any()
						? Activator.CreateInstance(type, arguments)
						: Activator.CreateInstance(type);
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
		/// Quickly create a new type of a generic.
		/// </summary>
		/// <param name="type"> The base type that requires generics. </param>
		/// <param name="genericTypes"> The types for the generic. </param>
		/// <param name="arguments"> The value of the arguments. </param>
		/// <returns> The new instances of the type. </returns>
		public static object CreateInstance(this Type type, Type[] genericTypes, params object[] arguments)
		{
			if (!type.GenericTypeArguments.Any())
			{
				var genericType = type.MakeGenericType(genericTypes);
				return CreateInstance(genericType, arguments);
			}

			return Activator.CreateInstance(type, arguments);
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
			var key = info.ToString().Replace(info.Name, fullName);
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
			return _typeMethodInfos.GetOrAdd(methodKey, GetCachedMethods(typeKey, flags).FirstOrDefault(x => x.Name == name));
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
			var key = info.ToString().Replace(info.Name, fullName);

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
			return _typePropertyInfos.GetOrAdd(key, _ => type.GetProperties(typeFlags));
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
		/// Retrieves the default value for a given Type.
		/// </summary>
		/// <param name="type"> The Type for which to get the default value </param>
		/// <returns> The default value for <paramref name="type" /> </returns>
		/// <remarks>
		/// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type
		/// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an
		/// exception.
		/// </remarks>
		public static object GetDefaultValue(this Type type)
		{
			// If no type was supplied, if the type is nullable, or if the type was a Void / String, return null
			if ((type == null) || type.IsNullable() || (type == typeof(void)) || (type == typeof(string)))
			{
				return null;
			}

			var isCollection = type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ICollection<>));
			if (isCollection)
			{
				var collectionType = typeof(Collection<>);
				var constructedListType = collectionType.MakeGenericType(type.GenericTypeArguments);
				return Activator.CreateInstance(constructedListType);
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
					return Activator.CreateInstance(type);
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

				var arg = type.GenericTypeArguments?.ToArray();
				if ((arg == null) || (arg.Length <= 0))
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
		public static Type GetRealType(this object item)
		{
			var type = item.GetType();
			var isProxy = (type.FullName?.Contains("System.Data.Entity.DynamicProxies") == true)
				|| (type.FullName?.Contains("Castle.Proxies") == true);
			return isProxy ? type.BaseType : type;
		}

		/// <summary>
		/// Gets the real type of the entity. For use with proxy entities.
		/// </summary>
		/// <param name="type"> The type to process. </param>
		/// <returns> The real base type for the proxy or just the initial type if it is not a proxy. </returns>
		public static Type GetRealType(this Type type)
		{
			var isProxy = (type.FullName?.Contains("System.Data.Entity.DynamicProxies") == true)
				|| (type.FullName?.Contains("Castle.Proxies") == true);

			return isProxy ? type.BaseType : type;
		}

		/// <summary>
		/// Gets a list of virtual property names. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The value to get the property names for. </param>
		/// <returns> The list of virtual property names for the type. </returns>
		public static IEnumerable<string> GetVirtualPropertyNames(this Type type)
		{
			return GetCachedVirtualProperties(type).Select(x => x.Name).ToArray();
		}

		/// <summary>
		/// Determine if the property is a virtual method.
		/// </summary>
		/// <param name="info"> The info to process. </param>
		/// <returns> True if the accessor is virtual. </returns>
		public static bool IsVirtual(this PropertyInfo info)
		{
			return (info.CanRead
					&& info.GetMethod.IsVirtual
					&& !info.GetMethod.IsAbstract
					&& !info.GetMethod.IsFinal
					&& info.GetMethod.Attributes.HasFlag(MethodAttributes.VtableLayoutMask))
				|| (info.CanWrite
					&& info.SetMethod.IsVirtual
					&& !info.SetMethod.IsAbstract
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
		/// <param name="value"> The value to update all properties for. </param>
		/// <param name="nonSupportedType"> An optional function to update non supported property value types. </param>
		/// <returns> </returns>
		public static T UpdateWithNonDefaultValues<T>(this T value, Func<PropertyInfo, object> nonSupportedType = null)
		{
			var random = RandomNumberGenerator.Create();
			var buffer = new byte[16];
			var properties = value
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToList();

			foreach (var property in properties)
			{
				var type = property.PropertyType;

				if (type.IsEnum)
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToInt32(buffer, 0));
				}
				else if (type == typeof(bool))
				{
					property.SetValue(value, true);
				}
				else if (type == typeof(DateTime))
				{
					property.SetValue(value, DateTime.UtcNow);
				}
				else if (type == typeof(byte))
				{
					random.GetBytes(buffer, 0, 1);
					property.SetValue(value, buffer[0]);
				}
				else if (type == typeof(short))
				{
					random.GetBytes(buffer, 0, 2);
					property.SetValue(value, BitConverter.ToInt16(buffer, 0));
				}
				else if (type == typeof(ushort))
				{
					random.GetBytes(buffer, 0, 2);
					property.SetValue(value, BitConverter.ToUInt16(buffer, 0));
				}
				else if (type == typeof(decimal))
				{
					var r = new Random();
					var dValue = NextDecimal(r);
					property.SetValue(value, dValue);
				}
				else if (type == typeof(double))
				{
					random.GetBytes(buffer, 0, 8);
					property.SetValue(value, BitConverter.ToDouble(buffer, 0));
				}
				else if (type == typeof(float))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToSingle(buffer, 0));
				}
				else if ((type == typeof(Guid)) || (type == typeof(Guid?)))
				{
					property.SetValue(value, Guid.NewGuid());
				}
				else if ((type == typeof(int)) || (type == typeof(int?)))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToInt32(buffer, 0));
				}
				else if ((type == typeof(uint)) || (type == typeof(uint?)))
				{
					random.GetBytes(buffer, 0, 4);
					property.SetValue(value, BitConverter.ToUInt32(buffer, 0));
				}
				else if ((type == typeof(long)) || (type == typeof(long?)))
				{
					random.GetBytes(buffer, 0, 8);
					property.SetValue(value, BitConverter.ToInt64(buffer, 0));
				}
				else if (type == typeof(string))
				{
					property.SetValue(value, Guid.NewGuid().ToString());
				}
				else if (type == typeof(TimeSpan))
				{
					property.SetValue(value, TimeSpan.FromSeconds(1));
				}
				else
				{
					if (nonSupportedType != null)
					{
						property.SetValue(value, nonSupportedType.Invoke(property));
					}
				}
			}

			return value;
		}

		/// <summary>
		/// Validates that all values are not default value.
		/// </summary>
		/// <typeparam name="T"> The type of the model. </typeparam>
		/// <param name="model"> The model to be validated. </param>
		/// <param name="exclusions"> An optional set of exclusions. </param>
		public static void ValidateAllValuesAreNotDefault<T>(this T model, params string[] exclusions)
		{
			var properties = model
				.GetCachedProperties()
				.Where(x => x.CanWrite)
				.ToList();

			foreach (var property in properties)
			{
				if ((exclusions.Length > 0) && exclusions.Contains(property.Name))
				{
					continue;
				}

				var value = property.GetValue(model);
				var defaultValue = property.PropertyType.GetDefaultValue();

				if (Equals(value, defaultValue))
				{
					throw new Exception($"Property {property.Name} should have been set but was not.");
				}
			}
		}

		private static MemberInfo GetCachedMember(object obj, string memberName)
		{
			var type = obj?.GetType() ?? throw new ArgumentNullException(nameof(obj));
			var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
			return (MemberInfo) type.GetCachedProperty(memberName, flags) ?? type.GetCachedField(memberName, flags);
		}

		private static string GetCacheKey(Type type, BindingFlags flags)
		{
			return type.FullName + flags;
		}

		private static decimal NextDecimal(this Random rng)
		{
			var scale = (byte) rng.Next(29);
			var sign = rng.Next(2) == 1;
			return new decimal(
				rng.NextInt32(),
				rng.NextInt32(),
				rng.NextInt32(),
				sign,
				scale);
		}

		/// <summary>
		/// Returns an Int32 with a random value across the entire range of
		/// possible values.
		/// </summary>
		private static int NextInt32(this Random rng)
		{
			var firstBits = rng.Next(0, 1 << 4) << 28;
			var lastBits = rng.Next(0, 1 << 28);
			return firstBits | lastBits;
		}

		#endregion
	}
}