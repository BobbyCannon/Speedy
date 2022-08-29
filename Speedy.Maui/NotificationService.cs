namespace Speedy.Maui
{
	// All the code in this file is included in all platforms.
	public class NotificationService : INotificationService
	{
		/// <inheritdoc />
		public void Notify(string message)
		{
			#if ANDROID
			
			#elif IOS
            
			#else
			// do nothing...
			#endif
		}
	}

	public interface INotificationService
	{
		void Notify(string message);
	}
}