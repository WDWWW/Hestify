#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Hestify
{
    public class HestifyRequestMessage : HttpRequestMessage
    {
    }

    public class RequestMessageOptions
    {
        internal readonly HttpRequestMessage Message = new HestifyRequestMessage();
        
        public HttpRequestHeaders Headers => Message.Headers;

        public NameValueCollection Query = HttpUtility.ParseQueryString("");

        public Uri? Uri { get; set; }

        public IList<string> RelativePaths { get; set; } = new List<string> ();
        
        public IHttpContentBuilder? ContentBuilder { get; set; }
        
        internal HttpRequestMessage BuildMessage()
        {
            if (ContentBuilder != default)
                Message.Content = ContentBuilder.Build();

            var uri = Message.RequestUri ?? Uri ?? throw new CannotBuildMessageException("Should request message builder have any base or relative url to bulid request message.");
            if (!uri.IsAbsoluteUri)
                throw new InvalidOperationException("HttpRequestMessage or RequestMessageOptions.Uri can't be relative path.");
            
            var relativePath = string.Join("/", RelativePaths);
            if (!string.IsNullOrEmpty(uri.AbsolutePath))
                relativePath = string.IsNullOrEmpty(relativePath)
                    ? uri.AbsolutePath
                    : uri.AbsolutePath + "/" + relativePath;

            Message.RequestUri = new UriBuilder(uri)
            {
                Path = new Regex(@"\/\/+").Replace(relativePath, "/"),
                Query = Query.ToString()
            }.Uri;
            return Message;
        }
    }

    public class CannotBuildMessageException : Exception
    {
        public CannotBuildMessageException(string? message) : base(message)
        {
        }
    }
    
    
    public interface IHttpContentBuilder
    {
        HttpContent Build();
    }

    internal class FormUrlEncodedContentBuilder : Dictionary<string, string>, IHttpContentBuilder
    {
        public HttpContent Build()
        {
            return new FormUrlEncodedContent(this);
        }
    }

    internal class MultipartFormDataContentBuilder : IHttpContentBuilder
    {
        private readonly List<(HttpContent content, string? name, string? fileName)> _contents = new();
        
        public void Add(HttpContent content)
        {
            _contents.Add((content, null, null));
        }

        public void Add(HttpContent content, string name)
        {
            _contents.Add((content, name, null));
        }

        public void Add(HttpContent content, string name, string fileName)
        {
            _contents.Add((content, name, fileName));
        }

        public HttpContent Build()
        {
            var formDataContent = new MultipartFormDataContent();
            foreach (var tuple in _contents)
            {
                switch (tuple)
                {
                    case (var content, null, null):
                        formDataContent.Add(content);
                        break;
                    case (var content, var name, null):
                        formDataContent.Add(content, name);
                        break;
                    case var (content, name, fileName):
                        formDataContent.Add(content, name, fileName);
                        break;
                }
            }
            return formDataContent;
        }
    }
    internal class StringContentBuilder : IHttpContentBuilder
    {
        private readonly string _text;
        private readonly string _mediaType;
        private readonly Encoding _encoding;

        public StringContentBuilder(string text, string mediaType, Encoding encoding)
        {
            _text = text;
            _mediaType = mediaType;
            _encoding = encoding;
        }

        public HttpContent Build()
        {
            return new StringContent(_text, _encoding, _mediaType);
        }
    }
}