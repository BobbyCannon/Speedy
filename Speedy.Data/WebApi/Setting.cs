#region References

using Speedy.Sync;

#endregion

namespace Speedy.Data.WebApi
{
	/// <summary>
	/// Represents the public setting model.
	/// </summary>
	public class Setting : SyncModel<long>
	{
		#region Properties

		public override long Id { get; set; }

		public string Name { get; set; }

		public string Value { get; set; }

		#endregion
	}
}