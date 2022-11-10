#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Caching.Memory;
using Speedy.Extensions;
using Speedy.Profiling;
using Speedy.Website.Data;
using Speedy.Website.Data.Entities;

#endregion

namespace Speedy.Website.Core.Services
{
	public class TrackerService : ITrackerPathRepository
	{
		#region Fields

		private static readonly MemoryCache _cache;
		private static readonly List<PropertyInfo> _configurationNameInfos;
		private static readonly List<PropertyInfo> _eventValueInfos;
		private readonly IDatabaseProvider<IContosoDatabase> _provider;
		private readonly TimeSpan _timeout;

		#endregion

		#region Constructors

		public TrackerService(IDatabaseProvider<IContosoDatabase> provider) : this(provider, TimeSpan.Zero)
		{
		}

		public TrackerService(IDatabaseProvider<IContosoDatabase> provider, TimeSpan timeout)
		{
			_provider = provider;
			_timeout = timeout;
		}

		static TrackerService()
		{
			_cache = new MemoryCache(new MemoryCacheOptions());
			_eventValueInfos = typeof(TrackerPathEntity)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.Name.StartsWith("Value"))
				.ToList();

			_configurationNameInfos = typeof(TrackerPathConfigurationEntity)
				.GetProperties(BindingFlags.Instance | BindingFlags.Public)
				.Where(x => x.Name.StartsWith("Name"))
				.ToList();
		}

		#endregion

		#region Methods

		/// <summary>
		/// Writes a collection of events.
		/// </summary>
		/// <param name="events"> The events to write. </param>
		public void Write(params TrackerPath[] events)
		{
			using var database = _provider.GetDatabase();
			events.ForEach(x => AddEvent(database, x, null));

			try
			{
				database.SaveChanges();
			}
			catch
			{
				WriteEventsIndividually(events);
			}
		}

		private void AddEvent(IContosoDatabase database, TrackerPath item, TrackerPathEntity parent)
		{
			var entity = ToEntity(database, item, parent);
			database.TrackerPaths.Add(entity);
			item.Children.ForEach(x => AddEvent(database, x, entity));
		}

		private void AddOrUpdateEvent(IContosoDatabase database, TrackerPath item, TrackerPathEntity parent)
		{
			var entity = database.TrackerPaths.FirstOrDefault(x => x.SyncId == item.Id);

			if (entity != null)
			{
				Update(database, entity, item);
			}
			else
			{
				entity = ToEntity(database, item, parent);
				database.TrackerPaths.Add(entity);
			}

			item.Children.ForEach(x => AddOrUpdateEvent(database, x, entity));
		}

		private TrackerPathEntity ToEntity(IContosoDatabase database, TrackerPath item, TrackerPathEntity parent)
		{
			var response = new TrackerPathEntity { Parent = parent, SyncId = item.Id };
			Update(database, response, item);
			return response;
		}

		private void Update(IContosoDatabase database, TrackerPathEntity entity, TrackerPath item)
		{
			entity.CompletedOn = item.CompletedOn;
			entity.Data = item.Data;
			entity.StartedOn = item.StartedOn;
			entity.SyncId = item.Id;

			if (entity.SyncId == Guid.Empty)
			{
				entity.SyncId = Guid.NewGuid();
				item.Id = entity.SyncId;
			}

			var configuration = entity.Configuration ?? (_cache.TryGetValue(item.Name, out var result) ? (TrackerPathConfigurationEntity) result : entity.Configuration);

			if (configuration == null)
			{
				// Configuration doesn't exist on the entity or in the cache, so try and read it then cache if found
				configuration = database.TrackerPathConfigurations.FirstOrDefault(x => x.PathName == item.Name);

				if (configuration != null)
				{
					_cache.Set(item.Name, configuration, TimeService.UtcNow.Add(_timeout));
				}
			}

			if (configuration == null)
			{
				// Item was not on the entity, cached, or in the database. We need to add the event configuration
				configuration = new TrackerPathConfigurationEntity { PathName = item.Name, PathType = item.Type ?? item.Name };
				database.TrackerPathConfigurations.Add(configuration);
				database.SaveChanges();

				_cache.Set(item.Name, configuration, TimeService.UtcNow.Add(_timeout));
			}

			if ((entity.Configuration == null) && (entity.ConfigurationId == 0))
			{
				entity.ConfigurationId = configuration.Id;
			}

			foreach (var value in item.Values.OrderBy(x => x.Name))
			{
				var configurationValues = _configurationNameInfos.ToDictionary(x => x, x => x.GetValue(configuration)?.ToString());
				var configurationProperty = configurationValues.FirstOrDefault(x => x.Value == value.Name);

				if (configurationProperty.Key == null)
				{
					configurationProperty = configurationValues.FirstOrDefault(x => (x.Value == null) || (x.Value.Length <= 0));
					if (configurationProperty.Key == null)
					{
						continue;
					}

					configurationProperty.Key.SetValue(configuration, value.Name);
				}

				var eventProperty = _eventValueInfos.First(x => x.Name == configurationProperty.Key.Name.Replace("Name", "Value"));
				eventProperty.SetValue(entity, value.Value.MaxLength(900));
			}
		}

		private void WriteEventsIndividually(IEnumerable<TrackerPath> events)
		{
			using var database = _provider.GetDatabase();

			foreach (var x in events)
			{
				try
				{
					AddOrUpdateEvent(database, x, null);
					database.SaveChanges();
				}
				catch (Exception)
				{
					// log?
				}
			}
		}

		#endregion
	}
}