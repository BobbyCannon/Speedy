#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Speedy.Storage;

#endregion

namespace Speedy
{
	/// <summary>
	/// Extensions for all the things.
	/// </summary>
	public static class Extensions
	{
		#region Fields

		private static readonly ConcurrentDictionary<string, FieldInfo[]> _fieldInfos;
		private static readonly ConcurrentDictionary<string, MethodInfo> _genericMethods;
		private static readonly ConcurrentDictionary<string, MethodInfo[]> _methodInfos;
		private static readonly ConcurrentDictionary<string, MethodInfo> _methods;
		private static readonly ConcurrentDictionary<string, ParameterInfo[]> _parameterInfos;
		private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyInfos;
		private static readonly JsonSerializerSettings _serializationSettings;
		private static readonly ConcurrentDictionary<Type, string> _typeAssemblyNames;
		private static readonly ConcurrentDictionary<string, Type[]> _types;
		private static readonly ConcurrentDictionary<string, PropertyInfo[]> _virtualPropertyInfos;

		#endregion

		#region Constructors

		static Extensions()
		{
			_fieldInfos = new ConcurrentDictionary<string, FieldInfo[]>();
			_genericMethods = new ConcurrentDictionary<string, MethodInfo>();
			_methodInfos = new ConcurrentDictionary<string, MethodInfo[]>();
			_methods = new ConcurrentDictionary<string, MethodInfo>();
			_propertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
			_parameterInfos = new ConcurrentDictionary<string, ParameterInfo[]>();
			_serializationSettings = GetSerializerSettings(false, true, true, false);
			_types = new ConcurrentDictionary<string, Type[]>();
			_typeAssemblyNames = new ConcurrentDictionary<Type, string>();
			_virtualPropertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add or update a dictionary entry.
		/// </summary>
		/// <typeparam name="T1"> The type of the key. </typeparam>
		/// <typeparam name="T2"> The type of the value. </typeparam>
		/// <param name="dictionary"> The dictionary to update. </param>
		/// <param name="key"> The value of the key. </param>
		/// <param name="value"> The value of the value. </param>
		public static void AddOrUpdate<T1, T2>(this IDictionary<T1, T2> dictionary, T1 key, T2 value)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = value;
				return;
			}

			dictionary.Add(key, value);
		}

		/// <summary>
		/// Add multiple items to a collection
		/// </summary>
		/// <param name="set"> The set to add items to. </param>
		/// <param name="items"> The items to add. </param>
		/// <typeparam name="T"> The type of the items in the collection. </typeparam>
		public static void AddRange<T>(this ICollection<T> set, IEnumerable<T> items)
		{
			foreach (var item in items)
			{
				set.Add(item);
			}
		}

