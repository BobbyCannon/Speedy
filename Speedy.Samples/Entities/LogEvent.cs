#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy;

#endregion

namespace Speedy.Samples.Entities
{
	public class LogEvent : CreatedEntity<string>, ILogEvent
	{
		#region Properties

		public override string Id { get; set; }
		public string Message { get; set; }

		#endregion
	}
}