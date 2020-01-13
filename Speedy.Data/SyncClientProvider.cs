#region References

using System;
using System.Net;
using Speedy.Sync;

#endregion

namespace Speedy.Data
{
	public class SyncClientProvider
	{
		#region Fields

		private readonly Func<string, NetworkCredential, ISyncClient> _getClient;

		#endregion

		#region Constructors

		public SyncClientProvider(Func<string, NetworkCredential, ISyncClient> getClient)
		{
			_getClient = getClient;
		}

		#endregion

		#region Methods

		public ISyncClient GetClient(string name, NetworkCredential credential)
		{
			return _getClient.Invoke(name, credential);
		}

		#endregion
	}
}