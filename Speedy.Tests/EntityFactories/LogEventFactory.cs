#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class LogEventFactory
	{
		#region Methods

		public static LogEventEntity Get(Action<LogEventEntity> update = null)
		{
			var result = new LogEventEntity
			{
				Id = Guid.NewGuid().ToString(),
				Message = null
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}