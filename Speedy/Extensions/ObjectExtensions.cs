#region References

using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Speedy.Serialization;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for the object.
/// </summary>
public static class ObjectExtensions
{
	#region Methods

	/// <summary>
	/// Executes a provided action if the test is successful.
	/// </summary>
	/// <param name="test"> The test to determine action to take. </param>
	/// <param name="action1"> The action to perform if the test is true. </param>
	/// <param name="action2"> The action to perform if the test is false. </param>
	public static void IfThenElse(Func<bool> test, Action action1, Action action2)
	{
		if (test())
		{
			action1();
		}
		else
		{
			action2();
		}
	}

	/// <summary>
	/// Remove all event handlers from the provided value.
	/// </summary>
	/// <param name="value"> The value to remove all event handlers. </param>
	public static void RemoveEventHandlers(this object value)
	{
		if (value == null)
		{
			return;
		}

		var flags = BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
		var valueType = value.GetType();
		var eventFields = valueType.GetCachedEventFields(flags);
		EventHandlerList staticEventHandlers = null;

		void removeEventHandler(EventInfo info, Delegate subscriber)
		{
			var privateRemoveMethod = info.GetRemoveMethod(true);
			privateRemoveMethod.Invoke(value, flags, null, new object[] { subscriber }, CultureInfo.CurrentCulture);
		}

		foreach (var eventField in eventFields)
		{
			// After hours and hours of research and trial and error, it turns out that
			// STATIC Events have to be treated differently from INSTANCE Events...
			if (eventField.IsStatic)
			{
				if (staticEventHandlers == null)
				{
					var mi = valueType.GetCachedMethod("get_Events", flags);
					staticEventHandlers = (EventHandlerList) mi.Invoke(value, new object[] { });
				}

				var idx = eventField.GetValue(value);
				var eventHandler = staticEventHandlers[idx];
				var invocationList = eventHandler?.GetInvocationList();

				if (invocationList == null)
				{
					continue;
				}

				var eventInfo = valueType.GetEvent(eventField.Name, flags);

				if (eventInfo == null)
				{
					continue;
				}

				foreach (var subscriber in invocationList)
				{
					removeEventHandler(eventInfo, subscriber);
				}
			}
			else
			{
				var eventInfo = valueType.GetEvent(eventField.Name, flags);
				if (eventInfo == null)
				{
					continue;
				}

				var eventFieldValue = eventField.GetValue(value);
				if (eventFieldValue is not Delegate eventHandler)
				{
					continue;
				}

				foreach (var subscriber in eventHandler.GetInvocationList())
				{
					removeEventHandler(eventInfo, subscriber);
				}
			}
		}
	}

	/// <summary>
	/// Global shallow clone. If the object is ICloneable then the interface implementation will be used.
	/// </summary>
	/// <typeparam name="T"> The type of the object </typeparam>
	/// <param name="value"> </param>
	/// <returns> </returns>
	public static T ShallowClone<T>(this T value)
	{
		return value switch
		{
			ICloneable<T> cloneable => cloneable.ShallowClone(),
			ICloneable cloneable => (T) cloneable.ShallowClone(),
			_ => value.DeepClone(1)
		};
	}
	
	/// <summary>
	/// Global shallow clone. If the object is ICloneable then the interface implementation will be used.
	/// </summary>
	/// <param name="value"> </param>
	/// <returns> </returns>
	public static object ShallowClone(this object value)
	{
		return value is ICloneable cloneable
			? cloneable.ShallowClone()
			: value.DeepClone(1);
	}

	#endregion
}