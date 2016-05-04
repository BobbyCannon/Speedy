#region References

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Newtonsoft.Json;

#endregion

namespace Speedy.Net
{
	/// <summary>
	/// Client used to get and post web data.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public static class WebClient
	{
		#region Methods

		/// <summary>
		/// Gets the string values from a URI.
		/// </summary>
		/// <param name="uri"> The URI to read from. </param>
		/// <param name="location"> The relative location to read from. </param>
		/// <param name="timeout"> The amount of time to wait (in milliseconds) before timing out. </param>
		/// <returns> The data that was read from the uri and location. </returns>
		public static string Get(string uri, string location, int timeout = 5000)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(uri);
				client.Timeout = TimeSpan.FromMilliseconds(timeout);
				client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

				var response = client.GetAsync(location).Result;
				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"{response.StatusCode} ({response.ReasonPhrase})");
				}

				return response.Content.ReadAsStringAsync().Result;
			}
		}

		/// <summary>
		/// Gets the deserialized typed value from a URI.
		/// </summary>
		/// <typeparam name="T"> The type to deserialize into. </typeparam>
		/// <param name="uri"> The URI to read from. </param>
		/// <param name="location"> The relative location to read from. </param>
		/// <param name="timeout"> The amount of time to wait (in milliseconds) before timing out. </param>
		/// <returns> The data that was read from the URI and location. </returns>
		public static T Get<T>(string uri, string location, int timeout = 5000)
		{
			var result = Get(uri, location, timeout);
			return JsonConvert.DeserializeObject<T>(result);
		}

		/// <summary>
		/// Serializes the content then post the data to the URI.
		/// </summary>
		/// <typeparam name="T"> The type to serialize the response to. </typeparam>
		/// <param name="uri"> The URI to send to. </param>
		/// <param name="location"> The relative location to send to. </param>
		/// <param name="content"> The to serialize and send. </param>
		/// <param name="timeout"> The amount of time to wait (in milliseconds) before timing out. </param>
		/// <returns> The response for the post. </returns>
		public static void Post<T>(string uri, string location, T content = null, int timeout = 5000) where T : class
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(uri);
				client.Timeout = TimeSpan.FromMilliseconds(timeout);

				HttpResponseMessage response;
				using (HttpContent httpContent = new StringContent((content ?? new object()).ToJson(ignoreVirtuals: true)))
				{
					httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
					response = client.PostAsync(location, httpContent).Result;
				}

				if (!response.IsSuccessStatusCode)
				{
					throw new Exception($"{response.StatusCode} ({response.ReasonPhrase})");
				}
			}
		}

		/// <summary>
		/// Serializes the content then post the data to the URI. The response is deserialized into the provided type.
		/// </summary>
		/// <typeparam name="T1"> The type to serialize and send. </typeparam>
		/// <typeparam name="T2"> The type to serialize the response to. </typeparam>
		/// <param name="uri"> The URI to send to. </param>
		/// <param name="location"> The relative location to send to. </param>
		/// <param name="content"> The to serialize and send. </param>
		/// <param name="timeout"> The amount of time to wait (in milliseconds) before timing out. </param>
		/// <returns> The deserialized response of the post. </returns>
		public static T2 Post<T1, T2>(string uri, string location, T1 content, int timeout = 5000)
		{
			using (var client = new HttpClient())
			{
				client.BaseAddress = new Uri(uri);
				client.Timeout = TimeSpan.FromMilliseconds(timeout);

				HttpResponseMessage response;
				var json = content.ToJson(ignoreVirtuals: true);

				using (HttpContent httpContent = new StringContent(json))
				{
					httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
					response = client.PostAsync(location, httpContent).Result;
				}

				if (!response.IsSuccessStatusCode)
				{
					if (response.StatusCode == HttpStatusCode.Conflict)
					{
						var responseContent = response.Content.ReadAsStringAsync().Result;
						if (responseContent.Contains("You may only perform this action") && responseContent.Contains(" per second."))
						{
							Thread.Sleep(250);
							return Post<T1, T2>(uri, location, content, timeout);
						}
					}

					throw new Exception($"{response.StatusCode} ({response.ReasonPhrase})");
				}

				var result = response.Content.ReadAsStringAsync().Result;
				return JsonConvert.DeserializeObject<T2>(result);
			}
		}

		#endregion
	}
}