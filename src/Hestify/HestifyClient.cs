using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Hestify.Helpers;
using Newtonsoft.Json;

[assembly:InternalsVisibleTo("Hestify.Test")]
namespace Hestify
{
    public class HestifyClient : IDisposable
	{
		public readonly HttpClient Client;

		public HestifyClient(HttpClient client, string baseAddress)
		{
			Client = client;
			client.BaseAddress = new Uri(baseAddress);
		}

		public HestifyClient(HttpClient client)
		{
			Client = client;
		}

		public HestifyClient(string address) : this(new HttpClient {BaseAddress = new Uri(address)})
		{
		}

		public HestifyClient(Uri baseAddress) : this(new HttpClient {BaseAddress = baseAddress})
		{
		}

		public HestifyClient() : this(new HttpClient())
		{
		}

		private IEnumerable<Action<RequestMessageOptions>> MessageBuilders { get; set; } = Enumerable.Empty<Action<RequestMessageOptions>>();


		private IEnumerable<Action<HttpResponseMessage>> PostProcessor { get; set; } = Enumerable.Empty<Action<HttpResponseMessage>>();
		

		public HestifyClient WithHeader(string key, string value)
		{
			return WithRequestBuilder(message => message.Headers.Add(key, value));
		}

		public HestifyClient WithHeader(HttpRequestHeader key, string value)
		{
			return WithRequestBuilder(message => message.Headers.Add(key.ToString(), value));
		}

		public HestifyClient WithBearerToken(string token)
		{
			return WithAuthorization("Bearer", token);
		}

		public HestifyClient WithBasicToken(string token)
		{
			return WithAuthorization("Basic", token);
		}

		public HestifyClient WithAuthorization(string scheme, string token)
		{
			Checks.IsNotNull(scheme);
			Checks.IsNotNull(token);
			return WithHeader(HttpRequestHeader.Authorization, $"{scheme} {token}");
		}
		
		public HestifyClient WithJsonContent<T>(T content, string mediaType = "application/json") where T : class
		{
			return WithContent(new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, mediaType));
		}

		public HestifyClient WithXmlContent<T>(T content, string mediaType = "application/xml")
		{
			var type = typeof(T);
			if (type.FullName!.Contains("AnonymousType") &&
			    type.GetCustomAttributes<CompilerGeneratedAttribute>().Any())
				throw new ArgumentException("content type must not be anonymous type.");

			var xmlSerializer = new XmlSerializer(type);
			using var memoryStream = new MemoryStream();
			using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
			xmlSerializer.Serialize(streamWriter, content);
			var body = Encoding.UTF8.GetString(memoryStream.ToArray());
			return WithContent(new StringContent(body, Encoding.UTF8, mediaType));
		}

