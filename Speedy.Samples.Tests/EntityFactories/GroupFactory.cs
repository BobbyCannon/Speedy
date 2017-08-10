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

		public static Group Get(Action<Group> update = null)
		{
			var result = new Group
			{
				Description = Guid.NewGuid().ToString(),
				Id = default(int),
				Name = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}