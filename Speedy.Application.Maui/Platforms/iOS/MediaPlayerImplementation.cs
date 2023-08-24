// ReSharper disable once CheckNamespace

namespace Speedy.Application.Maui;

/// <inheritdoc />
public class MediaPlayerImplementation : MediaPlayer
{
	#region Properties

	/// <inheritdoc />
	public override bool IsPlaying { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void Play(string path)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public override void Speak(string message)
	{
		throw new NotImplementedException();
	}

	#endregion
}