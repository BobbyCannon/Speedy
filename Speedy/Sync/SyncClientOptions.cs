#region References

using System.Linq;
using Speedy.Extensions;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Represents the options for a sync client
	/// </summary>
	public class SyncClientOptions : CloneableBindable<SyncClientOptions>
	{
		#region Properties

		/// <summary>
		/// Determines if the sync client should cache primary keys for relationships.
		/// </summary>
		public bool EnablePrimaryKeyCache { get; set; }

		/// <summary>
		/// Indicates this client is the server and should maintain dates, meaning as you save data the CreatedOn, ModifiedOn will
		/// be updated to the current server time. This should only be set for the "Server" sync client that represents the primary database.
		/// </summary>
		public bool IsServerClient { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override SyncClientOptions DeepClone(int levels = -1)
		{
			return new()
			{
				EnablePrimaryKeyCache = EnablePrimaryKeyCache,
				IsServerClient = IsServerClient
			};
		}

		/// <inheritdoc />
		public override void UpdateWith(SyncClientOptions update, params string[] exclusions)
		{
			if (update is null)
			{
				return;
			}

			this.IfThen(x => !exclusions.Contains(nameof(EnablePrimaryKeyCache)), x => x.EnablePrimaryKeyCache = update.EnablePrimaryKeyCache);
			this.IfThen(x => !exclusions.Contains(nameof(IsServerClient)), x => x.IsServerClient = update.IsServerClient);
		}

		#endregion
	}
}