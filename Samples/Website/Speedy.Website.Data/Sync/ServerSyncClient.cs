﻿#region References

using System;
using System.Linq;
using System.Security.Authentication;
using Speedy.Data;
using Speedy.Data.SyncApi;
using Speedy.Exceptions;
using Speedy.Extensions;
using Speedy.Net;
using Speedy.Sync;
using Speedy.Website.Data.Entities;
using Speedy.Website.Data.Enumerations;

#endregion

namespace Speedy.Website.Data.Sync
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
			Options.IsServerClient = true;

			// Allows allow primary key caching on the server.
			Options.EnablePrimaryKeyCache = true;

			IncomingConverter = GetIncomingConverter(account);
			OutgoingConverter = _outgoingConverter;
		}

		static ServerSyncClient()
		{
			_accountAssemblyName = typeof(Account).ToAssemblyName();
			_outgoingConverter = GetOutgoingConverter();
		}

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
		public override SyncSession BeginSync(Guid id, SyncOptions untrustedOptions)
		{
			// note: never trust the sync options. These are just suggestions from the client, you MUST ensure these suggestions are valid.
			var syncOptions = new SyncOptions
			{
				IncludeIssueDetails = _account.InRole(AccountRole.Administrator),
				ItemsPerSyncRequest = untrustedOptions.ItemsPerSyncRequest > 600 ? 600 : untrustedOptions.ItemsPerSyncRequest,
				// Do not allow clients to permanently delete entities
				PermanentDeletions = false,
				LastSyncedOnClient = untrustedOptions.LastSyncedOnClient,
				LastSyncedOnServer = untrustedOptions.LastSyncedOnServer
			};

			// Detect the type of sync requested
			var syncKey = untrustedOptions.GetSyncType(SyncType.All);

			switch (syncKey)
			{
				case SyncType.Account:
				{
					if (untrustedOptions.Values.ContainsKey(ClientSyncManager.AccountValueKey))
					{
						// We want to sync a single account
						Guid.TryParse(untrustedOptions.Values[ClientSyncManager.AccountValueKey], out var accountSyncId);
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
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<AccountEntity>());
					break;
				}
				case SyncType.Address:
				{
					if (untrustedOptions.Values.ContainsKey(ClientSyncManager.AddressValueKey))
					{
						// We want to sync a single address
						Guid.TryParse(untrustedOptions.Values[ClientSyncManager.AddressValueKey], out var addressSyncId);
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
					syncOptions.AddSyncableFilter(new SyncRepositoryFilter<SettingEntity>(null, null,
						// This filter below replaces lookup for "SyncId"
						x => y => y.Name == x.Name)
					);
					break;
				}
			}

			// Do not allow clients to permanently delete entities
			syncOptions.PermanentDeletions = false;

			return base.BeginSync(id, syncOptions);
		}

		public static SyncClientIncomingConverter GetIncomingConverter(AccountEntity account)
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
								return true;
							}
							case SyncObjectStatus.Deleted:
							{
								// Do not allow deletes unless you are administrator
								var canDelete = account.InRole(AccountRole.Administrator);
								if (!canDelete)
								{
									// force an update to roll back client changes
									entity.ModifiedOn = TimeService.UtcNow;
								}

								return canDelete;
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
								if (entity.Id == 0)
								{
									// Server is building a deleted item, so process all values.
									processUpdate();
								}
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
								return true;
							}
							case SyncObjectStatus.Modified:
							case SyncObjectStatus.Deleted:
							default:
							{
								// force an update to roll back client changes
								entity.ModifiedOn = TimeService.UtcNow;

								// Also throw an exception to add a sync issue to the response
								throw new UpdateException("You cannot modify or delete log entries.");
							}
						}
					}),
				new SyncObjectIncomingConverter<Setting, long, SettingEntity, long>(null,
					(update, entity, processUpdate, type) =>
					{
						switch (type)
						{
							case SyncObjectStatus.Added:
							{
								processUpdate();
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
								if (entity.Name == "cannot delete")
								{
									// force an update to roll back client changes
									entity.ModifiedOn = TimeService.UtcNow;

									// Also throw an exception to add a sync issue to the response
									throw new UpdateException("You cannot delete this setting.");
								}

								return true;
							}
						}
					})
			);
		}

		public static SyncClientOutgoingConverter GetOutgoingConverter()
		{
			return new SyncClientOutgoingConverter(
				new SyncObjectOutgoingConverter<AccountEntity, int, Account, int>((x, y) => { y.Roles = x.Roles == null ? null : AccountEntity.SplitRoles(x.Roles); }),
				new SyncObjectOutgoingConverter<AddressEntity, long, Address, long>(),
				new SyncObjectOutgoingConverter<LogEventEntity, long, LogEvent, long>(),
				new SyncObjectOutgoingConverter<SettingEntity, long, Setting, long>()
			);
		}

		public void ValidateAccount(AccountEntity accountEntity)
		{
			if (accountEntity.SyncId != _account.SyncId)
			{
				throw new AuthenticationException(Constants.Unauthorized);
			}
		}

		#endregion
	}
}