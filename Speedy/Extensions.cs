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
using Speedy.Configuration;
using Speedy.Exceptions;
using Speedy.Storage;
using Speedy.Sync;

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
		/// Sync a list of entities changes.
		/// </summary>
		/// <param name="provider"> The database provider  to sync changes for. </param>
		/// <param name="objects"> The list of objects that have changed. </param>
		/// <param name="corrections"> True if applying corrections or if applying changes. Defaults to false (changes). </param>
		public static IEnumerable<SyncIssue> ApplySyncChanges(this ISyncableDatabaseProvider provider, IEnumerable<SyncObject> objects, bool corrections = false)
		{
			var groups = objects.GroupBy(x => x.TypeName).OrderBy(x => x.Key);

			if (provider.Options.SyncOrder.Any())
			{
				var order = provider.Options.SyncOrder;
				groups = groups.OrderBy(x => x.Key == order[0]);
				groups = order.Skip(1).Aggregate(groups, (current, typeName) => current.ThenBy(x => x.Key == typeName));
			}

			var response = new List<SyncIssue>();

			groups.ForEach(x => ProcessSyncObjects(provider, x.Where(y => y.Status != SyncObjectStatus.Deleted), response, corrections));
			groups.Reverse().ForEach(x => ProcessSyncObjects(provider, x.Where(y => y.Status == SyncObjectStatus.Deleted), response, corrections));

			return response;
		}

		/// <summary>
		/// Sync a list of entities changes.
		/// </summary>
		/// <param name="provider"> The database provider to sync changes for. </param>
		/// <param name="objects"> The list of objects that have changed. </param>
		public static IEnumerable<SyncIssue> ApplySyncCorrections(this ISyncableDatabaseProvider provider, IEnumerable<SyncObject> objects)
		{
			return ApplySyncChanges(provider, objects, true);
		}

		/// <summary>
		/// Deep clone the item.
		/// </summary>
		/// <typeparam name="T"> The type to clone. </typeparam>
		/// <param name="item"> The item to clone. </param>
		/// <param name="ignoreVirtuals"> Flag to ignore the virtual properties. </param>
		/// <returns> The clone of the item. </returns>
		public static T DeepClone<T>(this T item, bool ignoreVirtuals)
		{
			return FromJson<T>(item.ToJson(ignoreVirtuals));
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
			return item.Length > 0 && _validJsonStartCharacters.Contains(item[0]) ? JsonConvert.DeserializeObject<T>(item, _serializationSettingsNoVirtuals) : JsonConvert.DeserializeObject<T>("\"" + item + "\"", _serializationSettingsNoVirtuals);
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
		/// Gets count of changes from the database.
		/// </summary>
		/// <param name="provider"> The database provider to query. </param>
		/// <param name="request"> The details of the request. </param>
		/// <returns> The count of changes from the server. </returns>
		public static int GetSyncChangeCount(this ISyncableDatabaseProvider provider, SyncRequest request)
		{
			using (var database = provider.GetDatabase())
			{
				return database.GetSyncTombstones(x => x.CreatedOn >= request.Since && x.CreatedOn < request.Until).Count()
					+ database.GetSyncableRepositories().Sum(repository => repository.GetChangeCount(request.Since, request.Until));
			}
		}

		/// <summary>
		/// Gets the changes from the database.
		/// </summary>
		/// <param name="provider"> The database provider to query. </param>
		/// <param name="request"> The details of the request. </param>
		/// <returns> The list of changes from the server. </returns>
		public static IEnumerable<SyncObject> GetSyncChanges(this ISyncableDatabaseProvider provider, SyncRequest request)
		{
			var response = new List<SyncObject>();
			var currentSkippedCount = 0;

			using (var database = provider.GetDatabase())
			{
				foreach (var repository in database.GetSyncableRepositories())
				{
					var changeCount = repository.GetChangeCount(request.Since, request.Until);
					if (changeCount + currentSkippedCount <= request.Skip)
					{
						currentSkippedCount += changeCount;
						continue;
					}

					var items = repository.GetChanges(request.Since, request.Until, request.Skip - currentSkippedCount, request.Take).ToList();
					response.AddRange(items);
					currentSkippedCount += items.Count;

					if (response.Count >= request.Take)
					{
						return response;
					}
				}

				var tombstoneQuery = database.GetSyncTombstones(x => x.CreatedOn >= request.Since && x.CreatedOn < request.Until);
				var tombstoneCount = tombstoneQuery.Count();
				if (tombstoneCount + currentSkippedCount <= request.Skip)
				{
					return response;
				}

				tombstoneQuery = tombstoneQuery
					.OrderBy(x => x.CreatedOn)
					.ThenBy(x => x.Id)
					.AsQueryable();

				if (request.Skip - currentSkippedCount > 0)
				{
					tombstoneQuery = tombstoneQuery.Skip(request.Skip - currentSkippedCount);
				}

				var tombStones = tombstoneQuery
					.Take(request.Take)
					.ToList()
					.Select(x => x.ToSyncObject())
					.ToList();

				response.AddRange(tombStones);
				return response;
			}
		}

		/// <summary>
		/// Gets the correction sync objects for the sync issues.
		/// </summary>
		/// <param name="provider"> The database provider to query. </param>
		/// <param name="issues"> The issues to try and correct. </param>
		/// <returns> The list of changes from the server. </returns>
		public static IEnumerable<SyncObject> GetSyncCorrections(this ISyncableDatabaseProvider provider, IEnumerable<SyncIssue> issues)
		{
			var response = new List<SyncObject>();

			using (var database = provider.GetDatabase())
			{
				foreach (var issue in issues)
				{
					switch (issue.IssueType)
					{
						default:
							// Assuming this is because this entity or a relationship it depends on was deleted but then used 
							// in another client or server. This means we should sync it again.
							var repository = database.GetSyncableRepository(Type.GetType(issue.TypeName));
							var entity = repository.Read(issue.Id);

							if (entity != null)
							{
								response.Add(entity.ToSyncObject());
							}
							break;
					}
				}

				return response;
			}
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
		/// <param name="ignoreVirtuals"> Flag to ignore virtual members. </param>
		/// <returns> The JSON string of the serialized object. </returns>
		public static string ToJson<T>(this T item, bool ignoreVirtuals)
		{
			return JsonConvert.SerializeObject(item, Formatting.None, ignoreVirtuals ? _serializationSettings : _serializationSettingsNoVirtuals);
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

			response = type.GetProperties();
			return _propertyInfos.AddOrUpdate(type.FullName, response, (s, infos) => response);
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

		private static void ProcessSyncObject(SyncObject syncObject, ISyncableDatabase database, bool correction)
		{
			var syncEntity = syncObject.ToSyncEntity();
			var tombstone = database.GetSyncTombstones(x => x.SyncId == syncEntity.SyncId).FirstOrDefault();
			if (tombstone != null)
			{
				if (!correction)
				{
					Debug.WriteLine("Tombstoned: " + syncEntity.SyncId);
					return;
				}

				database.RemoveSyncTombstones(x => x.SyncId == syncEntity.SyncId);
			}

			var type = syncEntity.GetType();
			var repository = database.GetSyncableRepository(type);
			if (repository == null)
			{
				throw new InvalidDataException("Failed to find a syncable repository for the entity.");
			}

			var foundEntity = repository.Read(syncEntity.SyncId);
			var syncStatus = syncObject.Status;

			if (foundEntity != null && syncObject.Status == SyncObjectStatus.Added)
			{
				syncStatus = SyncObjectStatus.Modified;
			}
			else if (foundEntity == null && syncObject.Status == SyncObjectStatus.Modified)
			{
				syncStatus = SyncObjectStatus.Added;
			}

			switch (syncStatus)
			{
				case SyncObjectStatus.Added:
					syncEntity.Id = 0;
					syncEntity.UpdateLocalRelationships(database);
					repository.Add(syncEntity);
					break;

				case SyncObjectStatus.Modified:
					foundEntity?.UpdateLocalRelationships(database);

					if (foundEntity?.ModifiedOn < syncEntity.ModifiedOn || correction)
					{
						foundEntity?.Update(syncEntity);
					}
					break;

				case SyncObjectStatus.Deleted:
					if (foundEntity != null)
					{
						repository.Remove(foundEntity);
					}
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private static void ProcessSyncObjects(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			var syncObjectList = syncObjects.ToList();

			try
			{
				using (var database = provider.GetDatabase())
				{
					database.Options.MaintainDates = false;
					syncObjectList.ForEach(x => ProcessSyncObject(x, database, corrections));
					database.SaveChanges();
				}
			}
			catch
			{
				ProcessSyncObjectsIndividually(provider, syncObjectList, issues, corrections);
			}
		}

		private static void ProcessSyncObjectsIndividually(ISyncableDatabaseProvider provider, IEnumerable<SyncObject> syncObjects, ICollection<SyncIssue> issues, bool corrections)
		{
			foreach (var syncObject in syncObjects)
			{
				try
				{
					using (var database = provider.GetDatabase())
					{
						database.Options.MaintainDates = false;
						ProcessSyncObject(syncObject, database, corrections);
						database.SaveChanges();
					}
				}
				catch (SyncIssueException ex)
				{
					ex.Issues.ForEach(issues.Add);
					issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = syncObject.TypeName });
				}
				catch (InvalidOperationException)
				{
					issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = syncObject.TypeName });
				}
				catch (Exception ex)
				{
					var details = ex.ToDetailedString();

					// Cannot catch the DbUpdateException without reference EntityFramework.
					if (details.Contains("conflicted with the FOREIGN KEY constraint")
						|| details.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
					{
						issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.RelationshipConstraint, TypeName = syncObject.TypeName });
						continue;
					}

					issues.Add(new SyncIssue { Id = syncObject.SyncId, IssueType = SyncIssueType.Unknown, TypeName = syncObject.TypeName });
				}
			}
		}

		/// <summary>
		/// Updates the entities local relationships.
		/// </summary>
		/// <param name="entity"> The entity to update. </param>
		/// <param name="database"> The database with the relationship repositories. </param>
		/// <exception cref="SyncIssueException"> An exception will all sync issues. </exception>
		private static void UpdateLocalRelationships(this SyncEntity entity, ISyncableDatabase database)
		{
			var response = new List<SyncIssue>();

			foreach (var relationship in GetRelationshipConfigurations(entity))
			{
				if (!relationship.SyncId.HasValue)
				{
					continue;
				}

				var foundEntity = database.GetSyncableRepository(relationship.Type)?.Read(relationship.SyncId.Value);
				if (foundEntity != null)
				{
					relationship.IdPropertyInfo.SetValue(entity, foundEntity.Id);
					continue;
				}

				response.Add(new SyncIssue { Id = relationship.SyncId.Value, IssueType = SyncIssueType.RelationshipConstraint, TypeName = relationship.Type.ToAssemblyName() });
			}

			if (response.Any(x => x != null))
			{
				throw new SyncIssueException("This entity has relationship issues.", response.Where(x => x != null));
			}
		}

		private static IEnumerable<Relationship> GetRelationshipConfigurations(SyncEntity entity)
		{
			var syncEntityType = typeof(SyncEntity);
			var properties = entity.GetRealType().GetProperties();
			var syncProperties = properties
				.Where(x => syncEntityType.IsAssignableFrom(x.PropertyType))
				.Select(x => new
				{
					IdProperty = properties.FirstOrDefault(y => y.Name == x.Name + "Id"),
					SyncIdProperty = properties.FirstOrDefault(y => y.Name == x.Name + "SyncId"),
					Type = x.PropertyType
				})
				.ToList();

			var response = syncProperties
				.Where(x => x.IdProperty != null)
				.Where(x => x.SyncIdProperty != null)
				.Select(x => new Relationship
				{
					IdPropertyInfo = x.IdProperty,
					SyncId = (Guid?) x.SyncIdProperty.GetValue(entity),
					Type = x.Type
				})
				.ToList();

			return response;
		}

		/// <summary>
		/// Gets a detailed string of the exception. Includes messages of all exceptions.
		/// </summary>
		/// <param name="ex"> The exception to process. </param>
		/// <returns> The detailed string of the exception. </returns>
		private static string ToDetailedString(this Exception ex)
		{
			var builder = new StringBuilder();
			AddExceptionToBuilder(builder, ex);
			return builder.ToString();
		}

		#endregion
	}
}