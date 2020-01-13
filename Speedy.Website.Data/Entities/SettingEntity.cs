#region References

using Speedy.Sync;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class SettingEntity : SyncEntity<long>
	{
		#region Properties

		public override long Id { get; set; }

		public string Name { get; set; }

		public string Value { get; set; }

		#endregion
	}
}