#region References

using System.IO;
using System.Text;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for stream
/// </summary>
public static class StreamExtensions
{
	#region Methods

	/// <summary>
	/// Read a string from a stream.
	/// </summary>
	/// <param name="stream"> The stream to read from. </param>
	/// <param name="bufferLength"> The buffer length for individual reads. Defaults to 1024. </param>
	/// <returns> The string to read. </returns>
	public static string ReadString(this Stream stream, int bufferLength = 1024)
	{
		if (stream.CanSeek)
		{
			stream.Seek(0, SeekOrigin.Begin);
		}

		var builder = new StringBuilder();
		var buffer = new byte[bufferLength];
		int read;

		do
		{
			read = stream.Read(buffer, 0, bufferLength);
			if (read > 0)
			{
				builder.Append(Encoding.Default.GetString(buffer, 0, read));
			}
		} while (read > 0);

		return builder.ToString();
	}

	/// <summary>
	/// Read a string from a stream then disposes of the stream
	/// </summary>
	/// <param name="stream"> The stream to read from. </param>
	/// <param name="bufferLength"> The buffer length for individual reads. Defaults to 1024. </param>
	/// <returns> The string to read. </returns>
	public static string ReadStringAndCleanup(this Stream stream, int bufferLength = 1024)
	{
		using (stream)
		{
			return ReadString(stream, bufferLength);
		}
	}

	#endregion
}