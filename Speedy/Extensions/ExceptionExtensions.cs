#region References

using System;
using System.Text;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for enumerations
	/// </summary>
	public static class ExceptionExtensions
	{
		#region Methods

		/// <summary>
		/// Clean the exception of the Parameter meta data.
		/// </summary>
		/// <param name="ex"> The exception to clean. </param>
		/// <returns> The cleaned message. </returns>
		public static string CleanMessage(this Exception ex)
		{
			var offset = ex.Message.IndexOf("\r\nParameter");
			return offset > 0 ? ex.Message.Substring(0, offset) : ex.Message;
		}

		/// <summary>
		/// Gets the message value of the exception. Skips all AggregateException messages.
		/// </summary>
		/// <param name="exception"> The exception to process. </param>
		/// <returns> The message value of the exception. </returns>
		public static string GetMessage(this Exception exception)
		{
			if (exception is AggregateException ex)
			{
				return GetMessage(ex.InnerException);
			}

			return exception?.Message;
		}

		/// <summary>
		/// Gets the details of the exception.
		/// </summary>
		/// <param name="ex"> The exception to be processed. </param>
		/// <param name="includeStackTrace"> Optionally include the stack trace. Defaults to true. </param>
		/// <returns> The detailed string for the exception. </returns>
		public static string ToDetailedString(this Exception ex, bool includeStackTrace = true)
		{
			var builder = new StringBuilder();
			AddExceptionToBuilder(builder, ex, includeStackTrace);
			return builder.ToString().Trim().Trim(Environment.NewLine.ToCharArray());
		}

		/// <summary>
		/// Add the exception details to the string builder.
		/// </summary>
		/// <param name="builder"> The builder to be appended to. </param>
		/// <param name="ex"> The exception to be processed. </param>
		/// <param name="includeStackTrace"> Optionally include the stack trace. Defaults to true. </param>
		private static void AddExceptionToBuilder(StringBuilder builder, Exception ex, bool includeStackTrace = true)
		{
			builder.Append(builder.Length > 0 ? Environment.NewLine + ex.Message : ex.Message);

			if (includeStackTrace)
			{
				builder.Append(builder.Length > 0 ? Environment.NewLine + ex.StackTrace : ex.StackTrace);
			}

			builder.AppendLine();

			if (ex.InnerException != null)
			{
				AddExceptionToBuilder(builder, ex.InnerException, includeStackTrace);
			}
		}

		#endregion
	}
}