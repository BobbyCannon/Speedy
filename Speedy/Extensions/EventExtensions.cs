#region References

using System.Collections.Generic;
using System.Linq;
using Speedy.Logging;
using Speedy.Storage.KeyValue;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for key value repository
	/// </summary>
	public static class EventExtensions
	{
		#region Methods

		/// <summary>
		/// Add or update the collection with the event value.
		/// </summary>
		/// <param name="collection"> The collection to update. </param>
		/// <param name="name"> The event name to add or update. </param>
		/// <param name="value"> The event value to add or update. </param>
		public static void AddOrUpdate(this ICollection<EventValue> collection, string name, object value)
		{
			collection.AddOrUpdate(new EventValue(name, value));
		}

		/// <summary>
		/// Add or update the collection with the event value.
		/// </summary>
		/// <param name="collection"> The collection to update. </param>
		/// <param name="eventValue"> The event value to add or update. </param>
		public static void AddOrUpdate(this ICollection<EventValue> collection, EventValue eventValue)
		{
			var foundItem = collection.FirstOrDefault(x => x.Name == eventValue.Name);
			if (foundItem != null)
			{
				foundItem.Value = eventValue.Value;
				return;
			}

			collection.Add(eventValue);
		}

		/// <summary>
		/// Adds or updates the item in the collection.
		/// </summary>
		/// <param name="collection"> The collection to be updated. </param>
		/// <param name="items"> The items to be added or updated. </param>
		public static void AddOrUpdate(this ICollection<EventValue> collection, params EventValue[] items)
		{
			foreach (var item in items)
			{
				collection.AddOrUpdate(item);
			}
		}

		/// <summary>
		/// Creates a repository and writes the first session event.
		/// </summary>
		/// <param name="provider"> The provider to start a new repository on. </param>
		/// <param name="session"> The session event to start the repository with. </param>
		/// <returns> The repository containing the session event. </returns>
		public static IKeyValueRepository<Event> OpenRepository(this IKeyValueRepositoryProvider<Event> provider, Event session)
		{
			var repository = provider.OpenRepository(session.Id.ToString());
			repository.WriteAndSave(session);
			return repository;
		}

		/// <summary>
		/// Write the event to the repository and save it.
		/// </summary>
		/// <param name="repository"> The repository to write to. </param>
		/// <param name="value"> The event to be written to the repository. </param>
		internal static void WriteAndSave(this IKeyValueRepository<Event> repository, Event value)
		{
			repository.Write(value.Id.ToString(), value);
			repository.Save();
		}

		#endregion
	}
}