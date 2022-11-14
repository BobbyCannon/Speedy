#region References

using System;

#endregion

namespace Speedy.Data;

/// <summary>
/// Represents a basic comparer.
/// </summary>
/// <typeparam name="T"> The type to be compared. </typeparam>
public class BasicComparer<T> : Comparer<T> where T : new()
{
	#region Fields

	private readonly Func<T, T, (T, bool)> _update;

	private readonly Func<T, T, bool> _validate;

	#endregion

	#region Constructors

	/// <summary>
	/// Instantiates on instance of a basic comparer.
	/// </summary>
	/// <param name="validate"> The validate method. </param>
	/// <param name="update"> The update method. </param>
	public BasicComparer(Func<T, T, bool> validate, Func<T, T, (T, bool)> update)
		: this(validate, update, null)
	{
	}

	/// <summary>
	/// Instantiates on instance of a basic comparer.
	/// </summary>
	/// <param name="validate"> The validate method. </param>
	/// <param name="update"> The update method. </param>
	/// <param name="dispatcher"> An optional dispatcher. </param>
	public BasicComparer(Func<T, T, bool> validate, Func<T, T, (T, bool)> update, IDispatcher dispatcher) : base(dispatcher)
	{
		_validate = validate;
		_update = update;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override bool ShouldApplyUpdate(T value, T update)
	{
		return _validate.Invoke(value, update);
	}

	/// <inheritdoc />
	public override bool TryUpdateValue(ref T value, T update)
	{
		var result = _update.Invoke(value, update);
		if (result.Item2)
		{
			value = result.Item1;
		}

		return result.Item2;
	}

	#endregion
}