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
				Id = default(int),
				Description = Guid.NewGuid().ToString(),
				Name = Guid.NewGuid().ToString()
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}