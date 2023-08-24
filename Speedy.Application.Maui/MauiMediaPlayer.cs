#region References

#if !(WINDOWS || ANDROID || IOS)
using Speedy.Application.Inactive;
#endif

#endregion

namespace Speedy.Application.Maui;

public class MauiMediaPlayer
	#if (WINDOWS || ANDROID || IOS)
	: MediaPlayerImplementation
	#else
	: InactiveMediaPlayer
	#endif
{

}