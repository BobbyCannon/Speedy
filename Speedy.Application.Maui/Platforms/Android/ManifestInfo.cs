#region References

using Android;
using Android.App;

#endregion

[assembly: UsesPermission(Manifest.Permission.AccessCoarseLocation)]
[assembly: UsesPermission(Manifest.Permission.AccessFineLocation)]
//Required when targeting Android API 21+
[assembly: UsesFeature("android.hardware.location.gps")]
[assembly: UsesFeature("android.hardware.location.network")]