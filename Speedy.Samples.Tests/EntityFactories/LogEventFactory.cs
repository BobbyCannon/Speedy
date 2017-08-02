#region References

using System;
using System.Diagnostics.CodeAnalysis;
using Speedy.Samples.Entities;

#endregion

namespace Speedy.Samples.Tests.EntityFactories
{
	[ExcludeFromCodeCoverage]
	public class LogEventFactory
	{
		#region Methods

		public static LogEvent Get(Action<LogEvent> update = null)
		{
			var result = new LogEvent
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