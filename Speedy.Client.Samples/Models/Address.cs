#region References

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy.Sync;

#endregion

namespace Speedy.Client.Samples.Models
{
	public class Address : SyncModel<long>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public Address()
		{
			People = new List<Person>();
		}

		#endregion

		#region Properties

		public string City { get; set; }

		public override long Id { get; set; }

		public string Line1 { get; set; }

		public string Line2 { get; set; }

		public virtual ICollection<Person> People { get; set; }

		public string Postal { get; set; }

		public string State { get; set; }

		#endregion
	}
}