#region References

using System.IO;
using System.Text;

#endregion

namespace Speedy.Streams
{
	/// <summary>
	/// Encapsulates a stream writer which does not close the underlying stream.
	/// </summary>
	internal class NoCloseStreamWriter : StreamWriter
	{
		#region Constructors

		/// <summary>
		/// Creates a new stream writer object.
		/// </summary>
		/// <param name="stream"> The underlying stream to write to. </param>
		/// <param name="encoding"> The encoding for the stream. </param>
		public NoCloseStreamWriter(Stream stream, Encoding encoding)
			: base(stream, encoding)
		{
		}

		/// <summary>
		/// Creates a new stream writer object using default encoding.
		/// </summary>
		/// <param name="stream"> The underlying stream to write to. </param>
		public NoCloseStreamWriter(Stream stream)
			: this(stream, Encoding.UTF8)
		{
		}

		#endregion

		#region Methods

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <param name="disposing"> Should be true if managed resources should be disposed. </param>
		protected override void Dispose(bool disposing)
		{
			// Dispose the stream writer but pass false to the dispose
			// method to stop it from closing the underlying stream
			base.Dispose(false);
		}

		#endregion
	}
}