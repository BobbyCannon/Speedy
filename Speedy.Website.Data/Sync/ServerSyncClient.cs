#region References

using System;
using System.Linq;
using System.Security.Authentication;
using Speedy.Data;
using Speedy.Data.WebApi;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;
using Speedy.Website.Samples.Entities;
using Speedy.Website.Samples.Enumerations;

#endregion

namespace Speedy.Website.Samples.Sync
{
	public class ServerSyncClient : SyncClient
	{
		#region Constants

		private const string SyncAccountChangedKey = "SyncAccountChanged";

		#endregion

		#region Fields

		private readonly AccountEntity _account;
		private static readonly string _accountAssemblyName;
		private static readonly SyncClientOutgoingConverter _outgoingConverter;

		#endregion

		#region Constructors

		public ServerSyncClient(AccountEntity account, ISyncableDatabaseProvider<IContosoDatabase> provider)
			: base("Contoso Sync Client", provider)
		{
			_account = account;

			// Ensure the server is maintaining the modified on, without this data could not sync properly
			// Clients should never modify the "ModifiedOn" during sync but the Server must always set it
			Options.MaintainModifiedOn = true;

			// Allows allow primary key caching on the server.
			Options.EnablePrimaryKeyCache = true;
			Options.PrimaryKeyCacheTimeout = TimeSpan.FromMinutes(1);

			IncomingConverter = GetIncomingFilter();
			OutgoingConverter = _outgoingConverter;
		}

		static ServerSyncClient()
		{
			MinimumSyncSystemSupported = new Version(0, 0, 0, 0);
			MaximumSyncSystemSupported = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);

			_accountAssemblyName = typeof(Account).ToAssemblyName();
			_outgoingConverter = new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<AccountEntity, int, Account, int>(),
				new SyncObjectOutgoingConverter<AddressEntity, long, Address, long>(),
				new SyncObjectOutgoingConverter<LogEventEntity, long, LogEvent, long>()
			);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Maximum sync system supported.
		/// </summary>
		public static Version MaximumSyncSystemSupported { get; set; }

		/// <summary>
		/// Minimum sync system supported.
		/// </summary>
		public static Version MinimumSyncSystemSupported { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override ServiceResult<SyncIssue> ApplyChanges(Guid id, ServiceRequest<SyncObject> changes)
		{
			// Must define the incoming filter for each sync because we need the current authenticated user for processing
			SyncOptions.Values.AddOrUpdate(SyncAccountChangedKey, changes.Collection.Any(x => x.TypeName == _accountAssemblyName).ToString());
			return base.ApplyChanges(id, changes);
		}

		/// <inheritdoc />
		public override ServiceResult<SyncIssue> ApplyCorrections(Guid id, ServiceRequest<SyncObject> corrections)
		{
			// Must define the incoming filter for each sync because we need the current authenticated user for processing
			SyncOptions.Values.AddOrUpdate(SyncAccountChangedKey, corrections.Collection.Any(x => x.TypeName == _accountAssemblyName).ToString());
			return base.ApplyCorrections(id, corrections);
		}

		/// <inheritdoc />
		public override SyncSession BeginSync(Guid id, SyncOptions options)
		{
			var validSyncVersion = options.Values.TryGetValue(SyncOptions.SyncVersionKey, out var versionString)
				| Version.TryParse(versionString ?? string.Empty, out var version)
				| (version >= MinimumSyncSystemSupported)
				| (version <= MaximumSyncSystemSupported);

			if (!validSyncVersion)
			{
				throw new SpeedyException(Constants.SyncClientInvalid);
			}

			// note: never trust the sync options. These are just suggestions from the client, you MUST ensure these suggestions are valid.
			var syncOptions = new SyncOptions
			{
				IncludeIssueDetails = _account.InRole(AccountRole.Administrator),
				ItemsPerSyncRequest = options.ItemsPerSyncRequest > 300 ? 300 : options.ItemsPerSyncRequest,
				PermanentDeletions = false,
				LastSyncedOnClient = options.LastSyncedOnClient,
				LastSyncedOnServer = options.LastSyncedOnServer
			};

			// Detect the type of sync requested
			var syncKey = options.GetSyncType(SyncType.All);

			switch (syncKey)
			{
				case SyncType.Account:
				{
					if (options.Values.ContainsKey(ClientSyncManager.AccountValueKey))
					{
						// We want to sync a single account
						Guid.TryParse(options.Values[ClientSyncManager.AccountValueKey], out var accountSyncId);
						syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>(x => x.SyncId == accountSyncId));
					}
					else
					{
						throw new InvalidOperationException(Constants.InvalidSyncOptions);
					}
					break;
				}
				case SyncType.Accounts:
				{
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					break;
				}
				case SyncType.Address:
				{
					if (options.Values.ContainsKey(ClientSyncManager.AddressValueKey))
					{
						// We want to sync a single address
						Guid.TryParse(options.Values[ClientSyncManager.AddressValueKey], out var addressSyncId);
						syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>(x => x.SyncId == addressSyncId));
					}
					else
					{
						throw new InvalidOperationException(Constants.InvalidSyncOptions);
					}
					break;
				}
				case SyncType.Addresses:
				{
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					break;
				}
				case SyncType.LogEvents:
				{
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<LogEventEntity>());
					break;
				}
				case SyncType.All:
				default:
				{
					// Default to sync all items (addresses, accounts)
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AddressEntity>());
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>());
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<LogEventEntity>());
					break;
				}
			}

			// Do not allow clients to permanently delete entities
			syncOptions.PermanentDeletions = false;

			return base.BeginSync(id, syncOptions);
		}

		public void ValidateAccount(AccountEntity accountEntity)
		{
			if (accountEntity.SyncId != _account.SyncId)
			{
				throw new AuthenticationException(Constants.Unauthorized);
			}
		}

		private SyncClientIncomingConverter GetIncomingFilter()
		{
			return new SyncClientIncomingConverter(
				new SyncObjectIncomingConverter<Account, int, AccountEntity, int>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
								entity.SyncId = update.SyncId == Guid.Empty ? Guid.NewGuid() : update.SyncId;
								return true;
							}
							case SyncObjectStatus.Deleted:
							{
								// Do not allow deletes unless you are administrator
								return _account.InRole(AccountRole.Administrator);
							}
							default:
							{
								processUpdate();
								return true;
							}
						}
					}),
				new SyncObjectIncomingConverter<Address, long, AddressEntity, long>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
								entity.SyncId = update.SyncId == Guid.Empty ? Guid.NewGuid() : update.SyncId;
								return true;
							}
							case SyncObjectStatus.Modified:
							{
								processUpdate();
								return true;
							}
							case SyncObjectStatus.Deleted:
							default:
							{
								return true;
							}
						}
					}),
				new SyncObjectIncomingConverter<LogEvent, long, LogEventEntity, long>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
								entity.SyncId = update.SyncId == Guid.Empty ? Guid.NewGuid() : update.SyncId;
								return true;
							}
							case SyncObjectStatus.Modified:
							case SyncObjectStatus.Deleted:
							default:
							{
								return false;
							}
						}
					})
			);
		}

		#endregion
	}
}