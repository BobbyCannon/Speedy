namespace Speedy.Application.Maui;

// All the code in this file is included in all platforms.
public class NotificationService : INotificationService
{
	#region Methods

	/// <inheritdoc />
	public void Notify(string message)
	{
		#if ANDROID
		#elif IOS
		#else
			// do nothing...
		#endif
	}

	#endregion
}

public interface INotificationService
{
	#region Methods

	void Notify(string message);

	#endregion
}