		/// <summary>
		/// Creates a expression that represents a conditional AND operation that evaluates the second operand only if the first operand evaluates to true.
		/// </summary>
		/// <typeparam name="T"> The type used in the expression. </typeparam>
		/// <param name="left"> A Expression to set the Left property equal to. </param>
		/// <param name="right"> A Expression to set the Right property equal to. </param>
		/// <returns> The updated expression. </returns>
		public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
		{
			var parameter = Expression.Parameter(typeof(T));
			var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
			var vLeft = leftVisitor.Visit(left.Body);
			var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
			var vRight = rightVisitor.Visit(right.Body);
			return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(vLeft, vRight), parameter);
		}

		/// <summary>
		/// Appends new values to an existing HashSet.
		/// </summary>
		/// <typeparam name="T"> The type of value in the set. </typeparam>
		/// <param name="set"> The set to append to. </param>
		/// <param name="values"> The values to add. </param>
		/// <returns> A new HashSet containing the new values. </returns>
		public static HashSet<T> Append<T>(this HashSet<T> set, params T[] values)
		{
			return new HashSet<T>(set.Union(values));
		}

		/// <summary>
		/// Appends new values to an existing HashSet.
		/// </summary>
		/// <typeparam name="T"> The type of value in the set. </typeparam>
		/// <param name="set"> The set to append to. </param>
		/// <param name="values"> The values to add. </param>
		/// <returns> A new HashSet containing the new values. </returns>
		public static HashSet<T> Append<T>(this HashSet<T> set, HashSet<T> values)
		{
			return new HashSet<T>(set.Union(values));
		}

		/// <summary> Searches for the specified public method whose parameters match the specified argument types. The results are cached so the next query is much faster. </summary>
		/// <param name="type"> The type to get the method for. </param>
		/// <param name="name"> The string containing the name of the public method to get. </param>
		/// <param name="types"> An array of type objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of type objects (as provided by the EmptyTypes field) to get a method that takes no parameters. </param>
		/// <returns> An object representing the public method whose parameters match the specified argument types, if found; otherwise null. </returns>
		public static MethodInfo CachedGetMethod(this Type type, string name, params Type[] types)
		{
			MethodInfo response;
			var key = type.FullName + "." + name;

			if (_methods.ContainsKey(key))
			{
				if (_methods.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = types.Any() ? type.GetMethod(name, types) : type.GetMethod(name);
			return _methods.AddOrUpdate(key, response, (s, infos) => response);
		}

		/// <summary>
		/// Substitutes the elements of an array of types for the type parameters of the current generic method definition, and returns a
		/// MethodInfo object representing the resulting constructed method. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="info"> The property information to get the generic arguments for. </param>
		/// <param name="arguments"> An array of types to be substituted for the type parameters of the current generic method definition. </param>
		/// <returns> The method information with generics. </returns>
		public static MethodInfo CachedMakeGenericMethod(this MethodInfo info, Type[] arguments)
		{
			MethodInfo response;
			var fullName = info.ReflectedType?.FullName + "." + info.Name;
			var key = info.ToString().Replace(info.Name, fullName) + string.Join(", ", arguments.Select(x => x.FullName));

			if (_genericMethods.ContainsKey(key))
			{
				if (_genericMethods.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = info.MakeGenericMethod(arguments);
			return _genericMethods.AddOrUpdate(key, response, (s, i) => response);
		}

		/// <summary>
		/// Deep clone the item.
		/// </summary>
		/// <typeparam name="T"> The type to clone. </typeparam>
		/// <param name="item"> The item to clone. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore the virtual properties. </param>
		/// <returns> The clone of the item. </returns>
		public static T DeepClone<T>(this T item, bool ignoreVirtuals = false)
		{
			return FromJson<T>(item.ToJson(ignoreVirtuals: ignoreVirtuals));
		}

		/// <summary>
		/// Execute the action on each entity in the collection.
		/// </summary>
		/// <param name="items"> The collection of items to process. </param>
		/// <param name="action"> The action to execute for each item. </param>
		public static void ForEach(this IEnumerable items, Action<object> action)
		{
			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>
		/// Execute the action on each entity in the collection.
		/// </summary>
		/// <typeparam name="T"> The type of item in the collection. </typeparam>
		/// <param name="items"> The collection of items to process. </param>
		/// <param name="action"> The action to execute for each item. </param>
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (var item in items)
			{
				action(item);
			}
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <typeparam name="T"> The type to convert into. </typeparam>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <returns> The deserialized object. </returns>
		public static T FromJson<T>(this string item)
		{
			return JsonConvert.DeserializeObject<T>(item, _serializationSettings);
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <param name="type"> The type to convert into. </param>
		/// <returns> The deserialized object. </returns>
		public static object FromJson(this string item, Type type)
		{
			return FromJson(item, type, _serializationSettings);
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <param name="type"> The type to convert into. </param>
		/// <param name="settings"> The settings to be used. </param>
		/// <returns> The deserialized object. </returns>
		public static object FromJson(this string item, Type type, JsonSerializerSettings settings)
		{
			return string.IsNullOrWhiteSpace(item) ? null : JsonConvert.DeserializeObject(item, type, settings);
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

			MethodInfo[] response;

			if (_methodInfos.ContainsKey(key))
			{
				if (_methodInfos.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = info.GetAccessors();
			return _methodInfos.AddOrUpdate(key, response, (s, infos) => response);
		}

		/// <summary>
		/// Gets a list of fields for the provided item. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="item"> The item to get the fields for. </param>
		/// <param name="flags"> The flags used to query with. </param>
		/// <returns> The list of field infos for the item. </returns>
		public static IList<FieldInfo> GetCachedFields(this object item, BindingFlags? flags)
		{
			return item.GetType().GetCachedFields(flags);
		}

		/// <summary>
		/// Gets a list of fields for the provided type. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The type to get the fields for. </param>
		/// <param name="flags"> The flags used to query with. </param>
		/// <returns> The list of field infos for the type. </returns>
		public static IList<FieldInfo> GetCachedFields(this Type type, BindingFlags? flags)
		{
			if (type == null)
			{
				throw new InvalidOperationException();
			}

			var typeFlags = flags ?? BindingFlags.Instance | BindingFlags.GetField | BindingFlags.FlattenHierarchy;
			var key = GetCacheKey(type, typeFlags);

			FieldInfo[] response;

			if (_fieldInfos.ContainsKey(key))
			{
				if (_fieldInfos.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = type.GetFields(typeFlags);
			return _fieldInfos.AddOrUpdate(key, response, (s, infos) => response);
		}

		/// <summary>
		/// Gets a list of generic arguments for the provided method information. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="info"> The method information to get the generic arguments for. </param>
		/// <returns> The list of generic arguments for the method information of the value. </returns>
		public static IList<Type> GetCachedGenericArguments(this MethodInfo info)
		{
			Type[] response;
			var fullName = $"{info.ReflectedType?.FullName}.{info.Name}";
			var key = info.ToString().Replace(info.Name, fullName);

			if (_types.ContainsKey(key))
			{
				if (_types.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = info.GetGenericArguments();
			return _types.AddOrUpdate(key, response, (s, types) => response);
		}

		/// <summary>
		/// Gets a list of generic arguments for the provided type. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The type to get the generic arguments for. </param>
		/// <returns> The list of generic arguments for the type of the value. </returns>
		public static IList<Type> GetCachedGenericArguments(this Type type)
		{
			Type[] response;

			if (_types.ContainsKey(type.FullName ?? throw new InvalidOperationException()))
			{
				if (_types.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetGenericArguments();
			return _types.AddOrUpdate(type.FullName, response, (s, types) => response);
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
			if (type == null)
			{
				throw new InvalidOperationException();
			}

			var typeFlags = flags ?? BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			var key = GetCacheKey(type, typeFlags);

			MethodInfo[] response;

			if (_methodInfos.ContainsKey(key))
			{
				if (_methodInfos.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = type.GetMethods(typeFlags);
			return _methodInfos.AddOrUpdate(key, response, (s, infos) => response);
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

			ParameterInfo[] response;

			if (_parameterInfos.ContainsKey(key))
			{
				if (_parameterInfos.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = info.GetParameters();
			return _parameterInfos.AddOrUpdate(key, response, (s, infos) => response);
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
		/// Gets a list of property types for the provided type. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The type to get the properties for. </param>
		/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
		/// <returns> The list of properties for the type. </returns>
		public static IList<PropertyInfo> GetCachedProperties(this Type type, BindingFlags? flags = null)
		{
			if (type == null)
			{
				throw new InvalidOperationException();
			}

			var typeFlags = flags ?? BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			var key = GetCacheKey(type, typeFlags);

			PropertyInfo[] response;

			if (_propertyInfos.ContainsKey(key))
			{
				if (_propertyInfos.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = type.GetProperties(typeFlags);
			return _propertyInfos.AddOrUpdate(key, response, (s, infos) => response);
		}

		/// <summary>
		/// Gets a list of virtual property types for the provided type. The results are cached so the next query is much faster.
		/// </summary>
		/// <param name="type"> The type to get the properties for. </param>
		/// <param name="flags"> The flags to find properties by. Defaults to Public, Instance, Flatten Hierarchy </param>
		/// <returns> The list of properties for the type. </returns>
		public static IList<PropertyInfo> GetCachedVirtualProperties(this Type type, BindingFlags? flags = null)
		{
			if (type == null)
			{
				throw new InvalidOperationException();
			}

			var typeFlags = flags ?? BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
			var key = GetCacheKey(type, typeFlags);

			PropertyInfo[] response;

			if (_virtualPropertyInfos.ContainsKey(key))
			{
				if (_virtualPropertyInfos.TryGetValue(key, out response))
				{
					return response;
				}
			}

			response = type.GetCachedProperties(typeFlags)
				.Where(x => x.GetMethod.IsVirtual && !x.GetMethod.IsAbstract && !x.GetMethod.IsFinal && x.GetMethod.Attributes.HasFlag(MethodAttributes.VtableLayoutMask))
				.OrderBy(x => x.Name)
				.ToArray();

			return _virtualPropertyInfos.AddOrUpdate(key, response, (s, infos) => response);
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
		/// <param name="obj"> The source target. </param>
		/// <param name="memberName"> Name of the field or property. </param>
		/// <returns> the value of member </returns>
		public static object GetMemberValue(this object obj, string memberName)
		{
			var memInf = GetMemberInfo(obj, memberName);

			if (memInf == null)
			{
				throw new Exception("memberName");
			}

			switch (memInf)
			{
				case PropertyInfo propertyInfo:
					return propertyInfo.GetValue(obj, null);

				case FieldInfo fieldInfo:
					return fieldInfo.GetValue(obj);
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
		/// Get the serialization settings.
		/// </summary>
		/// <param name="camelCase"> True to camelCase or else use PascalCase. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> True to ignore members that are read only. </param>
		/// <param name="convertEnumsToString"> True to convert enumerations to strings value instead. </param>
		/// <returns> The serialization settings. </returns>
		public static JsonSerializerSettings GetSerializerSettings(bool camelCase, bool ignoreNullValues, bool ignoreReadOnly, bool convertEnumsToString)
		{
			var response = new JsonSerializerSettings();

			if (convertEnumsToString)
			{
				var namingStrategy = camelCase ? (NamingStrategy) new CamelCaseNamingStrategy() : new DefaultNamingStrategy();
				response.Converters.Add(new StringEnumConverter { NamingStrategy = namingStrategy });
			}

			response.Converters.Add(new IsoDateTimeConverter());
			response.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			response.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			response.NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
			response.ContractResolver = new SerializeContractResolver(camelCase, ignoreReadOnly);

			return response;
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
		/// Determines if the string is a JSON string.
		/// </summary>
		/// <param name="input"> The value to validate. </param>
		/// <returns> True if the input is JSON or false if otherwise. </returns>
		public static bool IsJson(this string input)
		{
			input = input.Trim();

			var isWellFormed = new Func<bool>(() =>
			{
				try
				{
					JToken.Parse(input);
				}
				catch
				{
					return false;
				}

				return true;
			});

			return (input.StartsWith("{") && input.EndsWith("}") || input.StartsWith("[") && input.EndsWith("]")) && isWellFormed();
		}

		/// <summary>
		/// Natural sort a string collection.
		/// </summary>
		/// <param name="collection"> The collection to sort. </param>
		/// <returns> The sorted collection. </returns>
		public static IEnumerable<string> NaturalSort(this IEnumerable<string> collection)
		{
			return NaturalSort(collection, CultureInfo.CurrentCulture);
		}

		/// <summary>
		/// Natural sort a string collection.
		/// </summary>
		/// <param name="collection"> The collection to sort. </param>
		/// <param name="cultureInfo"> The culture information to use during sort. </param>
		/// <returns> The sorted collection. </returns>
		public static IEnumerable<string> NaturalSort(this IEnumerable<string> collection, CultureInfo cultureInfo)
		{
			return collection.OrderBy(s => s, new SyncKeyComparer(cultureInfo));
		}

		/// <summary>
		/// Creates a expression that represents a conditional OR operation.
		/// </summary>
		/// <typeparam name="T"> The type used in the expression. </typeparam>
		/// <param name="left"> A Expression to set the Left property equal to. </param>
		/// <param name="right"> A Expression to set the Right property equal to. </param>
		/// <returns> The updated expression. </returns>
		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
		{
			var parameter = Expression.Parameter(typeof(T));
			var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
			var vLeft = leftVisitor.Visit(left.Body);
			var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
			var vRight = rightVisitor.Visit(right.Body);
			return Expression.Lambda<Func<T, bool>>(Expression.Or(vLeft, vRight), parameter);
		}

		/// <summary>
		/// Continues to run the action until we hit the timeout. If an exception occurs then delay for the
		/// provided delay time.
		/// </summary>
		/// <typeparam name="T"> The type for this retry. </typeparam>
		/// <param name="action"> The action to attempt to retry. </param>
		/// <param name="timeout"> The timeout to stop retrying. </param>
		/// <param name="delay"> The delay between retries. </param>
		/// <returns> The response from the action. </returns>
		public static T Retry<T>(Func<T> action, int timeout, int delay)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				return action();
			}
			catch (Exception)
			{
				Thread.Sleep(delay);

				var remaining = (int) (timeout - watch.Elapsed.TotalMilliseconds);
				if (remaining <= 0)
				{
					throw;
				}

				return Retry(action, remaining, delay);
			}
		}

		/// <summary>
		/// Continues to run the action until we hit the timeout. If an exception occurs then delay for the
		/// provided delay time.
		/// </summary>
		/// <param name="action"> The action to attempt to retry. </param>
		/// <param name="timeout"> The timeout to stop retrying. </param>
		/// <param name="delay"> The delay between retries. </param>
		/// <returns> The response from the action. </returns>
		public static void Retry(Action action, int timeout, int delay)
		{
			var watch = Stopwatch.StartNew();

			try
			{
				action();
			}
			catch (Exception)
			{
				Thread.Sleep(delay);

				var remaining = (int) (timeout - watch.Elapsed.TotalMilliseconds);
				if (remaining <= 0)
				{
					throw;
				}

				Retry(action, remaining, delay);
			}
		}

		/// <summary>
		/// Safely create a directory.
		/// </summary>
		/// <param name="info"> The information on the directory to create. </param>
		public static void SafeCreate(this DirectoryInfo info)
		{
			Retry(() =>
			{
				info.Refresh();

				if (!info.Exists)
				{
					info.Create();
				}
			}, 1000, 10);

			Wait(() =>
			{
				info.Refresh();
				return info.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely create a file.
		/// </summary>
		/// <param name="file"> The information of the file to create. </param>
		public static void SafeCreate(this FileInfo file)
		{
			file.Refresh();
			if (file.Exists)
			{
				return;
			}

			Retry(() =>
			{
				if (file.Exists)
				{
					return;
				}

				File.Create(file.FullName).Dispose();
			}, 1000, 10);

			Wait(() =>
			{
				file.Refresh();
				return file.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely delete a file.
		/// </summary>
		/// <param name="info"> The information of the file to delete. </param>
		public static void SafeDelete(this FileInfo info)
		{
			Retry(() =>
			{
				info.Refresh();

				if (info.Exists)
				{
					info.Delete();
				}
			}, 1000, 10);

			Wait(() =>
			{
				info.Refresh();
				return !info.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely delete a directory.
		/// </summary>
		/// <param name="info"> The information of the directory to delete. </param>
		public static void SafeDelete(this DirectoryInfo info)
		{
			Retry(() =>
			{
				info.Refresh();

				if (info.Exists)
				{
					info.Delete(true);
				}
			}, 1000, 10);

			Wait(() =>
			{
				info.Refresh();
				return !info.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely move a file.
		/// </summary>
		/// <param name="fileLocation"> The information of the file to move. </param>
		/// <param name="newLocation"> The location to move the file to. </param>
		public static void SafeMove(this FileInfo fileLocation, FileInfo newLocation)
		{
			fileLocation.Refresh();
			if (!fileLocation.Exists)
			{
				throw new FileNotFoundException("The file could not be found.", fileLocation.FullName);
			}

			Retry(() => fileLocation.MoveTo(newLocation.FullName), 1000, 10);

			Wait(() =>
			{
				fileLocation.Refresh();
				newLocation.Refresh();
				return !fileLocation.Exists && newLocation.Exists;
			}, 1000, 10);
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
			var memInf = GetMemberInfo(obj, memberName);

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
		/// Specifies additional related data to be further included based on a related type that was just included.
		/// </summary>
		/// <typeparam name="T"> The type of entity being queried. </typeparam>
		/// <typeparam name="TPreviousProperty"> The type of the entity that was just included. </typeparam>
		/// <typeparam name="TProperty"> The type of the related entity to be included. </typeparam>
		/// <param name="source"> The source query. </param>
		/// <param name="include"> A lambda expression representing the navigation property to be included (<c> t =&gt; t.Property1 </c>). </param>
		/// <returns> A new query with the related data included. </returns>
		public static IIncludableQueryable<T, TProperty> ThenInclude<T, TPreviousProperty, TProperty>(this IIncludableQueryable<T, ICollection<TPreviousProperty>> source, Expression<Func<TPreviousProperty, TProperty>> include) where T : class
		{
			return source.ThenInclude(include);
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
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> True to ignore members that are read only. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. Default value is false. </param>
		/// <param name="convertEnumsToString"> True to convert enumerations to strings value instead. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, bool camelCase = false, bool indented = false, bool ignoreNullValues = false, bool ignoreReadOnly = false, bool ignoreVirtuals = false, bool convertEnumsToString = false)
		{
			var settings = ToJsonSettings(item, camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString);
			return JsonConvert.SerializeObject(item, indented ? Formatting.Indented : Formatting.None, settings);
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="settings"> The settings to be used. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, JsonSerializerSettings settings, bool indented = false)
		{
			return JsonConvert.SerializeObject(item, indented ? Formatting.Indented : Formatting.None, settings);
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <param name="type"> The type of the object to serialize. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> True to ignore members that are read only. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. Default value is false. </param>
		/// <param name="convertEnumsToString"> True to convert enumerations to strings value instead. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static JsonSerializerSettings ToJsonSettings(this Type type, bool camelCase = false, bool ignoreNullValues = false, bool ignoreReadOnly = false, bool ignoreVirtuals = false, bool convertEnumsToString = false)
		{
			var settings = GetSerializerSettings(camelCase, ignoreNullValues, ignoreReadOnly, convertEnumsToString);
			var resolver = (SerializeContractResolver) settings.ContractResolver;

			if (ignoreVirtuals)
			{
				var realType = type.GetRealType();
				var values = realType.GetVirtualPropertyNames().ToArray();

				if (values.Length > 0)
				{
					resolver.ResetIgnores(new KeyValuePair<string, string[]>(realType.FullName, values));
				}
			}

			return settings;
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <param name="ignoreReadOnly"> True to ignore members that are read only. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. Default value is false. </param>
		/// <param name="convertEnumsToString"> True to convert enumerations to strings value instead. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static JsonSerializerSettings ToJsonSettings<T>(this T item, bool camelCase = false, bool ignoreNullValues = false, bool ignoreReadOnly = false, bool ignoreVirtuals = false, bool convertEnumsToString = false)
		{
			return ToJsonSettings(item.GetType(), camelCase, ignoreNullValues, ignoreReadOnly, ignoreVirtuals, convertEnumsToString);
		}

		/// <summary>
		/// Convert the event written event argument to its payload string
		/// </summary>
		/// <param name="args"> The item to process. </param>
		/// <returns> The formatted message. </returns>
		public static string ToPayloadString(this EventWrittenEventArgs args)
		{
			return string.Format(args.Message, args.Payload.ToArray());
		}

		/// <summary>
		/// Unwraps a sync entity and disconnects it from the Entity Framework context. Check the value to see if the
		/// IUnwrappable interface is implemented. If so the value's implementation is used instead.
		/// The default behavior is to ignore read only and virtual properties.
		/// </summary>
		/// <typeparam name="T"> The type of the incoming object. </typeparam>
		/// <typeparam name="T2"> The type of the outgoing object. </typeparam>
		/// <param name="value"> The value to unwrap from the proxy. </param>
		/// <param name="update"> An optional update method. </param>
		/// <returns> The disconnected entity. </returns>
		public static T2 Unwrap<T, T2>(this T value, Action<T2> update = null)
		{
			// notice: do not use Unwrappable until it can support to specific types

			var response = value.ToJson(ignoreReadOnly: true, ignoreVirtuals: true).FromJson<T2>();
			update?.Invoke(response);
			return response;
		}

		/// <summary>
		/// Unwraps a sync entity and disconnects it from the Entity Framework context.
		/// </summary>
		/// <param name="value"> The value to unwrap from the proxy. </param>
		/// <param name="type"> The type of the outgoing object. </param>
		/// <returns> The disconnected entity. </returns>
		public static object Unwrap(this object value, Type type)
		{
			if (value is IUnwrappable unwrappable)
			{
				return unwrappable.Unwrap();
			}

			return value.ToJson(ignoreReadOnly: true, ignoreVirtuals: true).FromJson(type);
		}

		/// <summary>
		/// Runs action if the test is true.
		/// </summary>
		/// <param name="item"> The item to process. (does nothing) </param>
		/// <param name="test"> The test to validate. </param>
		/// <param name="action"> The action to run if test is true. </param>
		/// <typeparam name="T"> The type the function returns </typeparam>
		/// <returns> The result of the action or default(T). </returns>
		public static T UpdateIf<T>(this object item, Func<bool> test, Func<T> action)
		{
			return test() ? action() : default;
		}

		/// <summary>
		/// Allows updating of one type to another based on member Name and Type.
		/// </summary>
		/// <typeparam name="T"> The type to be updated. </typeparam>
		/// <typeparam name="T2"> The source type of the provided update. </typeparam>
		/// <param name="value"> The value to be updated. </param>
		/// <param name="update"> The source of the updates. </param>
		/// <param name="excludeVirtuals"> An optional value to exclude virtual members. Defaults to true. </param>
		/// <param name="exclusions"> An optional list of members to exclude. </param>
		public static void UpdateWith<T, T2>(this T value, T2 update, bool excludeVirtuals = true, params string[] exclusions)
		{
			var destinationType = value.GetRealType();
			var sourceType = update.GetRealType();
			var destinationProperties = destinationType.GetCachedProperties();
			var sourceProperties = sourceType.GetCachedProperties();
			var virtualProperties = destinationType.GetVirtualPropertyNames().ToArray();

			foreach (var thisProperty in destinationProperties)
			{
				// Ensure the destination can write this property
				var canWrite = thisProperty.CanWrite && thisProperty.SetMethod.IsPublic;
				if (!canWrite)
				{
					continue;
				}

				var isPropertyExcluded = exclusions.Contains(thisProperty.Name);
				if (isPropertyExcluded)
				{
					continue;
				}

				if (excludeVirtuals && virtualProperties.Contains(thisProperty.Name))
				{
					continue;
				}

				// Check to see if the update source entity has the property
				var updateProperty = sourceProperties.FirstOrDefault(x => x.Name == thisProperty.Name && x.PropertyType == thisProperty.PropertyType);
				if (updateProperty == null)
				{
					// Skip this because target type does not have correct property.
					continue;
				}

				var updateValue = updateProperty.GetValue(update);
				var thisValue = thisProperty.GetValue(value);

				if (!Equals(updateValue, thisValue))
				{
					thisProperty.SetValue(value, updateValue);
				}
			}
		}

		/// <summary>
		/// Runs the action until the action returns true or the timeout is reached. Will delay in between actions of the provided
		/// time.
		/// </summary>
		/// <param name="action"> The action to call. </param>
		/// <param name="timeout"> The timeout to attempt the action. This value is in milliseconds. </param>
		/// <param name="delay"> The delay in between actions. This value is in milliseconds. </param>
		/// <returns> Returns true of the call completed successfully or false if it timed out. </returns>
		public static bool Wait(Func<bool> action, int timeout, int delay)
		{
			var watch = Stopwatch.StartNew();
			var watchTimeout = TimeSpan.FromMilliseconds(timeout);
			var result = false;

			while (!result)
			{
				if (watch.Elapsed > watchTimeout)
				{
					return false;
				}

				result = action();
				if (!result)
				{
					Thread.Sleep(delay);
				}
			}

			return true;
		}

		internal static FileStream CopyToAndOpen(this FileStream from, FileInfo to, int timeout)
		{
			lock (from)
			{
				var response = Retry(() => File.Open(to.FullName, FileMode.Create, FileAccess.ReadWrite, FileShare.None), timeout, 50);
				from.Position = 0;
				from.CopyTo(response);
				response.Flush(true);
				response.Position = 0;
				return response;
			}
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

		/// <summary>
		/// Open the file with read/write permission with file read share.
		/// </summary>
		/// <param name="info"> The information for the file. </param>
		/// <returns> The stream for the file. </returns>
		internal static FileStream OpenFile(this FileInfo info)
		{
			return Retry(() => File.Open(info.FullName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read), 1000, 50);
		}

		/// <summary>
		/// Gets a detailed string of the exception. Includes messages of all exceptions.
		/// </summary>
		/// <param name="ex"> The exception to process. </param>
		/// <returns> The detailed string of the exception. </returns>
		internal static string ToDetailedString(this Exception ex)
		{
			var builder = new StringBuilder();
			AddExceptionToBuilder(builder, ex);
			return builder.ToString();
		}

		internal static Task Wrap(Action action)
		{
			return Task.Factory.StartNew(action);
		}

		private static void AddExceptionToBuilder(StringBuilder builder, Exception ex)
		{
			builder.Append(builder.Length > 0 ? "\r\n" + ex.Message : ex.Message);

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException);
			}
		}

		private static string GetCacheKey(Type type, BindingFlags flags)
		{
			return type.FullName + flags;
		}

		private static MemberInfo GetMemberInfo(object obj, string memberName)
		{
			var type = obj.GetType();
			var flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
			return (MemberInfo) type.GetProperty(memberName, flags) ?? type.GetField(memberName, flags);
		}

		#endregion

		#region Classes

		private class ReplaceExpressionVisitor : ExpressionVisitor
		{
			#region Fields

			private readonly Expression _newValue;
			private readonly Expression _oldValue;

			#endregion

			#region Constructors

			public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
			{
				_oldValue = oldValue;
				_newValue = newValue;
			}

			#endregion

			#region Methods

			public override Expression Visit(Expression node)
			{
				return node == _oldValue ? _newValue : base.Visit(node);
			}

			#endregion
		}

		#endregion
	}
}