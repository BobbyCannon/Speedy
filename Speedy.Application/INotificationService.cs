namespace Speedy.Application;

/// <summary>
/// Represents a service for notifications.
/// </summary>
public interface INotificationService
{
	#region Methods

	/// <summary>
	/// Notify with the provided message.
	/// </summary>
	/// <param name="message"> The message. </param>
	void Notify(string message);

	#endregion
}