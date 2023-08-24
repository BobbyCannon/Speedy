namespace Speedy.Application.Inactive;

/// <inheritdoc />
public class InactiveMediaPlayer : MediaPlayer
{
	#region Properties

	/// <inheritdoc />
	public override bool IsPlaying { get; }

	#endregion

	#region Methods

	/// <inheritdoc />
	public override void Play(string path)
	{
	}

	/// <inheritdoc />
	public override void Speak(string message)
	{
	}

	#endregion
}