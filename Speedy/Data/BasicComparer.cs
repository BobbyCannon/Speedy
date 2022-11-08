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
	{
		_validate = validate;
		_update = update;
	}

	#endregion

	#region Methods

	/// <summary>
	/// Validates an update to see if it should be applied.
	/// </summary>
	/// <param name="update"> The update to validate. </param>
	/// <returns> True if the update should be applied. </returns>
	public override bool ValidateUpdate(T update)
	{
		return _validate.Invoke(Value, update);
	}

	/// <summary>
	/// Try to update the value.
	/// </summary>
	/// <param name="update"> The update to apply. </param>
	/// <returns> True if the value was updated otherwise false. </returns>
	protected override bool TryUpdateValue(T update)
	{
		var result = _update.Invoke(Value, update);
		if (result.Item2)
		{
			Value = result.Item1;
		}

		return result.Item2;
	}

	#endregion
}