namespace Speedy;

/// <summary>
/// Represents an object that subscribes to events.
/// </summary>
public interface IEventSubscriber
{
	#region Methods

	/// <summary>
	/// Cleanup all subscribed events.
	/// </summary>
	void CleanupEventSubscriptions();

	#endregion
}