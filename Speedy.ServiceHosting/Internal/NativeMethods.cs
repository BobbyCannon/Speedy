#region References

using System.Runtime.InteropServices;

#endregion

namespace Speedy.ServiceHosting.Internal
{
	internal class NativeMethods
	{
		#region Methods

		[DllImport("kernel32.dll")]
		public static extern bool SetConsoleCtrlHandler(HandlerRoutine handler, bool add);

		#endregion

		#region Delegates

		public delegate void HandlerRoutine();

		#endregion
	}
}