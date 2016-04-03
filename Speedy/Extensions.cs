#region References

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
		private static readonly JsonSerializerSettings _serializationSettings;
		private static readonly JsonSerializerSettings _serializationSettingsNoVirtuals;
		private static readonly ConcurrentDictionary<string, Type[]> _types;
		private static readonly char[] _validJsonStartCharacters;

		#endregion

		#region Constructors

		static Extensions()
		{
			_validJsonStartCharacters = new[] { '{', '[', '"' };
			_serializationSettings = GetSerializerSettings(true);
			_serializationSettingsNoVirtuals = GetSerializerSettings(false);
			_types = new ConcurrentDictionary<string, Type[]>();
			_methodInfos = new ConcurrentDictionary<string, MethodInfo[]>();
			_genericMethods = new ConcurrentDictionary<string, MethodInfo>();
			_methods = new ConcurrentDictionary<string, MethodInfo>();
			_propertyInfos = new ConcurrentDictionary<string, PropertyInfo[]>();
			_parameterInfos = new ConcurrentDictionary<string, ParameterInfo[]>();
		}

		#endregion

		#region Methods

		public static MethodInfo CachedGetMethod(this Type type, string name)
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

			response = type.GetMethod(name);
			return _methods.AddOrUpdate(key, response, (s, infos) => response);
		}

		public static MethodInfo CachedMakeGenericMethod(this MethodInfo info, Type[] arguments)
		{
			MethodInfo response;
			var fullName = info.ReflectedType?.FullName + "." + info.Name;
			var key = info.ToString().Replace(info.Name, fullName)
				+ string.Join(", ", arguments.Select(x => x.FullName));

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

		public static IList<MethodInfo> GetCachedAccessors(this PropertyInfo info)
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

		public static IList<Type> GetCachedGenericArguments(this MethodInfo info)
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

		public static IList<Type> GetCachedGenericArguments(this Type type)
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

		public static IList<MethodInfo> GetCachedMethods(this Type type, BindingFlags flags)
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

		public static IList<ParameterInfo> GetCachedParameters(this MethodInfo info)
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

		public static IList<PropertyInfo> GetCachedProperties(this Type type)
		{
			PropertyInfo[] response;

			if (_propertyInfos.ContainsKey(type.FullName))
			{
				if (_propertyInfos.TryGetValue(type.FullName, out response))
				{
					return response;
				}
			}

			response = type.GetProperties();
			return _propertyInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
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

		internal static T FromJson<T>(this string item)
		{
			return item.Length > 0 && _validJsonStartCharacters.Contains(item[0])
				? JsonConvert.DeserializeObject<T>(item, _serializationSettingsNoVirtuals)
				: JsonConvert.DeserializeObject<T>("\"" + item + "\"", _serializationSettingsNoVirtuals);
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

			Retry(() => File.Create(file.FullName).Dispose(), 1000, 10);

			Wait(() =>
			{
				file.Refresh();
				return file.Exists;
			}, 1000, 10);
		}

		internal static void SafeCreate(this DirectoryInfo directory)
		{
			directory.Refresh();
			if (directory.Exists)
			{
				return;
			}

			Retry(directory.Create, 1000, 10);

			Wait(() =>
			{
				directory.Refresh();
				return directory.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely delete a file.
		/// </summary>
		/// <param name="file"> The information of the file to delete. </param>
		internal static void SafeDelete(this FileInfo file)
		{
			file.Refresh();
			if (!file.Exists)
			{
				return;
			}

			Retry(file.Delete, 1000, 10);

			Wait(() =>
			{
				file.Refresh();
				return !file.Exists;
			}, 1000, 10);
		}

		/// <summary>
		/// Safely move a file.
		/// </summary>
		/// <param name="fileLocation"> The information of the file to move. </param>
		/// <param name="newLocation"> The location to move the file to. </param>
		internal static void SafeMove(this FileInfo fileLocation, FileInfo newLocation)
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

		internal static string ToJson<T>(this T item, bool ignoreVirtuals)
		{
			return JsonConvert.SerializeObject(item, Formatting.None, ignoreVirtuals ? _serializationSettings : _serializationSettingsNoVirtuals);
		}

		private static JsonSerializerSettings GetSerializerSettings(bool ignoreVirtuals)
		{
			var response = new JsonSerializerSettings();
			response.Converters.Add(new IsoDateTimeConverter());
			response.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;

			if (ignoreVirtuals)
			{
				response.ContractResolver = new IgnoreVirtualsSerializeContractResolver();
			}

			return response;
		}

		#endregion
	}
}