﻿#region References

using System.IO;
using System.Text;

#endregion

namespace Speedy
{
	/// <summary>
	/// Encapsulates a stream reader which does not close the underlying stream.
	/// </summary>
	internal class NoCloseStreamReader : StreamReader
	{
		#region Constructors

		/// <summary>
		/// Creates a new stream reader object.
		/// </summary>
		/// <param name="stream"> The underlying stream to write to. </param>
		/// <param name="encoding"> The encoding for the stream. </param>
		public NoCloseStreamReader(Stream stream, Encoding encoding)
			: base(stream, encoding)
		{
		}

		/// <summary>
		/// Creates a new stream reader object using default encoding.
		/// </summary>
		/// <param name="stream"> The underlying stream to write to. </param>
		public NoCloseStreamReader(Stream stream)
			: base(stream)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Disposes of the stream reader.
		/// </summary>
		/// <param name="disposing"> True to dispose managed objects. </param>
		protected override void Dispose(bool disposing)
		{
			// Dispose the stream reader but pass false to the dispose
			// method to stop it from closing the underlying stream
			base.Dispose(false);
		}

		#endregion
	}
}