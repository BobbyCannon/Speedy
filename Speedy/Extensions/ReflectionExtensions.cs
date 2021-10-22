#region References

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
			return _genericMethods.GetOrAdd(key, x => info.MakeGenericMethod(arguments));
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
			return _propertyGetAccessors.GetOrAdd(key, x => info.GetAccessors());
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
			return _typeFieldInfos.GetOrAdd(key, x => type.GetFields(typeFlags));
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
			return _methodsGenericArgumentInfos.GetOrAdd(key, x => info.GetGenericArguments());
		}

		/// <summary>
		/// Gets a list of generic arguments for the provided type. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The type to get the generic arguments for. </param>
		/// <returns> The list of generic arguments for the type of the value. </returns>
		public static IList<Type> GetCachedGenericArguments(this Type type)
		{
			return _methodsGenericArgumentInfos.GetOrAdd(type.FullName ?? throw new InvalidOperationException(), x => type.GetGenericArguments());
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
			return _typeMethodInfos.GetOrAdd(methodKey, x => types.Any() ? type.GetMethod(name, types) : type.GetMethod(name));
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

			return _methodParameters.GetOrAdd(key, x => info.GetParameters());
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
			return _typePropertyInfos.GetOrAdd(key, x => type.GetProperties(typeFlags));
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
		/// Gets a list of virtual property types for the provided type. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The type to get the properties for. </param>
		/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
		/// <returns> The list of properties for the type. </returns>
		public static IList<PropertyInfo> GetCachedVirtualProperties(this Type type, BindingFlags? flags = null)
		{
			var typeFlags = flags ?? DefaultFlags;
			var key = GetCacheKey(type ?? throw new InvalidOperationException(), typeFlags);

			return _typeVirtualPropertyInfos.GetOrAdd(key, x =>
			{
				return type.GetCachedProperties(typeFlags)
					.Where(p => p.GetMethod.IsVirtual && !p.GetMethod.IsAbstract && !p.GetMethod.IsFinal && p.GetMethod.Attributes.HasFlag(MethodAttributes.VtableLayoutMask))
					.OrderBy(p => p.Name)
					.ToArray();
			});
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

			return (expression as dynamic).Body.Member.Name;
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
				if (arg == null || arg.Length <= 0)
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

			switch (memberInfo)
			{
				case PropertyInfo propertyInfo:
					return propertyInfo.GetValue(value, null);

				case FieldInfo fieldInfo:
					return fieldInfo.GetValue(value);
			}

			throw new Exception();
		}

		/// <summary>
		/// Gets the real type of the entity. For use with proxy entities.
		/// </summary>
		/// <param name="item"> The object to process. </param>
		/// <returns> The real base type for the proxy or just the initial type if it is not a proxy. </returns>
		public static Type GetRealType(this object item)
		{
			var type = item.GetType();
			var isProxy = type.FullName?.Contains("System.Data.Entity.DynamicProxies") == true || type.FullName?.Contains("Castle.Proxies") == true;
			return isProxy ? type.BaseType : type;
		}

		/// <summary>
		/// Gets the real type of the entity. For use with proxy entities.
		/// </summary>
		/// <param name="type"> The type to process. </param>
		/// <returns> The real base type for the proxy or just the initial type if it is not a proxy. </returns>
		public static Type GetRealType(this Type type)
		{
			var isProxy = type.FullName?.Contains("System.Data.Entity.DynamicProxies") == true || type.FullName?.Contains("Castle.Proxies") == true;
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
					propertyInfo.SetValue(obj, newValue, null);
					break;

				case FieldInfo fieldInfo:
					fieldInfo.SetValue(obj, newValue);
					break;

				default:
					throw new Exception();
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
		/// Retrieves the default value for a given Type
		/// </summary>
		/// <param name="type"> The Type for which to get the default value </param>
		/// <returns> The default value for <paramref name="type" /> </returns>
		/// <remarks>
		/// If a null Type, a reference Type, or a System.Void Type is supplied, this method always returns null.  If a value type
		/// is supplied which is not publicly visible or which contains generic parameters, this method will fail with an
		/// exception.
		/// </remarks>
		internal static object GetDefault(this Type type)
		{
			// If no Type was supplied, if the Type was a reference type, or if the Type was a System.Void, return null
			if (type == null || !type.IsValueType || type == typeof(void))
			{
				return null;
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

		#endregion
	}
}