#region References

using System;
using System.Threading;
using Speedy.Automation.Desktop;
using Speedy.Automation.Internal.Inputs;

#endregion

namespace Speedy.Automation
{
	/// <summary>
	/// A static combination a keyboard and mouse instance.
	/// </summary>
	public static class Input
	{
		#region Fields

		/// <summary>
		/// The instance of the <see cref="InputMessageDispatcher" /> to use for dispatching <see cref="InputTypeWithData" /> messages.
		/// </summary>
		private static readonly InputMessageDispatcher _messageDispatcher;

		#endregion

		#region Constructors

		static Input()
		{
			_messageDispatcher = new InputMessageDispatcher();

			Keyboard = new Keyboard();
			Mouse = new Mouse();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Represents the keyboard and allows for simulated input.
		/// </summary>
		public static Keyboard Keyboard { get; }

		/// <summary>
		/// Represents the mouse and allows for simulated input.
		/// </summary>
		public static Mouse Mouse { get; }

		#endregion

		#region Methods

		/// <summary>
		/// Dispatches the specified inputs from the provided InputBuilder in their specified order by issuing a single call.
		/// </summary>
		/// <param name="builder"> The builder containing the input. </param>
		public static InputBuilder SendInput(InputBuilder builder)
		{
			return SendInput(builder, TimeSpan.Zero);
		}

		/// <summary>
		/// Dispatches the specified inputs from the provided InputBuilder in their specified order by issuing a single call.
		/// </summary>
		/// <param name="builder"> The builder containing the input. </param>
		/// <param name="delay"> An optional delay after sending input. </param>
		public static InputBuilder SendInput(InputBuilder builder, TimeSpan delay)
		{
			_messageDispatcher.DispatchInput(builder.ToArray());

			if ((delay == TimeSpan.Zero) && (Keyboard.DefaultInputDelay != TimeSpan.Zero))
			{
				Thread.Sleep(Keyboard.DefaultInputDelay);
			}
			else if (delay > TimeSpan.Zero)
			{
				Thread.Sleep(delay);
			}

			return builder;
		}

		#endregion
	}
}