#region References

using System.Linq;
using Newtonsoft.Json;

#endregion

namespace Speedy.Extensions
{
	/// <summary>
	/// Extensions for Newtonsoft JSON.net
	/// </summary>
	public static class NewtonsoftExtensions
	{
		#region Methods

		/// <summary>
		/// Locates the first item of the provided type.
		/// </summary>
		/// <typeparam name="T"> The type of the JsonConverter. </typeparam>
		/// <param name="settings"> The JSON serializer settings. </param>
		/// <returns> The converter of provided type or otherwise default type. </returns>
		public static T GetConverter<T>(this JsonSerializerSettings settings) where T : JsonConverter
		{
			var type = typeof(T);
			return (T) settings.Converters.FirstOrDefault(t => t.GetType() == type);
		}

		#endregion
	}
}