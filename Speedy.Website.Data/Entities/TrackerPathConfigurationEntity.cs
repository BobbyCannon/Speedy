#region References

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;
using Speedy.Website.Samples.Enumerations;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class TrackerPathConfigurationEntity : SyncEntity<int>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public TrackerPathConfigurationEntity()
		{
			Paths = new List<TrackerPathEntity>();
		}

		#endregion

		#region Properties

		public string CompletedOnName { get; set; }

		public string DataName { get; set; }

		/// <inheritdoc />
		public override int Id { get; set; }

		public string Name01 { get; set; }

		public string Name02 { get; set; }

		public string Name03 { get; set; }

		public string Name04 { get; set; }

		public string Name05 { get; set; }

		public string Name06 { get; set; }

		public string Name07 { get; set; }

		public string Name08 { get; set; }

		public string Name09 { get; set; }

		public string PathName { get; set; }

		public virtual ICollection<TrackerPathEntity> Paths { get; set; }

		public string PathType { get; set; }

		public string StartedOnName { get; set; }

		public PathValueDataType Type01 { get; set; }

		public PathValueDataType Type02 { get; set; }

		public PathValueDataType Type03 { get; set; }

		public PathValueDataType Type04 { get; set; }

		public PathValueDataType Type05 { get; set; }

		public PathValueDataType Type06 { get; set; }

		public PathValueDataType Type07 { get; set; }

		public PathValueDataType Type08 { get; set; }

		public PathValueDataType Type09 { get; set; }

		#endregion
	}
}