		public HestifyClient WithFormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> pairs)
		{
			return WithContent(new FormUrlEncodedContent(pairs));
		}

		public HestifyClient WithFileContent(string filePath, string name = "file")
		{
			var fileInfo = new FileInfo(filePath);
			return WithFileContent(fileInfo, name);
		}
		
		public HestifyClient WithFileContent(Stream stream, string name)
		{
			return WithFileContent(new StreamContent(stream), name);
		}

		public HestifyClient WithFileContent(Stream stream, string name, string filename)
		{
			return WithFileContent(new StreamContent(stream), name, filename);
		}
		
		public HestifyClient WithFileContent(FileInfo fileInfo, string name)
		{
			return WithFileContent(new StreamContent(fileInfo.OpenRead()), name, fileInfo.Name);
		}
		
		public HestifyClient WithFileContent(StreamContent stream, string name)
		{
			return WithContent(new MultipartFormDataContent {{stream, name}});
		}
		
		public HestifyClient WithFileContent(StreamContent stream, string name, string filename)
		{
			return WithContent(new MultipartFormDataContent {{stream, name, filename}});
		}

		public HestifyClient WithContent(HttpContent content, MediaTypeHeaderValue value = null)
		{
			return WithRequestMessageBuilder(message =>
			{
				if (message.Content != default)
					throw new InvalidOperationException("HttpContent is already set. Request can have only one http content.");
				
				message.Content = content;
				if (value != null) 
					message.Content!.Headers.ContentType = value;
			});
		}

		public HestifyClient WithQuery(string key, string value)
		{
			return WithRequestBuilder(message => message.Query[key] = value);
		}

		public HestifyClient WithQuery(params (string key, string value)[] parameters)
		{
			return WithRequestBuilder(message =>
			{
				foreach (var (key, value) in parameters)
					message.Query[key] = value;
			});
		}

		public HestifyClient WithRelativePath(string path)
		{
			var uri = new Uri(path, UriKind.RelativeOrAbsolute);
			if (uri.IsAbsoluteUri)
				throw new InvalidOperationException("path is only allow relative uri path.");
			
			return WithRequestBuilder(options =>
			{
				if (path.StartsWith("/"))
					options.RelativePaths.Clear();
				
				options.RelativePaths.Add(path);
			});
		}

		public HestifyClient WithBaseUri(string baseAddress)
		{
			return WithUri(new Uri(baseAddress, UriKind.Absolute));
		}

		public HestifyClient WithUri(Uri uri)
		{
			return uri.IsAbsoluteUri
				? WithRequestBuilder(options => options.Uri = uri)
				: WithRelativePath(uri.OriginalString);
		}

		public HestifyClient WithUri(string uri)
		{
			return WithUri(new Uri(uri, UriKind.RelativeOrAbsolute));
		}

		public HestifyClient WithEnsureSuccessCode()
		{
			return WithResponseProcessor(response => response.EnsureSuccessStatusCode());
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

		private async Task<HttpResponseMessage> SendAsync(HttpMethod method)
		{
			using var message = BuildRequest()  ;
			message.Method = method;
			var response = await Client.SendAsync(message);

			foreach (var action in PostProcessor)
				action(response);
			
			return response;
		}

		public HestifyClient MultipartForm(FileStream fileStream, string name = null)
		{
			return WithMultipartForm(fileStream, name, Path.GetFileName(fileStream.Name));
		}

		public HestifyClient WithMultipartForm(Stream stream, string name = null, string filename = null)
		{
			return WithRequestBuilder(message =>
			{
				switch (message.ContentBuilder)
				{
					case MultipartFormDataContentBuilder content:
						content.Add(new StreamContent(stream), name, filename);
						break;
					case null:
						var builder = new MultipartFormDataContentBuilder();
						if (name == null && filename == null)
							builder.Add(new StreamContent(stream));
						else if (filename == null)
							builder.Add(new StreamContent(stream), name);
						else 
							builder.Add(new StreamContent(stream), name ?? filename, filename);
						
						message.ContentBuilder = builder;
						break;
					default:
						throw new InvalidOperationException(
							"Couldn't add multipart form content if there is content of a different than MultipartFormDataContent");
				}
			});
		}


		public HttpRequestMessage BuildRequest()
		{
			var message = new RequestMessageOptions();
			foreach (var messageBuilder in MessageBuilders)
				messageBuilder(message);
			return message.BuildMessage();
		}

		public HestifyClient WithRequestMessageBuilder(Action<HttpRequestMessage> action)
		{
			return new(Client)
			{
				MessageBuilders = MessageBuilders.WithOne(options => action(options.Message)),
				PostProcessor = PostProcessor
			};
		}


		public HestifyClient WithRequestBuilder(Action<RequestMessageOptions> action)
		{
			return new(Client)
			{
				MessageBuilders = MessageBuilders.WithOne(action),
				PostProcessor = PostProcessor
			};
		}

		public HestifyClient WithResponseProcessor(Action<HttpResponseMessage> action)
		{
			return new(Client)
			{
				MessageBuilders = MessageBuilders,
				PostProcessor = PostProcessor.WithOne(action)
			};
		}
			

		public void Dispose()
		{
			Client?.Dispose();
		}
	}
}