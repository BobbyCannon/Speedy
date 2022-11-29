#region References

using System.Dynamic;
using System.Linq;
using System.Reflection;
using Speedy.Extensions;

#endregion

namespace Speedy;

/// <summary>
/// Making any call to an objects members thread safe.
/// </summary>
public class ThreadSafe<T> : ThreadSafe
{
	#region Constructors

	/// <inheritdoc />
	public ThreadSafe(T instance) : base(instance)
	{
	}

	#endregion
}

/// <summary>
/// Making any call to an objects members thread safe.
/// </summary>
public class ThreadSafe : DynamicObject
{
	#region Fields

	private static readonly object _doubleLock, _floatLock;
	private readonly object _lock;
	private readonly object _object;
	private readonly TypeInfo _typeInfo;

	#endregion

	#region Constructors

	/// <summary>
	/// Initializes a new instance of the <see cref="ThreadSafe" /> class.
	/// </summary>
	/// <param name="instance"> The wrapped object whose operations will be made thread-safe. </param>
	public ThreadSafe(object instance)
	{
		_object = instance;
		_typeInfo = instance.GetType().GetTypeInfo();
		_lock = new object();
	}

	static ThreadSafe()
	{
		_doubleLock = new object();
		_floatLock = new object();
	}

	#endregion

	#region Methods

	/// <summary>
	/// Decrement an float by a value or float.Epsilon if not provided.
	/// </summary>
	/// <param name="value"> The value to be decremented. </param>
	/// <param name="decrease"> An optional decrease. The value defaults to the smallest possible value. </param>
	/// <returns> The incremented value. </returns>
	public static void Decrement(ref float value, float decrease = float.Epsilon)
	{
		lock (_doubleLock)
		{
			value = value.Decrement(decrease);
		}
	}

	/// <summary>
	/// Decrement an double by a value or double.Epsilon if not provided.
	/// </summary>
	/// <param name="value"> The value to be decremented. </param>
	/// <param name="decrease"> An optional decrease. The value defaults to the smallest possible value. </param>
	/// <returns> The incremented value. </returns>
	public static void Decrement(ref double value, double decrease = double.Epsilon)
	{
		lock (_doubleLock)
		{
			value = value.Decrement(decrease);
		}
	}

	/// <summary>
	/// Increment an double by a value or double.Epsilon if not provided.
	/// </summary>
	/// <param name="value"> The value to be incremented. </param>
	/// <param name="increase"> An optional increase. The value defaults to the smallest possible value. </param>
	/// <returns> The incremented value. </returns>
	public static void Increment(ref double value, double increase = double.Epsilon)
	{
		lock (_doubleLock)
		{
			value = value.Increment(increase);
		}
	}

	/// <summary>
	/// Increment an float by a value or float.Epsilon if not provided.
	/// </summary>
	/// <param name="value"> The value to be incremented. </param>
	/// <param name="increase"> An optional increase. The value defaults to the smallest possible value. </param>
	/// <returns> The incremented value. </returns>
	public static void Increment(ref float value, float increase = float.Epsilon)
	{
		lock (_floatLock)
		{
			value = value.Increment(increase);
		}
	}

	/// <inheritdoc />
	public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
	{
		var prop = GetIndexedProperty();
		if (prop != null)
		{
			lock (_lock)
			{
				result = prop.GetValue(_object, indexes);
			}

			return true;
		}

		result = null;
		return false;
	}

	/// <inheritdoc />
	public override bool TryGetMember(GetMemberBinder binder, out object result)
	{
		var prop = _typeInfo.GetDeclaredProperty(binder.Name);
		if (prop != null)
		{
			lock (_lock)
			{
				result = prop.GetValue(_object);
			}

			return true;
		}

		var field = _typeInfo.GetDeclaredField(binder.Name);
		if (field != null)
		{
			lock (_lock)
			{
				result = field.GetValue(_object);
			}

			return true;
		}

		result = null;
		return false;
	}

	/// <inheritdoc />
	public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
	{
		var method = _typeInfo.GetDeclaredMethod(binder.Name);
		if (method != null)
		{
			lock (_lock)
			{
				result = method.Invoke(_object, args);
			}

			return true;
		}

		result = null;
		return false;
	}

	/// <inheritdoc />
	public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
	{
		var prop = GetIndexedProperty();
		if (prop != null)
		{
			lock (_lock)
			{
				prop.SetValue(_object, value, indexes);
			}

			return true;
		}

		return false;
	}

	/// <inheritdoc />
	public override bool TrySetMember(SetMemberBinder binder, object value)
	{
		var prop = _typeInfo.GetDeclaredProperty(binder.Name);
		if (prop != null)
		{
			lock (_lock)
			{
				prop.SetValue(_object, value);
			}

			return true;
		}

		var field = _typeInfo.GetDeclaredField(binder.Name);
		if (field != null)
		{
			lock (_lock)
			{
				field.SetValue(_object, value);
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Attempts to find the indexer property.
	/// </summary>
	/// <returns>
	/// The <see cref="PropertyInfo" /> for the indexer property, if found;
	/// otherwise, <see langword="null" />.
	/// </returns>
	private PropertyInfo GetIndexedProperty()
	{
		// TODO: Is there a better way to do this?

		var prop = _typeInfo.GetDeclaredProperty("Item");
		if ((prop == null) || (prop.GetIndexParameters().Length == 0))
		{
			prop = _typeInfo.DeclaredProperties.FirstOrDefault(p => p.GetIndexParameters().Length != 0);
		}

		return prop;
	}

	#endregion
}