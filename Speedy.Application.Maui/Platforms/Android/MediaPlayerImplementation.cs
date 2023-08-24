#region References

using Speedy.Extensions;

#endregion

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

	#region Methods

	public override void Play(string path)
	{
		_player.Stop();
		_player.Reset();
		_player.SetDataSource(path);
		_player.Prepare();
		_player.Start();
	}

	public void SetVolume(int volume)
	{
		var percent = volume.EnsureRange(0, 100) / 100.0f;
		_player.SetVolume(percent, percent);
	}

	/// <inheritdoc />
	public override async void Speak(string message)
	{
		await TextToSpeech.Default.SpeakAsync(message);
	}

	#endregion
}