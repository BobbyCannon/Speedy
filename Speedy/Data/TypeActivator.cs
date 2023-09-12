#region References

using System;

#endregion

namespace Speedy.Data;

/// <summary>
/// Allows assigning an interface to a specific implementation.
/// </summary>
/// <typeparam name="T"> The interface type. </typeparam>
/// <typeparam name="T2"> The implementation type. </typeparam>
public class TypeActivator<T, T2> : TypeActivator where T2 : T
{
	#region Fields

	private readonly Func<object[], T2> _activator;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public TypeActivator() : this(null)
	{
	}

	/// <inheritdoc />
	public TypeActivator(Func<object[], T2> activator) : base(typeof(T))
	{
		_activator = activator;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override object CreateInstance(params object[] arguments)
	{
		return _activator != null
			? _activator.Invoke(arguments)
			: Activator.CreateInstanceInternal(false, typeof(T2), null, arguments);
	}

	#endregion
}

/// <summary>
/// Allows assigning an interface to a specific implementation.
/// </summary>
/// <typeparam name="T"> The interface type. </typeparam>
public class TypeActivator<T> : TypeActivator
{
	#region Fields

	private readonly Func<object[], T> _activator;

	#endregion

	#region Constructors

	/// <inheritdoc />
	public TypeActivator() : this(null)
	{
	}

	/// <summary>
	/// Create an instance of the type activator.
	/// </summary>
	/// <param name="activator"> The activator for the type. </param>
	public TypeActivator(Func<object[], T> activator) : base(typeof(T))
	{
		_activator = activator;
	}

	#endregion

	#region Methods

	/// <inheritdoc />
	public override object CreateInstance(params object[] arguments)
	{
		return _activator != null
			? _activator.Invoke(arguments)
			: Activator.CreateInstanceInternal(false, Type, null, arguments);
	}

	#endregion
}

/// <summary>
/// Allows assigning an interface to a specific implementation.
/// </summary>
public class TypeActivator
{
	#region Constructors

	/// <summary>
	/// Create an instance of the type activator.
	/// </summary>
	/// <param name="type"> The type this activator is for. </param>
	public TypeActivator(Type type)
	{
		Type = type;
	}

	#endregion

	#region Properties

	/// <summary>
	/// The type this activator is for.
	/// </summary>
	public Type Type { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Create an instance of the Type.
	/// </summary>
	/// <param name="arguments"> The value of the arguments. </param>
	/// <returns> The new instances of the type. </returns>
	public virtual object CreateInstance(params object[] arguments)
	{
		return Activator.CreateInstanceInternal(false, Type, null, arguments);
	}

	#endregion
}