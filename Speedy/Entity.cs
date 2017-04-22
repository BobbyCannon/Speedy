using System;

namespace Speedy
{
	/// <summary>
	/// Represents a Speedy entity.
	/// </summary>
	public abstract class Entity<T> : IEntity
	{
		#region Properties

		public abstract T Id { get; set; }

		#endregion

		#region Methods

		public virtual void EntityAdded()
		{
		}

		public virtual void EntityDeleted()
		{
		}

		public virtual void EntityModified()
		{
		}

		public virtual bool TrySetId(string id)
		{
			try
			{
				Id = id.FromJson<T>();
				return true;
			}
			catch
			{
				return false;
			}
		}

		public virtual bool IdIsSet()
		{
			return !Equals(Id, default(T));
		}

		public virtual T NewId(ref T currentKey)
		{
			return default(T);
		}

		#endregion
	}

	public interface IEntity
	{
		#region Methods

		void EntityAdded();

		void EntityDeleted();

		void EntityModified();

		bool TrySetId(string id);

		#endregion
	}
}