#region References

using System.Threading;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents an incrementing entity of type byte.
	/// </summary>
	public abstract class IncrementingTinyEntity : Entity<byte>
	{
		#region Fields

		private static readonly object _lock;

		#endregion

		#region Constructors

		static IncrementingTinyEntity()
		{
			_lock = new object();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override byte NewId(ref byte currentKey)
		{
			lock (_lock)
			{
				return ++currentKey;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type byte with created date.
	/// </summary>
	public abstract class IncrementingTinyCreatedEntity : CreatedEntity<byte>
	{
		#region Fields

		private static readonly object _lock;

		#endregion

		#region Constructors

		static IncrementingTinyCreatedEntity()
		{
			_lock = new object();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override byte NewId(ref byte currentKey)
		{
			lock (_lock)
			{
				return ++currentKey;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type byte with created and modified date.
	/// </summary>
	public abstract class IncrementingTinyModifiableEntity : ModifiableEntity<byte>
	{
		#region Fields

		private static readonly object _lock;

		#endregion

		#region Constructors

		static IncrementingTinyModifiableEntity()
		{
			_lock = new object();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override byte NewId(ref byte currentKey)
		{
			lock (_lock)
			{
				return ++currentKey;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type short.
	/// </summary>
	public abstract class IncrementingSmallEntity : Entity<short>
	{
		#region Fields

		private static readonly object _lock;

		#endregion

		#region Constructors

		static IncrementingSmallEntity()
		{
			_lock = new object();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override short NewId(ref short currentKey)
		{
			lock (_lock)
			{
				return ++currentKey;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type short with created date.
	/// </summary>
	public abstract class IncrementingSmallCreatedEntity : CreatedEntity<short>
	{
		#region Fields

		private static readonly object _lock;

		#endregion

		#region Constructors

		static IncrementingSmallCreatedEntity()
		{
			_lock = new object();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override short NewId(ref short currentKey)
		{
			lock (_lock)
			{
				return ++currentKey;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type short with created and modified date.
	/// </summary>
	public abstract class IncrementingSmallModifiableEntity : ModifiableEntity<short>
	{
		#region Fields

		private static readonly object _lock;

		#endregion

		#region Constructors

		static IncrementingSmallModifiableEntity()
		{
			_lock = new object();
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public override short NewId(ref short currentKey)
		{
			lock (_lock)
			{
				return ++currentKey;
			}
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type int.
	/// </summary>
	public abstract class IncrementingEntity : Entity<int>
	{
		#region Methods

		/// <inheritdoc />
		public override int NewId(ref int currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type int with created date.
	/// </summary>
	public abstract class IncrementingCreatedEntity : CreatedEntity<int>
	{
		#region Methods

		/// <inheritdoc />
		public override int NewId(ref int currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type int with created and modified date.
	/// </summary>
	public abstract class IncrementingModifiableEntity : ModifiableEntity<int>
	{
		#region Methods

		/// <inheritdoc />
		public override int NewId(ref int currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type long.
	/// </summary>
	public abstract class IncrementingLargeEntity : Entity<long>
	{
		#region Methods

		/// <inheritdoc />
		public override long NewId(ref long currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type long with created date.
	/// </summary>
	public abstract class IncrementingLargeCreatedEntity : CreatedEntity<long>
	{
		#region Methods

		/// <inheritdoc />
		public override long NewId(ref long currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}

	/// <summary>
	/// Represents an incrementing entity of type long with created and modified date.
	/// </summary>
	public abstract class IncrementingLargeModifiableEntity : ModifiableEntity<long>
	{
		#region Methods

		/// <inheritdoc />
		public override long NewId(ref long currentKey)
		{
			return Interlocked.Increment(ref currentKey);
		}

		#endregion
	}
}