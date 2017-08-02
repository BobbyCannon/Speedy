#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class GroupMemberFactory
	{
		#region Methods

		public static GroupMember Get(Action<GroupMember> update = null)
		{
			var result = new GroupMember
			{
				Id = default(int),
				Group = GroupFactory.Get(),
				GroupSyncId = default(Guid),
				Member = PersonFactory.Get(),
				MemberSyncId = default(Guid),
				Role = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}