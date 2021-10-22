#region References

using System;
using System.IO;
using System.Linq;
using System.Net;

#endregion

namespace Speedy.ServiceHosting.Web
{
	/// <summary>
	/// An example of a windows service web service.
	/// </summary>
	public class WindowsServiceWebService : IWindowsServiceWebService
	{
		#region Fields

		private readonly string _appDataDirectory;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates
		/// </summary>
		/// <param name="directory"> </param>
		public WindowsServiceWebService(string directory)
		{
			_appDataDirectory = directory;
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public WindowsServiceUpdate CheckForUpdate(WindowsServiceDetails details)
		{
			var response = new WindowsServiceUpdate();
			if (!Directory.Exists(_appDataDirectory))
			{
				response.Size = -1;
				return response;
			}

			var filter = details.Name + "-*.zip";
			var zipFilePath = Directory.GetFiles(_appDataDirectory, filter).OrderByDescending(x => x).FirstOrDefault();
			if (zipFilePath == null)
			{
				response.Size = -2;
				return response;
			}

			var fileNameParts = Path.GetFileNameWithoutExtension(zipFilePath).Split('-');
			if (fileNameParts.Length != 2)
			{
				response.Size = -3;
				return response;
			}

			var version = fileNameParts[1];
			if (version != details.Version)
			{
				response.Name = Path.GetFileName(zipFilePath);
				response.Size = new FileInfo(zipFilePath).Length;
				return response;
			}

			return response;
		}

		/// <inheritdoc />
		public byte[] DownloadUpdateChunk(WindowsServiceUpdateRequest request)
		{
			if (!Directory.Exists(_appDataDirectory))
			{
				throw new Exception("Could not find the directory update.");
			}

			var filePath = _appDataDirectory + "\\" + request.Name;
			var fileInfo = new FileInfo(filePath);
			return !fileInfo.Exists ? Array.Empty<byte>() : FileChunk(fileInfo, request.Offset);
		}

		/// <inheritdoc />
		public void Login(NetworkCredential credentials)
		{
			throw new NotImplementedException();
		}

		private static byte[] FileChunk(FileInfo info, long offset)
		{
			if ((offset < 0) || (offset >= info.Length))
			{
				throw new ArgumentException("The offset is out of range.", nameof(offset));
			}

			using var file = File.Open(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
			if (offset >= file.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(offset), "The offset is larger than the file size.");
			}

			long length = 1048576;
			if ((offset + length) >= file.Length)
			{
				length = file.Length - offset;
			}

			var response = new byte[length];
			file.Position = offset;
			file.Read(response, 0, response.Length);
			return response;
		}

		#endregion
	}
}