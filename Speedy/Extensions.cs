#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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

		private static readonly ConcurrentDictionary<string, MethodInfo> _genericMethods;
		private static readonly ConcurrentDictionary<string, MethodInfo[]> _methodInfos;
		private static readonly ConcurrentDictionary<string, MethodInfo> _methods;
		private static readonly ConcurrentDictionary<string, ParameterInfo[]> _parameterInfos;
		private static readonly ConcurrentDictionary<string, PropertyInfo[]> _propertyInfos;
		private static readonly JsonSerializerSettings _serializationSettingsNoVirtuals;
		private static readonly ConcurrentDictionary<string, Type[]> _types;

		#endregion

		#region Constructors

		static Extensions()
		{
			_types = new ConcurrentDictionary<string, Type[]>();
			_methodInfos = new ConcurrentDictionary<string, MethodInfo[]>();
			_genericMethods = new ConcurrentDictionary<string, MethodInfo>();
			_methods = new ConcurrentDictionary<string, MethodInfo>();
			_propertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
			_parameterInfos = new ConcurrentDictionary<string, ParameterInfo[]>();
			_serializationSettingsNoVirtuals = GetSerializerSettings(false, true, false);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add multiple items to a hash set.
		/// </summary>
		/// <param name="set"> The set to add items to. </param>
		/// <param name="items"> The items to add. </param>
		/// <typeparam name="T"> The type of the items in the hash set. </typeparam>
		public static void AddRange<T>(this HashSet<T> set, params T[] items)
		{
			foreach (var item in items)
			{
				set.Add(item);
			}
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
			return JsonConvert.DeserializeObject<T>(item, _serializationSettingsNoVirtuals);
		}

		/// <summary>
		/// Convert the string into an object.
		/// </summary>
		/// <param name="item"> The JSON data to deserialize. </param>
		/// <param name="type"> The type to convert into. </param>
		/// <returns> The deserialized object. </returns>
		public static object FromJson(this string item, Type type)
		{
			return string.IsNullOrWhiteSpace(item) ? null : JsonConvert.DeserializeObject(item, type, _serializationSettingsNoVirtuals);
		}

		/// <summary>
		/// Gets the real type of the entity. For use with proxy entities.
		/// </summary>
		/// <param name="item"> The object to process. </param>
		/// <returns> The real base type for the proxy or just the initial type if it is not a proxy. </returns>
		public static Type GetRealType(this object item)
		{
			var type = item.GetType();
			var isProxy = type.FullName.Contains("System.Data.Entity.DynamicProxies");
			return isProxy ? type.BaseType : type;
		}

		/// <summary>
		/// Get the serialization settings.
		/// </summary>
		/// <param name="camelCase"> True to camelCase or else use PascalCase. </param>
		/// <param name="ignoreVirtuals"> True to ignore virtual members else include them. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <returns> The serialization settings. </returns>
		public static JsonSerializerSettings GetSerializerSettings(bool camelCase, bool ignoreVirtuals, bool ignoreNullValues)
		{
			var response = new JsonSerializerSettings();
			response.Converters.Add(new IsoDateTimeConverter());
			response.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
			response.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			response.NullValueHandling = ignoreNullValues ? NullValueHandling.Ignore : NullValueHandling.Include;
			
			if (camelCase)
			{
				response.Converters.Add(new StringEnumConverter { CamelCaseText = true });
				response.ContractResolver = new CamelCasePropertyNamesContractResolver();
			}

			if (ignoreVirtuals)
			{
				response.ContractResolver = new IgnoreVirtualsSerializeContractResolver();
			}

			return response;
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
		/// Converts the type to an assembly name. Does not include version. Ex. System.String,mscorlib
		/// </summary>
		/// <param name="type"> The type to get the assembly name for. </param>
		/// <returns> The assembly name for the provided type. </returns>
		public static string ToAssemblyName(this Type type)
		{
			return type.FullName + "," + type.Assembly.GetName().Name;
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="camelCase"> The flag to determine if we should use camel case or not. Default value is false. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. Default value is false. </param>
		/// <param name="ignoreNullValues"> True to ignore members that are null else include them. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, bool camelCase = false, bool indented = false, bool ignoreVirtuals = false, bool ignoreNullValues = false)
		{
			var settings = GetSerializerSettings(camelCase, ignoreVirtuals, ignoreNullValues);
			return JsonConvert.SerializeObject(item, indented ? Formatting.Indented : Formatting.None, settings);
		}

		/// <summary>
		/// Serialize an object into a JSON string.
		/// </summary>
		/// <typeparam name="T"> The type of the object to serialize. </typeparam>
		/// <param name="item"> The object to serialize. </param>
		/// <param name="settings"> The settigs to use during serialization. </param>
		/// <param name="indented"> The flag to determine if the JSON should be indented or not. Default value is false. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, JsonSerializerSettings settings, bool indented = false)
		{
			return JsonConvert.SerializeObject(item, indented ? Formatting.Indented : Formatting.None, settings);
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
			return test() ? action() : default(T);
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

		internal static void AddExceptionToBuilder(StringBuilder builder, Exception ex)
		{
			builder.Append(builder.Length > 0 ? "\r\n" + ex.Message : ex.Message);

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException);
			}
		}

		/// <summary>
		/// Add or update a dictionary entry.
		/// </summary>
		/// <typeparam name="T1"> The type of the key. </typeparam>
		/// <typeparam name="T2"> The type of the value. </typeparam>
		/// <param name="dictionary"> The dictionary to update. </param>
		/// <param name="key"> The value of the key. </param>
		/// <param name="value"> The value of the value. </param>
		internal static void AddOrUpdate<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value)
		{
			if (dictionary.ContainsKey(key))
			{
				dictionary[key] = value;
				return;
			}

			dictionary.Add(key, value);
		}

		internal static MethodInfo CachedGetMethod(this Type type, string name, params Type[] types)
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

		internal static MethodInfo CachedMakeGenericMethod(this MethodInfo info, Type[] arguments)
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

		internal static IList<MethodInfo> GetCachedAccessors(this PropertyInfo info)
		{
			MethodInfo[] response;
			var key = info.ReflectedType?.FullName + "." + info.Name;

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

		internal static IList<Type> GetCachedGenericArguments(this MethodInfo info)
		{
			Type[] response;
			var fullName = info.ReflectedType?.FullName + "." + info.Name;
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

		internal static IList<Type> GetCachedGenericArguments(this Type type)
		{
			Type[] response;

			if (_types.ContainsKey(type.FullName))
			{
				if (_types.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetGenericArguments();
			return _types.AddOrUpdate(type.FullName, response, (s, types) => response);
		}

		internal static IList<MethodInfo> GetCachedMethods(this Type type, BindingFlags flags)
		{
			MethodInfo[] response;

			if (_methodInfos.ContainsKey(type.FullName))
			{
				if (_methodInfos.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetMethods(flags);
			return _methodInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
		}

		internal static IList<ParameterInfo> GetCachedParameters(this MethodInfo info)
		{
			ParameterInfo[] response;
			var fullName = info.ReflectedType?.FullName + "." + info.Name;
			var key = info.ToString().Replace(info.Name, fullName);

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

		internal static IList<PropertyInfo> GetCachedProperties(this Type type)
		{
			PropertyInfo[] response;

			if (_propertyInfos.ContainsKey(type.FullName))
			{
				if (_propertyInfos.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
			return _propertyInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
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
		/// Safely create a file.
		/// </summary>
		/// <param name="file"> The information of the file to create. </param>
		internal static void SafeCreate(this FileInfo file)
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

		#endregion
	}
}