#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class GroupFactory
	{
		#region Methods

		public static GroupEntity Get(Action<GroupEntity> update = null)
		{
			var result = new GroupEntity
			{
				Description = Guid.NewGuid().ToString(),
				Id = default,
				Name = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}