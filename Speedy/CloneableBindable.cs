#region References

using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Represents a bindable object.
/// </summary>
public abstract class CloneableBindable<T, T2> : CloneableBindable<T>, ICloneable<T2>
	where T : T2, new()
{
	#region Constructors

	/// <summary>
	/// Instantiates a bindable object.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected CloneableBindable(IDispatcher dispatcher = null) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	T2 ICloneable<T2>.DeepClone(int? maxDepth)
	{
		return DeepClone(maxDepth);
	}

	/// <inheritdoc />
	T2 ICloneable<T2>.ShallowClone()
	{
		return DeepClone();
	}

	#endregion
}

/// <summary>
/// Represents a bindable object.
/// </summary>
public abstract class CloneableBindable<T> : Bindable<T>, ICloneable<T>
	where T : new()
{
	#region Constructors

	/// <summary>
	/// Instantiates a bindable object.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	protected CloneableBindable(IDispatcher dispatcher = null) : base(dispatcher)
	{
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public virtual T DeepClone(int? maxDepth = null)
	{
		var response = new T();

		switch (response)
		{
			case Entity entity:
			{
				entity.UpdateWith(this, false, false, false);
				break;
			}
			case IUpdatable<T> updatable:
			{
				updatable.UpdateWith(this);
				break;
			}
			case IUpdatable updatable:
			{
				updatable.UpdateWith(this);
				break;
			}
			default:
			{
				response.UpdateWithUsingReflection(this);
				break;
			}
		}

		return response;
	}

	/// <inheritdoc />
	public virtual T ShallowClone()
	{
		return DeepClone();
	}

	/// <inheritdoc />
	object ICloneable.DeepClone(int? maxDepth)
	{
		return DeepClone(maxDepth);
	}

	/// <inheritdoc />
	object ICloneable.ShallowClone()
	{
		return ShallowClone();
	}

	#endregion
}