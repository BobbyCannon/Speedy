#region References

using System;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Event arguments for the sync engine status change event.
	/// </summary>
	public class SyncEngineState : Bindable
	{
		#region Properties

		/// <summary>
		/// Gets or sets the current count of items processed.
		/// </summary>
		public long Count { get; set; }

		/// <summary>
		/// Gets or set the message for the state.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Gets the percentage of progress. Ranging from [0.00] to [100.00].
		/// </summary>
		public double Percent => Total == 0 ? 0 : Math.Round((double) Count / Total * 100, 2);

		/// <summary>
		/// Gets or sets the current status of the sync.
		/// </summary>
		public SyncEngineStatus Status { get; set; }

		/// <summary>
		/// Gets or sets the total count of the items to process.
		/// </summary>
		public long Total { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override string ToString()
		{
			if (Total > 0)
			{
				return string.IsNullOrWhiteSpace(Message) 
					? $"{Status}: {Count} of {Total} ({Percent}%)"
					: $"{Status}: {Count} of {Total} ({Percent}%) - {Message}";
			}

			return string.IsNullOrWhiteSpace(Message) 
				? $"{Status}"
				: $"{Status}: {Message}";
		}

		#endregion
	}
}