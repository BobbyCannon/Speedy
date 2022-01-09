#region References

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Speedy.Extensions;

#endregion

namespace Speedy.Mobile.Droid
{
	public class PermissionProvider
	{
		#region Fields

		private readonly string[] _activityPermissions =
		{
			Manifest.Permission.ActivityRecognition
		};

		private readonly string[] _bluetoothPermissions =
		{
			Manifest.Permission.Bluetooth,
			Manifest.Permission.BluetoothAdmin
		};

		private readonly string[] _cameraPermissions =
		{
			Manifest.Permission.Camera
		};

		private readonly string[] _locationPermissions =
		{
			Manifest.Permission.AccessCoarseLocation,
			Manifest.Permission.AccessFineLocation
		};

		private readonly Activity _mainActivity;

		private readonly string[] _microphonePermissions =
		{
			Manifest.Permission.RecordAudio
		};

		private readonly string[] _networkPermissions =
		{
			Manifest.Permission.AccessNetworkState,
			Manifest.Permission.AccessWifiState,
			Manifest.Permission.ChangeWifiState,
			Manifest.Permission.Internet
		};

		private static readonly string[] _storagePermissions =
		{
			Manifest.Permission.ReadExternalStorage,
			Manifest.Permission.WriteExternalStorage
		};

		#endregion

		#region Constructors

		public PermissionProvider(Activity activity)
		{
			_mainActivity = activity;

			// These are configurable so default to false
			IsActivityAvailable = false;
			IsBluetoothAvailable = false;
			IsCameraAvailable = false;
			IsLocationAvailable = false;
			IsMicrophoneAvailable = false;
			IsNetworkAvailable = false;
			IsStorageAvailable = false;

			if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
			{
				_locationPermissions = _locationPermissions
					.Append(Manifest.Permission.ForegroundService)
					.ToArray();
			}

			if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
			{
				_locationPermissions = _locationPermissions
					.Append(Manifest.Permission.AccessBackgroundLocation)
					.ToArray();
			}
			else
			{
				// Anything older than API 29 doesn't have activity permissions
				IsActivityAvailable = true;
			}
		}

		#endregion

		#region Properties

		public bool IsActivityAvailable { get; protected set; }

		public bool IsAllAvailable =>
			IsActivityAvailable
			&& IsBluetoothAvailable
			&& IsCameraAvailable
			&& IsLocationAvailable
			&& IsMicrophoneAvailable
			&& IsNetworkAvailable
			&& IsStorageAvailable;

		public bool IsBluetoothAvailable { get; protected set; }

		public bool IsCameraAvailable { get; protected set; }

		public bool IsLocationAvailable { get; protected set; }

		public bool IsMicrophoneAvailable { get; protected set; }

		public bool IsNetworkAvailable { get; protected set; }

		public bool IsStorageAvailable { get; protected set; }

		#endregion

		#region Methods

