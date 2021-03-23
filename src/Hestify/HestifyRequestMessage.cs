#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        public IHttpContentBuilder? ContentBuilder { get; set; }
        
        internal HttpRequestMessage BuildMessage()
        {
            if (ContentBuilder != default)
                Message.Content = ContentBuilder.Build();

            Message.RequestUri = new UriBuilder(Message.RequestUri ?? throw new CannotBuildMessageException("Should request meesage builder have base or relative url for requesting"))
            {
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