#region References

using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;

#endregion

namespace Speedy.Mobile.Droid
{
	[Service]
	public class MobileForegroundService : Service
	{
		#region Constants

		public const string ActionStartService = "Speedy.action.START_SERVICE";
		public const string ActionStopService = "Speedy.action.STOP_SERVICE";
		public const string ChannelId = "SpeedyService";
		public const int ServiceRunningNotificationId = 10000;
		public const string ServiceStartedKey = "HasSpeedyServiceServiceBeenStarted";

		#endregion

		#region Fields

		private readonly Intent _startServiceIntent;
		private readonly Intent _stopServiceIntent;

		#endregion

		#region Constructors

		public MobileForegroundService()
		{
			// Set intents for use in button events
			_startServiceIntent = new Intent(Application.Context, typeof(MobileForegroundService));
			_startServiceIntent.SetAction(ActionStartService);

			_stopServiceIntent = new Intent(Application.Context, typeof(MobileForegroundService));
			_stopServiceIntent.SetAction(ActionStopService);

			IsStarted = false;
		}

		#endregion

		#region Properties

		public bool IsStarted { get; private set; }

		#endregion

		#region Methods

		public override IBinder OnBind(Intent intent)
		{
			return null;
		}

		public override void OnDestroy()
		{
			// Remove the notification from the status bar.
			var notificationManager = (NotificationManager) GetSystemService(NotificationService);
			notificationManager?.Cancel(ServiceRunningNotificationId);

			IsStarted = false;

			base.OnDestroy();
		}

		public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
		{
			try
			{
				var notification = CreateNotification();
				if (notification != null)
				{
					StartForeground(ServiceRunningNotificationId, notification);
					IsStarted = true;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(ex.Message);
			}

			// This tells Android not to restart the service if it is killed to reclaim resources.
			return StartCommandResult.Sticky;
		}

		public void StartService()
		{
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				// On android 8.0 and higher
				Application.Context.StartForegroundService(_startServiceIntent);
			}
			else
			{
				// Prior to android 8.0
				Application.Context.StartService(_startServiceIntent);
			}

			IsStarted = true;
		}

		public void StopService()
		{
			Application.Context.StopService(_stopServiceIntent);
			IsStarted = false;
		}

		private Bitmap BuildIconBitmap()
		{
			return BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon);
		}

		private Notification CreateNotification()
		{
			if (Resources == null)
			{
				return null;
			}

			// Building intent
			var intent = new Intent(Application.Context, typeof(MainActivity));
			intent.AddFlags(ActivityFlags.SingleTop);
			intent.PutExtra("Title", "Message");

			var pendingIntent = PendingIntent.GetActivity(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

			var builder = new Notification.Builder(Application.Context, ChannelId)
				.SetContentTitle(Resources.GetString(Resource.String.app_name))
				.SetContentText(Resources.GetString(Resource.String.notification_text))
				.SetSmallIcon(Resource.Drawable.icon)
				.SetLargeIcon(BuildIconBitmap())
				.SetColorized(true)
				.SetColor(Resource.Color.background_material_dark)
				.SetOngoing(true)
				.SetContentIntent(pendingIntent);

			// Building channel if API version is 26 or above
			if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
			{
				var notificationChannel = new NotificationChannel(ChannelId, "Title", NotificationImportance.High);
				notificationChannel.Importance = NotificationImportance.High;
				notificationChannel.EnableLights(true);
				notificationChannel.EnableVibration(true);
				notificationChannel.SetShowBadge(true);
				notificationChannel.SetVibrationPattern(new long[] { 100, 200, 300, 400, 500, 400, 300, 200, 400 });

				if (Application.Context.GetSystemService(NotificationService) is NotificationManager notificationManager)
				{
					builder.SetChannelId(ChannelId);
					notificationManager.CreateNotificationChannel(notificationChannel);
				}
			}

			return builder.Build();
		}

		#endregion
	}
}