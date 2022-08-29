#region References

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

#endregion

namespace Speedy.Automation.Desktop
{
	/// <summary>
	/// Represents a cursor for the mouse.
	/// </summary>
	public class MouseCursor
	{
		#region Fields

		/// <summary>
		/// Gets the cursor that represents default and wait.
		/// </summary>
		public static readonly MouseCursor DefaultAndWait;

		/// <summary>
		/// Gets the cursor that represents a pointer.
		/// </summary>
		public static readonly MouseCursor Pointer;

		/// <summary>
		/// Gets the cursor that represents text edit.
		/// </summary>
		public static readonly MouseCursor ShapedCursor;

		/// <summary>
		/// Gets the cursor that represents wait.
		/// </summary>
		public static readonly MouseCursor Wait;

		private readonly int _value;

		private static readonly List<MouseCursor> _waitCursors;

		#endregion

		#region Constructors

		static MouseCursor()
		{
			DefaultAndWait = new MouseCursor(StandardCursors.AppStarting);
			ShapedCursor = new MouseCursor(StandardCursors.Ibeam);
			Pointer = new MouseCursor(StandardCursors.Arrow);
			Wait = new MouseCursor(StandardCursors.Wait);
			_waitCursors = new List<MouseCursor> { DefaultAndWait, Wait };
		}

		private MouseCursor(int value)
		{
			_value = value;
		}

		private MouseCursor(StandardCursors cursor)
		{
			var c = LoadCursor(IntPtr.Zero, cursor);
			_value = c.ToInt32();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets the current cursor for the mouse.
		/// </summary>
		public static MouseCursor Current
		{
			get
			{
				var cursorInfo = CursorInfo.Create();
				GetCursorInfo(ref cursorInfo);
				var i = cursorInfo.handle.ToInt32();
				return new MouseCursor(i);
			}
		}

		/// <summary>
		/// Gets a list of cursors that represent wait cursors.
		/// </summary>
		internal static IReadOnlyList<MouseCursor> WaitCursors => _waitCursors.AsReadOnly();

		#endregion

		#region Methods

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
		/// </summary>
		/// <returns>
		/// True if the specified object  is equal to the current object otherwise false.
		/// </returns>
		/// <param name="obj"> The object to compare with the current object. </param>
		/// <filterpriority> 2 </filterpriority>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != typeof(MouseCursor))
			{
				return false;
			}

			return ((MouseCursor) obj)._value == _value;
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object" />.
		/// </returns>
		/// <filterpriority> 2 </filterpriority>
		public override int GetHashCode()
		{
			return _value;
		}

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		/// <filterpriority> 2 </filterpriority>
		public override string ToString()
		{
			return _value.ToString();
		}

		[DllImport("user32.dll")]
		private static extern bool GetCursorInfo(ref CursorInfo cursorInfo);

		[DllImport("user32.dll")]
		private static extern IntPtr LoadCursor(IntPtr hInstance, StandardCursors lpCursorName);

		#endregion

		#region Structures

		[StructLayout(LayoutKind.Sequential)]
		private struct CursorInfo
		{
			#region Fields

			public readonly uint flags;
			public readonly IntPtr handle;
			public readonly Point point;
			public uint size;

			#endregion

			#region Methods

			public static CursorInfo Create()
			{
				return new CursorInfo { size = (uint) Marshal.SizeOf(typeof(CursorInfo)) };
			}

			#endregion
		}

		#endregion

		#region Enumerations

		private enum StandardCursors
		{
			Arrow = 32512,
			Ibeam = 32513,
			Wait = 32514,
			Cross = 32515,
			UpArrow = 32516,
			Size = 32640,
			Icon = 32641,
			SizeTopLeft = 32642,
			SizeTopRight = 32643,
			SizeLeftRight = 32644,
			SizeTopBottom = 32645,
			SizeAll = 32646,
			No = 32648,
			Hand = 32649,
			AppStarting = 32650,
			Help = 32651
		}

		#endregion
	}
}