#region References

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Speedy;

#endregion

namespace Speedy.Samples.Entities
{
	public class PetType : Entity<string>
	{
		#region Constructors

		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		public PetType()
		{
			Types = new List<Pet>();
		}

		#endregion

		#region Properties

		public override string Id { get; set; }
		public string Type { get; set; }
		public virtual ICollection<Pet> Types { get; set; }

		#endregion
	}
}