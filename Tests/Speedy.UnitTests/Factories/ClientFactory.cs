#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Data.Client;
using Speedy.Data.SyncApi;

#endregion

namespace Speedy.UnitTests.Factories
{
	[ExcludeFromCodeCoverage]
	public static class ClientFactory
	{
		#region Methods

		public static ClientAccount GetClientAccount(string name, ClientAddress address, Action<ClientAccount> update = null)
		{
			var result = new ClientAccount
			{
				Address = address,
				EmailAddress = name + "@speedy.local",
				Name = name,
				Roles = string.Empty,
				SyncId = Guid.NewGuid()
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static ClientAddress GetClientAddress(string line1 = null, string postal = null, string state = null, Action<ClientAddress> update = null)
		{
			var result = new ClientAddress
			{
				City = Guid.NewGuid().ToString(),
				Id = default,
				Line1 = line1 ?? Guid.NewGuid().ToString(),
				Line2 = Guid.NewGuid().ToString(),
				Postal = (postal ?? Guid.NewGuid().ToString()).Substring(25),
				State = (state ?? Guid.NewGuid().ToString()).Substring(25),
				SyncId = Guid.NewGuid()
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static ClientLogEvent GetLogEvent(string message, LogLevel level, Action<ClientLogEvent> update = null)
		{
			var result = new ClientLogEvent
			{
				Level = level,
				Message = message,
				SyncId = Guid.NewGuid()
			};

			update?.Invoke(result);
			result.ResetHasChanges();

			return result;
		}

		public static ClientSetting GetSetting(string name, string value)
		{
			return new ClientSetting
			{
				Name = name,
				Value = value
			};
		}

		#endregion
	}
}