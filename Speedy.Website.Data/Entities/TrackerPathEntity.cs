#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Website.Data.Entities
{
	/// <summary>
	/// Represents a tracker path.
	/// </summary>
	public class TrackerPathEntity : SyncEntity<long>
	{
		#region Constructors

		/// <summary>
		/// Instantiates a new instance of the class.
		/// </summary>
		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public TrackerPathEntity()
		{
			Children = new List<TrackerPathEntity>();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets or set the child paths.
		/// </summary>
		public virtual ICollection<TrackerPathEntity> Children { get; set; }

		/// <summary>
		/// Gets or set the date and time the path was completed.
		/// </summary>
		public DateTime CompletedOn { get; set; }

		/// <summary>
		/// The path configuration for value mapping.
		/// </summary>
		public virtual TrackerPathConfigurationEntity Configuration { get; set; }

		/// <summary>
		/// Gets or sets the ID of the path configuration.
		/// </summary>
		public int ConfigurationId { get; set; }

		/// <summary>
		/// Gets or sets the raw data for the path.
		/// </summary>
		public string Data { get; set; }

		/// <summary>
		/// Gets the elapsed ticks.
		/// </summary>
		public long ElapsedTicks
		{
			get => ElapsedTime.Ticks;
			set
			{
				/* Only used for storage */
			}
		}

		/// <summary>
		/// Gets the elapsed time between the created and completed date and time.
		/// </summary>
		public TimeSpan ElapsedTime => CompletedOn - StartedOn;

		/// <inheritdoc />
		public override long Id { get; set; }

		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		public virtual TrackerPathEntity Parent { get; set; }

		/// <summary>
		/// Gets or sets the parent ID.
		/// </summary>
		public long? ParentId { get; set; }

		/// <summary>
		/// Gets or set the date and time the path was started.
		/// </summary>
		public DateTime StartedOn { get; set; }

		public string Value01 { get; set; }

		public string Value02 { get; set; }

		public string Value03 { get; set; }

		public string Value04 { get; set; }

		public string Value05 { get; set; }

		public string Value06 { get; set; }

		public string Value07 { get; set; }

		public string Value08 { get; set; }

		public string Value09 { get; set; }

		#endregion
	}
}