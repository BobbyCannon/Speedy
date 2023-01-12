// ReSharper disable once CheckNamespace

namespace Speedy.Application.Maui;

/// <summary>
/// https://github.com/jfversluis/Plugin.Maui.Audio/blob/main/src/Plugin.Maui.Audio/AudioPlayer.android.cs
/// </summary>
public class MediaPlayerImplementation : MediaPlayer
{
	#region Fields

	private readonly Android.Media.MediaPlayer _player;

	#endregion

	#region Constructors

	protected MediaPlayerImplementation()
	{
		_player = new Android.Media.MediaPlayer();
	}

	#endregion

	#region Properties

	public override bool IsPlaying => _player.IsPlaying;

	#endregion
}