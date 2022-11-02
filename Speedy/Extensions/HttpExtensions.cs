#region References

using System.Net;

#endregion

namespace Speedy.Extensions;

/// <summary>
/// Extensions for HTTP (ish)
/// </summary>
public static class HttpExtensions
{
	#region Methods

	/// <summary>
	/// Returns the string from a HttpWebResponse.
	/// </summary>
	/// <param name="response"> The response to read from. </param>
	/// <returns>
	/// The string from the response.
	/// </returns>
	public static string GetResponseString(this HttpWebResponse response)
	{
		using var stream = response.GetResponseStream();
		return stream.ReadString();
	}

	/// <summary>
	/// A value that indicates whether the HTTP response was successful.
	/// </summary>
	/// <param name="response"> The response to validate. </param>
	/// <returns>
	/// True if HttpStatusCode is in the successful range (200-299) otherwise false;
	/// </returns>
	public static bool IsSuccessStatusCode(this HttpWebResponse response)
	{
		var asInt = (int) response.StatusCode;
		return (asInt >= 200) && (asInt <= 299);
	}

	#endregion
}