// ReSharper disable once CheckNamespace

namespace Speedy.Application.Maui;

// https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/src/Plugin.Maui.Audio/AudioPlayer.android.cs

public class MediaPlayerImplementation : MediaPlayer
{
	#region Fields

	private readonly Android.Media.MediaPlayer player;

	#endregion

	#region Constructors

	protected MediaPlayerImplementation()
	{
		player = new Android.Media.MediaPlayer();
	}

	#endregion

	#region Properties

	public override bool IsPlaying => player.IsPlaying;

	#endregion
}