#region References

using System.Speech.Synthesis;
using NAudio.Wave;
using Speedy.Extensions;

#endregion

namespace Speedy.Application.Wpf;

/// <inheritdoc />
public class WpfMediaPlayer : MediaPlayer
{
	#region Fields

	private AudioFileReader _audioFile;
	private WaveOutEvent _outputDevice;

	#endregion

	#region Properties

	/// <inheritdoc />
	public override bool IsPlaying => _audioFile != null;

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void Play(string path)
	{
		_outputDevice?.Stop();
		_outputDevice?.Dispose();
		_audioFile?.Dispose();

		_audioFile = new AudioFileReader(path);
		_outputDevice = new WaveOutEvent();
		_outputDevice.Init(_audioFile);
		_outputDevice.Play();
	}

	/// <summary>
	/// Set the volume of the output device.
	/// </summary>
	/// <param name="volume"> The volume to be set. </param>
	public void SetVolume(int volume)
	{
		if (_outputDevice != null)
		{
			_outputDevice.Volume = volume.EnsureRange(0, 100) / 100.0f;
		}
	}

	/// <inheritdoc />
	public override void Speak(string message)
	{
		using var s = new SpeechSynthesizer();
		s.Speak(message);
	}

	#endregion
}