namespace Speedy.Application;

/// <summary>
/// Represents a media player.
/// </summary>
public abstract class MediaPlayer
{
	#region Properties

	/// <summary>
	/// Returns true if the media player is playing.
	/// </summary>
	public abstract bool IsPlaying { get; }

	#endregion

	#region Methods

	/// <summary>
	/// Plays a file.
	/// </summary>
	/// <param name="path"> The path to the file to play. </param>
	public abstract void Play(string path);

	/// <summary>
	/// Speak out a text phrase.
	/// </summary>
	/// <param name="message"> The message to speak. </param>
	public abstract void Speak(string message);

	#endregion
}