#region References

using Speedy.Configuration.CommandLine;

#endregion

namespace Speedy.ServiceHosting
{
	/// <summary>
	/// Represents a specific argument for a windows service.
	/// </summary>
	public class WindowsServiceArgument : CommandLineArgument
	{
		#region Constructors

		/// <summary>
		/// Instantiates an instance of the windows service command line argument.
		/// </summary>
		public WindowsServiceArgument()
		{
			IncludeInServiceArguments = true;
		}

		#endregion

		#region Properties

		/// <summary>
		/// Determine if this argument should be included in the windows service argument string. Defaults to true (include).
		/// </summary>
		public bool IncludeInServiceArguments { get; set; }

		#endregion
	}
}