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

		/// <summary>
		/// Update the SyncStatistics with an update.
		/// </summary>
		/// <param name="update"> The update to be applied. </param>
		/// <param name="exclusions"> An optional set of properties to exclude. </param>
		public override void UpdateWith(SyncClientOptions update, params string[] exclusions)
		{
			// If the update is null then there is nothing to do.
			if (update == null)
			{
				return;
			}

			// ****** You can use CodeGeneratorTests.GenerateUpdateWith to update this ******

			if (exclusions.Length <= 0)
			{
				EnablePrimaryKeyCache = update.EnablePrimaryKeyCache;
				IsServerClient = update.IsServerClient;
			}
			else
			{
				this.IfThen(x => !exclusions.Contains(nameof(EnablePrimaryKeyCache)), x => x.EnablePrimaryKeyCache = update.EnablePrimaryKeyCache);
				this.IfThen(x => !exclusions.Contains(nameof(IsServerClient)), x => x.IsServerClient = update.IsServerClient);
			}
		}

		/// <inheritdoc />
		public override void UpdateWith(object update, params string[] exclusions)
		{
			switch (update)
			{
				case SyncClientOptions options:
				{
					UpdateWith(options, exclusions);
					return;
				}
				default:
				{
					base.UpdateWith(update, exclusions);
					return;
				}
			}
		}

		#endregion
	}
}