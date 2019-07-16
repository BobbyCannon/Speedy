#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;
using Speedy.Website.Models;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberFactory
	{
		#region Methods

		public static GroupMemberEntity Get(GroupEntity group, PersonEntity person, Action<GroupMemberEntity> update = null)
		{
			var time = TimeService.UtcNow;

			var result = new GroupMemberEntity
			{
				Group = group,
				GroupId = group.Id,
				Id = default,
				Member = person,
				MemberSyncId = person.SyncId,
				Role = Guid.NewGuid().ToString(),
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}