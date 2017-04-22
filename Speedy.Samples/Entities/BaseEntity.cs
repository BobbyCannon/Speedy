#region References

using System.Threading;

#endregion

namespace Speedy.Samples.Entities
{
	public abstract class BaseEntity : Entity<int>
	{
		#region Methods

		public override int NewId(ref int currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	public abstract class BaseCreatedEntity : CreatedEntity<int>
	{
		#region Methods

		public override int NewId(ref int currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	public abstract class BaseModifiableEntity : ModifiableEntity<int>
	{
		#region Methods

		public override int NewId(ref int currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}
}