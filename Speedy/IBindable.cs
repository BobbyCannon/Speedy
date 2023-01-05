#region References

using System.ComponentModel;
using System.Runtime.CompilerServices;

#endregion

namespace Speedy;

/// <summary>
/// Represents a bindable object.
/// </summary>
public interface IBindable<in T> : IBindable, IUpdatable<T>
{
}

/// <summary>
/// Represents a bindable object.
/// </summary>
public interface IBindable : INotifyPropertyChanged
{
	#region Methods

	/// <summary>
	/// Get the current dispatcher in use.
	/// </summary>
	/// <returns>
	/// The dispatcher that is currently being used. Null if no dispatcher is assigned.
	/// </returns>
	IDispatcher GetDispatcher();

	/// <summary>
	/// Return true if the change notifications are paused or otherwise false.
	/// </summary>
	bool IsChangeNotificationsPaused();

	/// <summary>
	/// Indicates the property has changed on the bindable object.
	/// </summary>
	/// <param name="propertyName"> The name of the property has changed. </param>
	void OnPropertyChanged([CallerMemberName] string propertyName = null);

	/// <summary>
	/// Pause / Un-pause the property change notifications
	/// </summary>
	void PausePropertyChangeNotifications(bool pause = true);

	/// <summary>
	/// Updates the entity for this entity.
	/// </summary>
	/// <param name="dispatcher"> The optional dispatcher to use. </param>
	void UpdateDispatcher(IDispatcher dispatcher);

	#endregion
}