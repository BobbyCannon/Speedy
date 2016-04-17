#region References

using System;
using System.Collections.Generic;
using System.Data;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represent an entity that can be synced.
	/// </summary>
	public abstract class SyncEntity : ModifiableEntity
	{
		#region Fields

		private static readonly HashSet<string> _ignoreProperties;

		#endregion

		#region Constructors

		static SyncEntity()
		{
			_ignoreProperties = new HashSet<string>(new[] { "Id", "SyncId" });
		}

		#endregion

		#region Properties

		/// <summary>
		/// The ID of the sync entity.
		/// </summary>
		/// <remarks>
		/// This ID is should be globally unique. Never reuse GUIDs.
		/// </remarks>
		public Guid SyncId { get; set; }

		/// <summary>
		/// Gets or sets the sync status for the sync entity.
		/// </summary>
		public SyncStatus SyncStatus { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Update the entity with the changes.
		/// </summary>
		/// <param name="update"> The entity with the changes. </param>
		public void Update(SyncEntity update)
		{
			var type = GetType();
			var updateType = update.GetType();

			if (type.FullName != updateType.FullName)
			{
				throw new DataException("Trying update a sync entity with a mismatched type.");
			}

			var properties = type.GetCachedProperties();
			foreach (var property in properties)
			{
				if (_ignoreProperties.Contains(property.Name))
				{
					continue;
				}

				property.SetValue(this, property.GetValue(update));
			}
		}

		#endregion
	}
}