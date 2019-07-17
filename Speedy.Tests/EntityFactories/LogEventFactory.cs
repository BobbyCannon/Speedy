#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;
using Speedy.Samples.Enumerations;

#endregion

namespace Speedy.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class LogEventFactory
	{
		#region Methods

		public static LogEventEntity Get(Action<LogEventEntity> update = null)
		{
			var time = TimeService.UtcNow;
			var result = new LogEventEntity
			{
				Id = Guid.NewGuid().ToString(),
				Message = null,
				Level = LogLevel.Information,
				CreatedOn = time,
				ModifiedOn = time
			};

			update?.Invoke(result);

			return result;
		}

		#endregion
	}
}