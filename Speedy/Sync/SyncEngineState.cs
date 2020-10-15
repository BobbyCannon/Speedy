#region References

using System;
using System.Linq;
using Speedy.Extensions;
using Speedy.Storage;

#endregion

namespace Speedy.Sync
{
	/// <summary>
	/// Event arguments for the sync engine status change event.
	/// </summary>
	public class SyncEngineState : Bindable<SyncEngineState>
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instances of the sync engine state.
		/// </summary>
		/// <param name="dispatcher"> An optional dispatcher. </param>
		public SyncEngineState(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		#endregion

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
		public double Percent
		{
			get
			{
				var result = Total == 0 ? 0 : Math.Round((double) Count / Total * 100, 2);
				return double.IsNaN(result) || double.IsInfinity(result) ? 0 : result;
			}
		}

		/// <summary>
		/// Indicates if the sync engine is running.
		/// </summary>
		public bool IsRunning =>
			Status != SyncEngineStatus.Cancelled
			&& Status != SyncEngineStatus.Completed
			&& Status != SyncEngineStatus.Stopped;

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
		public override void OnPropertyChanged(string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Count):
				case nameof(Total):
					OnPropertyChanged(nameof(Percent));
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

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

		/// <inheritdoc />
		public override void UpdateWith(SyncEngineState update, params string[] exclusions)
		{
			if (update == null)
			{
				return;
			}

			this.IfThen(x => !exclusions.Contains(nameof(Count)), x => x.Count = update.Count);
			this.IfThen(x => !exclusions.Contains(nameof(Message)), x => x.Message = update.Message);
			this.IfThen(x => !exclusions.Contains(nameof(Status)), x => x.Status = update.Status);
			this.IfThen(x => !exclusions.Contains(nameof(Total)), x => x.Total = update.Total);
		}

		/// <inheritdoc />
		public override void UpdateWith(object value, params string[] exclusions)
		{
			UpdateWith(value as SyncEngineState, exclusions);
		}

		/// <inheritdoc />
		public override void UpdateWithOnly(SyncEngineState update, params string[] inclusions)
		{
			if (update == null)
			{
				return;
			}

			this.IfThen(x => inclusions.Contains(nameof(Count)), x => x.Count = update.Count);
			this.IfThen(x => inclusions.Contains(nameof(Message)), x => x.Message = update.Message);
			this.IfThen(x => inclusions.Contains(nameof(Status)), x => x.Status = update.Status);
			this.IfThen(x => inclusions.Contains(nameof(Total)), x => x.Total = update.Total);
		}

		/// <inheritdoc />
		public override void UpdateWithOnly(object value, params string[] inclusions)
		{
			UpdateWithOnly(value as SyncEngineState, inclusions);
		}

		#endregion
	}
}