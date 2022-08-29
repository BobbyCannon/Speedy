#region References

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Speedy.Extensions;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Used to download files from web servers.
	/// </summary>
	public class FileDownloader
	{
		#region Fields

		private CancellationTokenSource _cancellationToken;

		#endregion

		#region Properties

		/// <summary>
		/// The amount of data received.
		/// </summary>
		public long BytesReceived { get; private set; }

		/// <summary>
		/// The calculated current speed.
		/// </summary>
		public long CurrentSpeed { get; private set; }

		/// <summary>
		/// Existing length in the case of resuming download.
		/// </summary>
		public long ExistingLength { get; set; }

		/// <summary>
		/// Determines if a download is in progress.
		/// </summary>
		public bool IsBusy => _cancellationToken != null;

		/// <summary>
		/// The percent of progress.
		/// </summary>
		public int ProgressPercentage => (int) ((BytesReceived / (float) TotalBytesToReceive) * 100);

		/// <summary>
		/// The calculated time left.
		/// </summary>
		public TimeSpan TimeLeft
		{
			get
			{
				var bytesRemaining = TotalBytesToReceive - BytesReceived;
				return CurrentSpeed == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(bytesRemaining / CurrentSpeed);
			}
		}

		/// <summary>
		/// The total bytes to receive.
		/// </summary>
		public long TotalBytesToReceive { get; private set; }

		#endregion

		#region Methods

		/// <summary>
		/// Cancels the download.
		/// </summary>
		public void Cancel()
		{
			_cancellationToken?.Cancel();
		}

		/// <summary>
		/// </summary>
		/// <param name="remoteUri"> The remote URI of the file. </param>
		/// <param name="localFilePath"> The local file path. </param>
		/// <param name="force"> The option to force to overwrite local file. </param>
		/// <param name="progressChanged"> Called when the file download progress changes. </param>
		/// <param name="cancellationCheck"> Called to check to see if download should be cancelled. </param>
		/// <returns> The file downloaded completed details. </returns>
		public FileDownloaderCompletedEventArgs Download(string remoteUri, string localFilePath, bool force, Action progressChanged = null, Func<bool> cancellationCheck = null)
		{
			var fileInfo = new FileInfo(localFilePath);
			if (fileInfo.Exists && force)
			{
				fileInfo.SafeDelete();
				fileInfo.Refresh();
			}

			ExistingLength = fileInfo.Exists ? fileInfo.Length : 0;

			try
			{
				var webRequest = (HttpWebRequest) WebRequest.Create(remoteUri);
				using var webResponse = (HttpWebResponse) webRequest.GetResponse();

				// File size of the download
				BytesReceived = ExistingLength;
				TotalBytesToReceive = webResponse.ContentLength;

				// Calculate remaining length
				var remaining = TotalBytesToReceive - ExistingLength;
				if (remaining > 0)
				{
					// Only download if something is remaining
					var request = (HttpWebRequest) WebRequest.Create(remoteUri);
					request.Proxy = null;
					request.AddRange(ExistingLength);

					using var response = (HttpWebResponse) request.GetResponse();
					var downloadResumable = (int) response.StatusCode == 206;

					using var fileStream = fileInfo.Open(downloadResumable ? FileMode.Append : FileMode.Create, FileAccess.Write);
					using var webStream = response.GetResponseStream();

					int byteSize;
					var buffer = new byte[4096];
					var sw = Stopwatch.StartNew();

					CurrentSpeed = 0;

					while (!_cancellationToken.IsCancellationRequested && ((byteSize = webStream.Read(buffer, 0, buffer.Length)) > 0))
					{
						fileStream.Write(buffer, 0, byteSize);

						BytesReceived += byteSize;

						if (sw.ElapsedMilliseconds >= 1000)
						{
							CurrentSpeed = (long) (((BytesReceived - ExistingLength) * 1000) / sw.Elapsed.TotalMilliseconds);
						}

						progressChanged?.Invoke();
					}

					sw.Stop();
				}
				else
				{
					// File is already downloaded
					progressChanged?.Invoke();
				}

				return new FileDownloaderCompletedEventArgs { Cancelled = _cancellationToken.IsCancellationRequested };
			}
			catch (WebException ex)
			{
				return new FileDownloaderCompletedEventArgs { HasError = true, ErrorMessage = ex.Message };
			}
		}

		/// <summary>
		/// Start a file download.
		/// </summary>
		/// <param name="remoteUri"> The remote URI of the file. </param>
		/// <param name="localFilePath"> The local file path. </param>
		/// <param name="force"> Optional force to overwrite local file. </param>
		public void StartDownload(string remoteUri, string localFilePath, bool force)
		{
			_cancellationToken = new CancellationTokenSource();

			FileDownloaderCompletedEventArgs args = null;

			Task.Run(() =>
				{
					args = Download(remoteUri, localFilePath, force, OnProgressChanged, () => _cancellationToken.IsCancellationRequested);
					OnCompleted(args);
				})
				.ContinueWith(x =>
				{
					if (x.IsFaulted && (x.Exception != null))
					{
						args ??= new FileDownloaderCompletedEventArgs();
						args.HasError = true;
						args.ErrorMessage = x.Exception.InnerException?.Message ?? x.Exception.Message;

						OnCompleted(new FileDownloaderCompletedEventArgs
						{
							HasError = true,
							ErrorMessage = x.Exception.InnerException?.Message ?? x.Exception.Message,
							Cancelled = _cancellationToken?.IsCancellationRequested ?? false
						});
					}

					_cancellationToken?.Dispose();
					_cancellationToken = null;
				});
		}

		/// <summary>
		/// Called when the file download is completed.
		/// </summary>
		/// <param name="args"> The file downloaded completed details. </param>
		protected virtual void OnCompleted(FileDownloaderCompletedEventArgs args)
		{
			Completed?.Invoke(this, args);
		}

		/// <summary>
		/// Called when the file download progress changes.
		/// </summary>
		protected virtual void OnProgressChanged()
		{
			ProgressChanged?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#region Events

		/// <summary>
		/// Event for when the file download is completed.
		/// </summary>
		public event EventHandler<FileDownloaderCompletedEventArgs> Completed;

		/// <summary>
		/// Event for when the file download progress changes.
		/// </summary>
		public event EventHandler ProgressChanged;

		#endregion
	}
}