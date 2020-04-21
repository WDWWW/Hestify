using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Hestify
{
    public class HestifyClient
	{
		public delegate Task<HestifyClient> Interceptor(HestifyClient client);

		private readonly string _relativeUri;

		public readonly HttpClient Client;
		private Interceptor _interceptor;

		public HestifyClient(HttpClient client, string relativeUri)
		{
			Client = client;
			_relativeUri = relativeUri;
			_interceptor = Task.FromResult;
		}

		private IDictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();

		private NameValueCollection RequestParams { get; set; } = new NameValueCollection();

		private HttpContent Content { get; set; }

		public HestifyClient WithHeader(string key, string value)
		{
			var clone = Clone();
			clone.RequestHeaders.Add(key, value);
			return clone;
		}

		public HestifyClient WithHeader(HttpRequestHeader key, string value)
		{
			var clone = Clone();
			clone.RequestHeaders.Add(key.ToString(), value);
			return clone;
		}

		public HestifyClient AddInterceptor(Interceptor interceptor)
		{
			var origin = _interceptor;
			_interceptor = async client => await interceptor(await origin(client));
			return this;
		}

		public HestifyClient WithBearerToken(string token)
		{
			return WithHeader("Authorization", "Bearer " + token);
		}

		public HestifyClient WithJsonBody<T>(T content) where T : class
		{
			if (Content != null)
				throw new ArgumentException("Content is already set. WithJsonBody is not allow multiple times.");

			var clone = Clone();
			clone.Content = new JsonContent(content);;
			return clone;
		}

		public HestifyClient WithParam(string key, string value)
		{
			var clone = Clone();
			clone.RequestParams.Add(key, value);
			return clone;
		}

		public HestifyClient WithParams(params (string key, string value)[] parameters)
		{
			var clone = Clone();
			foreach (var (key, value) in parameters) clone.RequestParams.Add(key, value);
			return clone;
		}

		public HestifyClient WithPagination(int index, int size)
		{
			return WithParams(("index", index.ToString()), ("size", size.ToString()));
		}

		public async Task<HttpResponseMessage> GetAsync()
		{
			return await SendAsync(HttpMethod.Get);
		}

		public async Task<HttpResponseMessage> PostAsync()
		{
			return await SendAsync(HttpMethod.Post);
		}

		public async Task<HttpResponseMessage> DeleteAsync()
		{
			return await SendAsync(HttpMethod.Delete);
		}

		public async Task<HttpResponseMessage> PutAsync()
		{
			return await SendAsync(HttpMethod.Put);
		}

		public async Task<HttpResponseMessage> PatchAsync()
		{
			return await SendAsync(HttpMethod.Patch);
		}

		private async Task<HttpResponseMessage> SendAsync(HttpMethod httpMethod)
		{
			var client = await _interceptor(this);
			return await Client.SendAsync(client.HttpRequestMessage(httpMethod));
		}

		public HestifyClient WithMultipartForm(FileStream fileStream, string name = null)
		{
			return WithMultipartForm(fileStream, name, Path.GetFileName(fileStream.Name));
		}

		public HestifyClient WithMultipartForm(Stream stream, string name = null, string filename = null)
		{
			Content ??= new MultipartFormDataContent();

			if (!(Content is MultipartFormDataContent content))
				throw new ArgumentException("Can't be with multipart content. Content was set other content type.");
			
			var clone = Clone();
			content.Add(new StreamContent(stream), name, filename);
			return clone;

		}

		private HttpRequestMessage HttpRequestMessage(HttpMethod httpMethod)
		{
			var httpRequestMessage = new HttpRequestMessage
			{
				Method = httpMethod,
				Content = Content,
				RequestUri = new UriBuilder(Client.BaseAddress)
				{
					Path = _relativeUri,
					Query = ToQueryString()
				}.Uri
			};

			foreach (var (key, value) in RequestHeaders) httpRequestMessage.Headers.Add(key, value);

			return httpRequestMessage;
		}

		private string ToQueryString()
		{
			var query = HttpUtility.ParseQueryString(string.Empty);

			for (var i = 0; i < RequestParams.Count; i++)
			{
				foreach (var value in RequestParams.GetValues(i))
				{
					query[RequestParams.GetKey(i)] = value;
				}
			}

			return query.ToString();
		}

		private HestifyClient Clone()
		{
			var clone = new HestifyClient(Client, _relativeUri)
			{
				RequestHeaders = new Dictionary<string, string>(RequestHeaders),
				RequestParams = new NameValueCollection(RequestParams),
				Content = Content,
				_interceptor = _interceptor
			};
			return clone;
		}
	}
}