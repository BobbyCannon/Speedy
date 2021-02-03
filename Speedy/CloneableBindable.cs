#region References

using Speedy.Extensions;
using Speedy.Serialization;

#endregion

namespace Speedy
{
	/// <summary>
	/// Represents a bindable object.
	/// </summary>
	public abstract class CloneableBindable<T> : Bindable<T>, ICloneable<T> where T : new()
	{
		#region Fields

		// ReSharper disable once StaticMemberInGenericType
		private static readonly SerializerSettings _defaultDeepCloneSettings;

		#endregion

		#region Constructors

		/// <summary>
		/// Instantiates a bindable object.
		/// </summary>
		/// <param name="dispatcher"> The dispatcher to update with. </param>
		protected CloneableBindable(IDispatcher dispatcher = null) : base(dispatcher)
		{
		}

		static CloneableBindable()
		{
			_defaultDeepCloneSettings = new SerializerSettings { IgnoreVirtuals = true, IgnoreReadOnly = true };
		}

		#endregion

		#region Methods

		/// <inheritdoc />
		public virtual T DeepClone(int levels = -1)
		{
			// Defaults to serializer deep clone. Type would want to override this to make it more efficient.
			return this.ToJson(_defaultDeepCloneSettings).FromJson<T>();
		}

		/// <inheritdoc />
		public virtual T ShallowClone()
		{
			var response = new T();
			response.UpdateWith(this);
			return response;
		}

		/// <inheritdoc />
		public virtual T ShallowCloneExcept(params string[] exclusions)
		{
			var response = new T();
			response.UpdateWith(this, exclusions);
			return response;
		}

		object ICloneable.DeepClone(int levels)
		{
			return DeepClone(levels);
		}

		object ICloneable.ShallowClone()
		{
			return ShallowClone();
		}

		object ICloneable.ShallowCloneExcept(params string[] exclusions)
		{
			return ShallowCloneExcept(exclusions);
		}

		#endregion
	}
}