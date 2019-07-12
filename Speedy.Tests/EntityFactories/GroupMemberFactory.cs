#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberFactory
	{
		#region Methods

		public static GroupMemberEntity Get(Action<GroupMemberEntity> update = null)
		{
			var result = new GroupMemberEntity
			{
				Group = GroupFactory.Get(),
				GroupSyncId = default,
				Id = default,
				Member = PersonFactory.Get(),
				MemberSyncId = default,
				Role = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}