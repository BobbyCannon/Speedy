#region References

using System;

#endregion

namespace Speedy.Samples.Entities
{
	public class Pet : ModifiableEntity<Pet.PetKey>
	{
		#region Properties

		public override PetKey Id
		{
			get => new PetKey { Name = Name, OwnerId = OwnerId };
			set
			{
				Name = Id.Name;
				OwnerId = Id.OwnerId;
			}
		}

		public string Name { get; set; }

		public virtual Person Owner { get; set; }

		public int OwnerId { get; set; }

		#endregion

		#region Methods

		/// <inheritdoc />
		public override bool IdIsSet()
		{
			return true;
		}

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

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}
				if (ReferenceEquals(this, obj))
				{
					return true;
				}
				if (obj.GetType() != GetType())
				{
					return false;
				}
				return Equals((PetKey) obj);
			}

			public override int GetHashCode()
			{
				return ((Name?.GetHashCode() ?? 0) * 397) ^ OwnerId;
			}

			#endregion
		}

		#endregion
	}
}