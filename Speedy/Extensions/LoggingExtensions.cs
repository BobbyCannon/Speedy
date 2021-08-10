#region References

using System.Diagnostics.Tracing;
using System.Linq;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for logging.
	/// </summary>
	public static class LoggingExtensions
	{
		#region Methods

		/// <summary>
		/// Convert the event written event argument to its payload string
		/// </summary>
		/// <param name="args"> The item to process. </param>
		/// <returns> The formatted message. </returns>
		public static string GetDetailedMessage(this EventWrittenEventArgs args)
		{
			return $"{args.Payload[2]} {args.Level} : {args.GetMessage()}";
		}

		/// <summary>
		/// Convert the event written event argument to its payload string
		/// </summary>
		/// <param name="args"> The item to process. </param>
		/// <returns> The formatted message. </returns>
		public static string GetMessage(this EventWrittenEventArgs args)
		{
			return string.Format(args.Message, args.Payload.ToArray());
		}

		#endregion
	}
}