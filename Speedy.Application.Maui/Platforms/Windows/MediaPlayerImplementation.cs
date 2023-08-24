#region References

using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.UI.Xaml.Controls;
using Speedy.Extensions;

#endregion

// ReSharper disable once CheckNamespace

namespace Speedy.Application.Maui;

/// <inheritdoc />
public class MediaPlayerImplementation : MediaPlayer
{
	#region Fields

	private readonly MediaPlayerElement _player;

	#endregion

	#region Constructors

	public MediaPlayerImplementation()
	{
		_player = new MediaPlayerElement
		{
			AreTransportControlsEnabled = false,
			AutoPlay = false
		};
	}

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsPlaying => _player.MediaPlayer.CurrentState == MediaPlayerState.Playing;

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void Play(string path)
	{
		_player.Source = MediaSource.CreateFromUri(new Uri(path));
		_player.MediaPlayer.Position = TimeSpan.Zero;
		_player.MediaPlayer.Play();
		_player.MediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
	}

	public void SetVolume(int volume)
	{
		_player.MediaPlayer.Volume = volume.EnsureRange(0, 100) / 100.0;
	}

	/// <inheritdoc />
	public override async void Speak(string message)
	{
		await TextToSpeech.Default.SpeakAsync(message);
	}

	private void MediaPlayerOnMediaEnded(Windows.Media.Playback.MediaPlayer sender, object args)
	{
		MauiDispatcher
			.Instance?
			.Run(() =>
			{
				_player.Source = null;
				_player.MediaPlayer.MediaEnded -= MediaPlayerOnMediaEnded;
			});
	}

	#endregion
}