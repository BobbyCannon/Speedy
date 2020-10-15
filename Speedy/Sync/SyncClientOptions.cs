#region References

using System;
using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the options for a sync client
	/// </summary>
	public class SyncClientOptions : Bindable<SyncClientOptions>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the sync client options.
		/// </summary>
		public SyncClientOptions()
		{
			PrimaryKeyCacheTimeout = TimeSpan.FromMinutes(15);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Determines if the sync client should cache primary keys for relationships.
		/// </summary>
		public bool EnablePrimaryKeyCache { get; set; }

		/// <summary>
		/// Indicates this client should maintain dates, meaning as you save data the ModifiedOn will be updated to the current time.
		/// This should really only be set for the "Master" sync client that represents the master dataset.
		/// </summary>
		public bool MaintainModifiedOn { get; set; }

		/// <summary>
		/// The amount of time to cache primary keys.
		/// </summary>
		public TimeSpan PrimaryKeyCacheTimeout { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override void UpdateWith(SyncClientOptions update, params string[] exclusions)
		{
			if (update is null)
			{
				return;
			}

			this.IfThen(x => !exclusions.Contains(nameof(MaintainModifiedOn)), x => x.MaintainModifiedOn = update.MaintainModifiedOn);
		}

		/// <inheritdoc />
		public override void UpdateWith(object value, params string[] exclusions)
		{
			UpdateWith(value as SyncClientOptions, exclusions);
		}

		/// <inheritdoc />
		public override void UpdateWithOnly(SyncClientOptions update, params string[] inclusions)
		{
			if (update is null)
			{
				return;
			}

			this.IfThen(x => inclusions.Contains(nameof(MaintainModifiedOn)), x => x.MaintainModifiedOn = update.MaintainModifiedOn);
		}

		/// <inheritdoc />
		public override void UpdateWithOnly(object update, params string[] inclusions)
		{
			UpdateWithOnly(update as SyncClientOptions, inclusions);
		}

		#endregion
	}
}