		public bool CheckPermissions(PermissionType permissionType)
		{
			var response = true;

			if (permissionType.HasFlag(PermissionType.Activity) && (Build.VERSION.SdkInt >= BuildVersionCodes.Q))
			{
				var isActivityAvailable = GetPermissionsStatus(_activityPermissions);
				if (isActivityAvailable != IsActivityAvailable)
				{
					IsActivityAvailable = isActivityAvailable;
					OnPermissionsChanged(PermissionType.Activity);
				}

				response &= isActivityAvailable;
			}

			if (permissionType.HasFlag(PermissionType.Bluetooth))
			{
				var isBluetoothAvailable = GetPermissionsStatus(_bluetoothPermissions);
				if (isBluetoothAvailable != IsBluetoothAvailable)
				{
					IsBluetoothAvailable = isBluetoothAvailable;
					OnPermissionsChanged(PermissionType.Bluetooth);
				}

				response &= isBluetoothAvailable;
			}

			if (permissionType.HasFlag(PermissionType.Camera))
			{
				var isCameraAvailable = GetPermissionsStatus(_cameraPermissions);
				if (isCameraAvailable != IsCameraAvailable)
				{
					IsCameraAvailable = isCameraAvailable;
					OnPermissionsChanged(PermissionType.Camera);
				}

				response &= isCameraAvailable;
			}

			if (permissionType.HasFlag(PermissionType.Location))
			{
				var isLocationAvailable = GetPermissionsStatus(_locationPermissions);
				if (isLocationAvailable != IsLocationAvailable)
				{
					IsLocationAvailable = isLocationAvailable;
					OnPermissionsChanged(PermissionType.Location);
				}

				response &= isLocationAvailable;
			}

			if (permissionType.HasFlag(PermissionType.Microphone))
			{
				var isMicrophoneAvailable = GetPermissionsStatus(_microphonePermissions);
				if (isMicrophoneAvailable != IsMicrophoneAvailable)
				{
					IsMicrophoneAvailable = isMicrophoneAvailable;
					OnPermissionsChanged(PermissionType.Microphone);
				}

				response &= isMicrophoneAvailable;
			}

			if (permissionType.HasFlag(PermissionType.Network))
			{
				var isNetworkAvailable = GetPermissionsStatus(_networkPermissions);
				if (isNetworkAvailable != IsNetworkAvailable)
				{
					IsNetworkAvailable = isNetworkAvailable;
					OnPermissionsChanged(PermissionType.Network);
				}

				response &= isNetworkAvailable;
			}

			if (permissionType.HasFlag(PermissionType.Storage))
			{
				var isStorageAvailable = GetPermissionsStatus(_storagePermissions);
				if (isStorageAvailable != IsStorageAvailable)
				{
					IsStorageAvailable = isStorageAvailable;
					OnPermissionsChanged(PermissionType.Storage);
				}

				response &= isStorageAvailable;
			}

			return response;
		}

		public Task<bool> CheckPermissionsAsync(PermissionType permissionType)
		{
			return Task.Run(() => CheckPermissions(permissionType));
		}

		public void RequestPermission(PermissionType permissionType)
		{
			var totalPermissions = new List<string>();
			var totalPermissionTypes = PermissionType.None;

			if (permissionType.HasFlag(PermissionType.Activity) && (Build.VERSION.SdkInt >= BuildVersionCodes.Q))
			{
				totalPermissions.AddRange(_activityPermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Activity);
			}

			if (permissionType.HasFlag(PermissionType.Bluetooth))
			{
				totalPermissions.AddRange(_bluetoothPermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Bluetooth);
			}

			if (permissionType.HasFlag(PermissionType.Camera))
			{
				totalPermissions.AddRange(_cameraPermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Camera);
			}

			if (permissionType.HasFlag(PermissionType.Location))
			{
				totalPermissions.AddRange(_locationPermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Location);
			}

			if (permissionType.HasFlag(PermissionType.Microphone))
			{
				totalPermissions.AddRange(_microphonePermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Microphone);
			}

			if (permissionType.HasFlag(PermissionType.Network))
			{
				totalPermissions.AddRange(_networkPermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Network);
			}

			if (permissionType.HasFlag(PermissionType.Storage))
			{
				totalPermissions.AddRange(_storagePermissions);
				totalPermissionTypes = totalPermissionTypes.SetFlag(PermissionType.Storage);
			}

			CheckAndRequestPermissions(totalPermissions.ToArray(), totalPermissionTypes);
		}

		protected virtual void OnPermissionsChanged(PermissionType permissionType)
		{
			PermissionsChanged?.Invoke(this, permissionType);
		}

		private void CheckAndRequestPermissions(string[] permissions, PermissionType permissionType)
		{
			foreach (var permission in permissions)
			{
				if (_mainActivity.CheckSelfPermission(permission) != Permission.Granted)
				{
					_mainActivity.RequestPermissions(permissions, (int) permissionType);
				}
			}

			// have to see if the requests were successful
			// note: RequestPermissions is not blocking so this will check permission before getting user answers
			CheckPermissions(permissionType);
		}

		private bool GetPermissionsStatus(IEnumerable<string> permissions)
		{
			// Leave this as a simple foreach so we can debug any permission issues

			// ReSharper disable once LoopCanBeConvertedToQuery
			foreach (var permission in permissions)
			{
				if (_mainActivity.CheckSelfPermission(permission) != Permission.Granted)
				{
					return false;
				}
			}

			return true;
		}

		#endregion

		#region Events

		public event EventHandler<PermissionType> PermissionsChanged;

		#endregion
	}
}