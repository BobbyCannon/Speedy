#region References

using System;

#endregion

namespace Speedy.Samples.Entities
{
	public class Pet : Entity<Pet.PetKey>
	{
		#region Properties

		public DateTime CreatedOn { get; set; }

		public override PetKey Id
		{
			get => new PetKey { Name = Name, OwnerId = OwnerId };
			set
			{
				Name = value.Name;
				OwnerId = value.OwnerId;
			}
		}

		public DateTime ModifiedOn { get; set; }
		public string Name { get; set; }
		public virtual Person Owner { get; set; }
		public int OwnerId { get; set; }
		public virtual PetType Type { get; set; }
		public string TypeId { get; set; }

		#endregion

		#region Classes

		public class PetKey : IEquatable<PetKey>
		{
			#region Properties

			public string Name { get; set; }
			public int OwnerId { get; set; }

			#endregion

			#region Methods

			public bool Equals(PetKey other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}

				if (ReferenceEquals(this, other))
				{
					return true;
				}

				return string.Equals(Name, other.Name) && OwnerId == other.OwnerId;
			}

			public override bool Equals(object other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}

				if (ReferenceEquals(this, other))
				{
					return true;
				}

				return other.GetType() == GetType() && Equals((PetKey) other);
			}

			public override int GetHashCode()
			{
				return new { Name, OwnerId }.GetHashCode();
			}

			#endregion
		}

		#endregion
	}
}