#region References

using System;

#endregion

namespace Speedy.Website.Samples.Entities
{
	public class PetEntity : Entity<(string Name, int OwnerId)>, IModifiableEntity
	{
		#region Properties

		/// <inheritdoc />
		public DateTime CreatedOn { get; set; }

		public override (string Name, int OwnerId) Id
		{
			get => (Name, Owner?.Id ?? OwnerId);
			set
			{
				Name = value.Name;
				OwnerId = value.OwnerId;
			}
		}

		/// <inheritdoc />
		public DateTime ModifiedOn { get; set; }

		public string Name { get; set; }

		public virtual AccountEntity Owner { get; set; }

		public int OwnerId { get; set; }

		public virtual PetTypeEntity Type { get; set; }

		public string TypeId { get; set; }

		#endregion
	}
}