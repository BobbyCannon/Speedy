#region References

using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;
using Speedy.Application.Maui;

#endregion

// ReSharper disable once CheckNamespace
namespace Speedy.Samples.Maui;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		MauiPlatform.Initialize(this);
		base.OnCreate(savedInstanceState);
	}
}