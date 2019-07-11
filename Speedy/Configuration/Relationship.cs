#region References

using System;
using System.Reflection;

#endregion

namespace Speedy.Configuration
{
	internal class Relationship
	{
		#region Properties

		public PropertyInfo IdPropertyInfo { get; set; }
		public Guid? SyncId { get; set; }
		public Type Type { get; set; }
		public PropertyInfo TypeIdPropertyInfo { get; internal set; }

		#endregion
	}
}