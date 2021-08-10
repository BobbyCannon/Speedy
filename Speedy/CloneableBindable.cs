#region References

using System;
using Speedy.Extensions;
using Speedy.Serialization;
using ICloneable = Speedy.Serialization.ICloneable;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class CloneableBindable<T> : Bindable, ICloneable where T : new()
	{
		#region Constructors

		/// <summary>
		/// Instantiates a bindable object.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected CloneableBindable(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public virtual object DeepClone(int? maxDepth = null)
		{
			return (T) this.DeepCloneObject(maxDepth);
		}

		/// <inheritdoc />
		public virtual object ShallowClone()
		{
			var test = Activator.CreateInstance<T>();
			if (test is Entity entity)
			{
				entity.UpdateWith(this, false, false, false);
			}
			else
			{
				test.UpdateWithUsingReflection(this);
			}
			return test;
		}

		#endregion
	}
}